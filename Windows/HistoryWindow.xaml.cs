using IdeaMaker.Models;
using IdeaMaker.Services;
using System.Windows;
using System.Windows.Controls;

namespace IdeaMaker.Windows
{
    public partial class HistoryWindow : Window
    {
        private readonly SettingsService _settings;
        public event EventHandler<IdeaTask>? TaskSelected;

        public HistoryWindow(SettingsService settings)
        {
            InitializeComponent();
            _settings = settings;
            HistoryList.ItemsSource = _settings.History;
            ClearBtn.Visibility = _settings.History.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryList.SelectedItem is IdeaTask task)
            {
                TaskSelected?.Invoke(this, task);
                Close();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定清空所有历史记录？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _settings.ClearHistory();
                HistoryList.ItemsSource = _settings.History;
                ClearBtn.Visibility = Visibility.Collapsed;
            }
        }
    }
}
