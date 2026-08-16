using IdeaMaker.Services;
using System.Windows;

namespace IdeaMaker.Dialogs
{
    public partial class SettingsDialog : Window
    {
        private readonly SettingsService _settings;
        private readonly DeepSeekService _deepseek;

        public SettingsDialog(SettingsService settings, DeepSeekService deepseek)
        {
            InitializeComponent();
            _settings = settings;
            _deepseek = deepseek;
            ApiKeyBox.Password = _settings.ApiKey;
            PromptBox.Text = _settings.SystemPrompt;
            DefaultPromptBox.Text = "你是一个创意任务生成器。当用户给出一个主题时，你需要生成一个具体、可执行、有挑战性的创意任务。任务描述要简洁（控制在150字以内），包含明确目标和执行方向，结合当前热点和趋势。最后给出难度等级（1-5级，1最简单，5最难），格式为：难度：X";
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void EditApiKey_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ApiKeyDialog(_settings, _deepseek) { Owner = this };
            dlg.ShowDialog();
            ApiKeyBox.Password = _settings.ApiKey;
        }

        private void CopyDefault_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(DefaultPromptBox.Text);
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确定恢复默认提示词？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                PromptBox.Text = string.Empty;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _settings.SystemPrompt = PromptBox.Text.Trim();
            MessageBox.Show("设置已保存", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }
    }
}
