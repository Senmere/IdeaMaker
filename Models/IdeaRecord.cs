using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IdeaMaker.Models
{
    public class IdeaTask
    {
        public string Id { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Difficulty { get; set; }
        public DateTime CreatedAt { get; set; }

        public IdeaTask()
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 8);
            CreatedAt = DateTime.Now;
        }
    }

    public class TrashTask
    {
        public string Id { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Difficulty { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int PointsDeducted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime TrashedAt { get; set; }

        public TrashTask()
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 8);
            TrashedAt = DateTime.Now;
        }
    }

    public class AppSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SystemPrompt { get; set; } = string.Empty;
        public int Points { get; set; }
        public int CompletedCount { get; set; }
        public List<IdeaTask> History { get; set; } = new List<IdeaTask>();
        public List<TrashTask> TrashTasks { get; set; } = new List<TrashTask>();
    }

    public class DeepSeekRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = "deepseek-chat";

        [JsonProperty("messages")]
        public List<Message> Messages { get; set; } = new List<Message>();

        [JsonProperty("stream")]
        public bool Stream { get; set; } = true;

        [JsonProperty("temperature")]
        public double Temperature { get; set; } = 1.0;

        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; } = 2000;
    }

    public class Message
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;
    }

    public class DeepSeekResponse
    {
        [JsonProperty("choices")]
        public List<Choice>? Choices { get; set; }
    }

    public class Choice
    {
        [JsonProperty("delta")]
        public Delta? Delta { get; set; }
    }

    public class Delta
    {
        [JsonProperty("content")]
        public string? Content { get; set; }
    }
}
