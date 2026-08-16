using IdeaMaker.Dialogs;
using IdeaMaker.Models;
using IdeaMaker.Services;
using IdeaMaker.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IdeaMaker
{
    public partial class MainWindow : Window
    {
        private readonly SettingsService _settings = new();
        private readonly DeepSeekService _deepseek = new();
        private IdeaTask? _currentTask;
        private bool _isGenerating;
        private string _currentTopic = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            _settings.PointsChanged += (_, _) => UpdateStats();
            UpdateStats();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_settings.ApiKey))
                ShowApiKeyModal();
        }

        private void UpdateStats()
        {
            PointsText.Text = _settings.Points.ToString();
            CompletedText.Text = _settings.CompletedCount.ToString();
        }

        private void SetDifficulty(int level)
        {
            DifficultyText.Text = $"{level} / 5";
            DifficultyStars.Children.Clear();
            for (int i = 0; i < 5; i++)
            {
                var star = new TextBlock
                {
                    Text = "★",
                    FontSize = 14,
                    Foreground = i < level
                        ? new SolidColorBrush(Color.FromRgb(251, 191, 36))
                        : new SolidColorBrush(Color.FromRgb(68, 68, 68)),
                    Margin = new Thickness(1, 0, 1, 0),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                DifficultyStars.Children.Add(star);
            }
        }

        private async void GenerateBtn_Click(object sender, RoutedEventArgs e)
        {
            var topic = TopicBox.Text.Trim();
            if (string.IsNullOrEmpty(topic))
            {
                MessageBox.Show("请输入主题关键词", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrEmpty(_settings.ApiKey))
            {
                ShowApiKeyModal();
                return;
            }

            if (_isGenerating) return;

            _currentTopic = topic;
            _isGenerating = true;
            GenerateBtn.IsEnabled = false;
            GenerateBtn.Content = "任务生成中...";
            TaskCard.Visibility = Visibility.Collapsed;
            ActionPanel.Visibility = Visibility.Collapsed;
            GenStatusCard.Visibility = Visibility.Visible;
            GenText.Text = "";
            GenStatus.Text = "任务生成中...";
            GenStatus.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94));

            var buf = new System.Text.StringBuilder();

            await _deepseek.StreamGenerate(
                _settings.ApiKey, topic, _settings.SystemPrompt,
                chunk =>
                {
                    buf.Append(chunk);
                    GenText.Text = buf.ToString();
                },
                (fullText, difficulty) =>
                {
                    _isGenerating = false;
                    GenerateBtn.IsEnabled = true;
                    GenerateBtn.Content = "生成任务";

                    _currentTask = new IdeaTask
                    {
                        Topic = topic,
                        Description = fullText,
                        Difficulty = difficulty
                    };

                    ShowTask(_currentTask);
                },
                err =>
                {
                    _isGenerating = false;
                    GenerateBtn.IsEnabled = true;
                    GenerateBtn.Content = "生成任务";
                    buf.Append($"\n\n错误: {err}");
                    GenText.Text = buf.ToString();
                    GenStatus.Text = "生成失败";
                    GenStatus.Foreground = new SolidColorBrush(Color.FromRgb(248, 113, 113));

                    if (err.Contains("API Key"))
                    {
                        MessageBox.Show(err, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        ShowApiKeyModal();
                    }
                }
            );
        }

        private void ShowTask(IdeaTask task)
        {
            GenStatusCard.Visibility = Visibility.Collapsed;
            TaskCard.Visibility = Visibility.Visible;
            ActionPanel.Visibility = Visibility.Visible;
            TopicLabel.Text = $"# {task.Topic}";
            TaskDesc.Text = task.Description;
            SetDifficulty(task.Difficulty);
        }

        private void CompleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTask == null) return;

            var result = MessageBox.Show($"确认完成此任务？将获得 {_currentTask.Difficulty} 积分！",
                "完成任务", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            _settings.Points += _currentTask.Difficulty;
            _settings.CompletedCount += 1;
            _settings.AddHistory(_currentTask);

            MessageBox.Show($"任务完成！获得 {_currentTask.Difficulty} 积分！\n当前积分: {_settings.Points}",
                "任务完成", MessageBoxButton.OK, MessageBoxImage.Information);

            ResetTaskView();
        }

        private void FailBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTask == null) return;

            var dlg = new FailReasonDialog(_currentTask.Difficulty) { Owner = this };
            if (dlg.ShowDialog() == true && !string.IsNullOrEmpty(dlg.Reason))
            {
                var deducted = dlg.DeductedPoints;
                var trash = new TrashTask
                {
                    Topic = _currentTask.Topic,
                    Description = _currentTask.Description,
                    Difficulty = _currentTask.Difficulty,
                    Reason = dlg.Reason,
                    PointsDeducted = deducted,
                    CreatedAt = _currentTask.CreatedAt
                };

                _settings.Points -= deducted;
                if (_settings.Points < 0) _settings.Points = 0;
                _settings.AddTrash(trash);

                MessageBox.Show($"已扣除 {deducted} 积分，任务进入垃圾桶。\n下次完成可获得补偿积分。",
                    "任务未完成", MessageBoxButton.OK, MessageBoxImage.Information);

                ResetTaskView();
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isGenerating) return;
            if (string.IsNullOrWhiteSpace(_currentTopic))
            {
                _currentTopic = TopicBox.Text.Trim();
            }
            if (string.IsNullOrEmpty(_currentTopic)) return;

            var result = MessageBox.Show("换一个任务将覆盖当前任务，确定吗？",
                "更换任务", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            TopicBox.Text = _currentTopic;
            GenerateBtn_Click(sender, e);
        }

        private void ResetTaskView()
        {
            _currentTask = null;
            TaskCard.Visibility = Visibility.Collapsed;
            ActionPanel.Visibility = Visibility.Collapsed;
            GenStatusCard.Visibility = Visibility.Collapsed;
            TopicBox.Text = "";
            _currentTopic = string.Empty;
        }

        private void HistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            var w = new HistoryWindow(_settings);
            w.TaskSelected += (_, task) =>
            {
                _currentTask = task;
                _currentTopic = task.Topic;
                TopicBox.Text = task.Topic;
                ShowTask(task);
            };
            w.Owner = this;
            w.ShowDialog();
        }

        private void TrashBtn_Click(object sender, RoutedEventArgs e)
        {
            var w = new TrashWindow(_settings);
            w.TaskRestored += (_, task) =>
            {
                var compensation = task.PointsDeducted + (int)(task.Difficulty * 0.8);
                _settings.Points += compensation;
                _settings.CompletedCount += 1;
                _settings.RemoveTrash(task.Id);

                var historyTask = new IdeaTask
                {
                    Topic = task.Topic,
                    Description = task.Description,
                    Difficulty = task.Difficulty,
                    CreatedAt = task.CreatedAt
                };
                _settings.AddHistory(historyTask);

                MessageBox.Show($"补偿 {compensation} 积分（已扣分 + 难度×0.8）！\n当前积分: {_settings.Points}",
                    "任务完成", MessageBoxButton.OK, MessageBoxImage.Information);
            };
            w.Owner = this;
            w.ShowDialog();
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SettingsDialog(_settings, _deepseek) { Owner = this };
            dlg.ShowDialog();
        }

        private void ShowApiKeyModal()
        {
            var dlg = new ApiKeyDialog(_settings, _deepseek) { Owner = this };
            dlg.ShowDialog();
        }
    }
}
