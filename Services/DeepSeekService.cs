using IdeaMaker.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IdeaMaker.Services
{
    public class DeepSeekService
    {
        private const string ApiUrl = "https://api.deepseek.com/chat/completions";
        private const string DefaultSystemPrompt = "你是一个创意任务生成器。当用户给出一个主题时，你需要生成一个具体、可执行、有挑战性的创意任务。任务描述要简洁（控制在150字以内），包含明确目标和执行方向，结合当前热点和趋势。最后给出难度等级（1-5级，1最简单，5最难），格式为：难度：X";

        public async Task<bool> ValidateApiKey(string apiKey)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var request = new DeepSeekRequest
                {
                    Messages = new List<Message>
                    {
                        new Message { Role = "user", Content = "Hi" }
                    },
                    Stream = false,
                    MaxTokens = 1
                };

                var response = await client.PostAsync(ApiUrl, new StringContent(
                    JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task StreamGenerate(string apiKey, string topic, string customPrompt,
            Action<string> onChunk, Action<string, int> onComplete, Action<string> onError)
        {
            var systemPrompt = string.IsNullOrWhiteSpace(customPrompt) ? DefaultSystemPrompt : customPrompt;

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var request = new DeepSeekRequest
                {
                    Messages = new List<Message>
                    {
                        new Message { Role = "system", Content = systemPrompt },
                        new Message { Role = "user", Content = $"请为以下主题生成一个创意任务：{topic}" }
                    },
                    MaxTokens = 800
                };

                var response = await client.PostAsync(ApiUrl, new StringContent(
                    JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var msg = response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        ? "API Key 无效或已过期，请重新设置"
                        : $"请求失败 ({(int)response.StatusCode})，请稍后重试";
                    onError(msg);
                    return;
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);

                var fullText = new StringBuilder();
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var trimmed = line.Trim();
                    if (!trimmed.StartsWith("data:")) continue;

                    var data = trimmed.Substring(5).Trim();
                    if (data == "[DONE]")
                        break;

                    try
                    {
                        var json = JsonConvert.DeserializeObject<DeepSeekResponse>(data);
                        var content = json?.Choices?.Count > 0 ? json.Choices[0]?.Delta?.Content : null;
                        if (content != null)
                        {
                            fullText.Append(content);
                            onChunk(content);
                        }
                    }
                    catch
                    {
                    }
                }

                var finalText = fullText.ToString();
                var difficulty = ExtractDifficulty(finalText);
                onComplete(finalText, difficulty);
            }
            catch (Exception ex)
            {
                onError(ex.Message);
            }
        }

        private static int ExtractDifficulty(string text)
        {
            var patterns = new[]
            {
                @"难度[：:]\s*(\d)",
                @"难度等级[：:]\s*(\d)",
                @"难度[：:]\s*(\d)\s*级",
                @"难度等级[：:]\s*(\d)\s*级",
                @"(\d)\s*级难度",
                @"难度为\s*(\d)",
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var level))
                {
                    return Math.Clamp(level, 1, 5);
                }
            }

            var starMatch = Regex.Matches(text, @"[★⭐✦]");
            if (starMatch.Count > 0)
                return Math.Clamp(starMatch.Count, 1, 5);

            return 3;
        }
    }
}
