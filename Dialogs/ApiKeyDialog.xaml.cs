using IdeaMaker.Services;
using System.Windows;
using System.Windows.Media;

namespace IdeaMaker.Dialogs
{
    public partial class ApiKeyDialog : Window
    {
        private readonly SettingsService _settings;
        private readonly DeepSeekService _deepseek;

        public ApiKeyDialog(SettingsService settings, DeepSeekService deepseek)
        {
            InitializeComponent();
            _settings = settings;
            _deepseek = deepseek;
            ApiKeyBox.Password = _settings.ApiKey;
            CancelBtn.Visibility = string.IsNullOrEmpty(_settings.ApiKey) ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void ValidateBtn_Click(object sender, RoutedEventArgs e)
        {
            var key = ApiKeyBox.Password.Trim();
            if (string.IsNullOrEmpty(key)) return;

            ValidateBtn.IsEnabled = false;
            ValidateBtn.Content = "验证中...";
            StatusText.Visibility = Visibility.Collapsed;

            var ok = await _deepseek.ValidateApiKey(key);

            StatusText.Visibility = Visibility.Visible;
            if (ok)
            {
                StatusText.Text = "API Key 验证通过";
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94));
            }
            else
            {
                StatusText.Text = "API Key 无效，请检查后重试";
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(248, 113, 113));
            }

            ValidateBtn.IsEnabled = true;
            ValidateBtn.Content = "验证";
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var key = ApiKeyBox.Password.Trim();
            if (string.IsNullOrEmpty(key)) return;
            _settings.ApiKey = key;
            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
