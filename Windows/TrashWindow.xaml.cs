using IdeaMaker.Models;
using IdeaMaker.Services;
using System.Windows;
using System.Windows.Controls;

namespace IdeaMaker.Windows
{
    public partial class TrashWindow : Window
    {
        private readonly SettingsService _settings;
        public event EventHandler<TrashTask>? TaskRestored;

        public TrashWindow(SettingsService settings)
        {
            InitializeComponent();
            _settings = settings;
            TrashList.ItemsSource = _settings.TrashTasks;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void TrashList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = TrashList.SelectedItem as TrashTask;
            RestoreBtn.IsEnabled = selected != null;
            ViewBtn.IsEnabled = selected != null;
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            if (TrashList.SelectedItem is not TrashTask task) return;

            var compensation = task.PointsDeducted + (int)(task.Difficulty * 0.8);
            var result = MessageBox.Show(
                $"确认完成此任务？\n\n将获得补偿积分: +{compensation}\n（已扣分 {task.PointsDeducted} + 难度×0.8 {(int)(task.Difficulty * 0.8)}）",
                "稍后完成", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            TaskRestored?.Invoke(this, task);
            TrashList.ItemsSource = _settings.TrashTasks;
            RestoreBtn.IsEnabled = false;
            ViewBtn.IsEnabled = false;
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            if (TrashList.SelectedItem is not TrashTask task) return;

            var detail = $"主题: {task.Topic}\n\n" +
                         $"难度: {task.Difficulty} / 5\n\n" +
                         $"任务描述:\n{task.Description}\n\n" +
                         $"未完成原因: {task.Reason}\n\n" +
                         $"已扣积分: {task.PointsDeducted}\n" +
                         $"进入垃圾桶时间: {task.TrashedAt:yyyy-MM-dd HH:mm}";

            MessageBox.Show(detail, "任务详情", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
