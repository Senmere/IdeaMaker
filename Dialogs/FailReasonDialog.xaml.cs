using System.Windows;

namespace IdeaMaker.Dialogs
{
    public partial class FailReasonDialog : Window
    {
        private readonly int _difficulty;

        public string Reason { get; private set; } = string.Empty;
        public int DeductedPoints { get; private set; }

        public FailReasonDialog(int difficulty)
        {
            InitializeComponent();
            _difficulty = difficulty;
            DeductedPoints = CalculateDeduction(string.Empty);
            UpdateDeductText();
        }

        private int CalculateDeduction(string reason)
        {
            var baseDeduct = 1;
            var len = reason?.Trim().Length ?? 0;

            if (len > 50) baseDeduct += 1;
            if (len > 100) baseDeduct += 1;
            if (len > 200) baseDeduct += 1;

            var reasonLower = (reason ?? string.Empty).ToLower();
            var lazyKeywords = new[] { "懒得", "不想", "算了", "放弃", "太麻烦", "不想做", "没兴趣", "无聊" };
            foreach (var kw in lazyKeywords)
            {
                if (reasonLower.Contains(kw))
                {
                    baseDeduct += 1;
                    break;
                }
            }

            var maxDeduct = _difficulty + 2;
            return System.Math.Clamp(baseDeduct, 1, maxDeduct);
        }

        private void ReasonBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            DeductedPoints = CalculateDeduction(ReasonBox.Text);
            UpdateDeductText();
        }

        private void UpdateDeductText()
        {
            DeductText.Content = DeductedPoints.ToString();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            var reason = ReasonBox.Text.Trim();
            if (string.IsNullOrEmpty(reason))
            {
                MessageBox.Show("请输入未完成的原因", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Reason = reason;
            DialogResult = true;
            Close();
        }
    }
}
