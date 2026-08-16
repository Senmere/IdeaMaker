using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace IdeaMaker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static readonly string ErrorLogPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");

    public App()
    {
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogAndShow(e.Exception, "Dispatcher");
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogAndShow(e.ExceptionObject as System.Exception, "AppDomain");
    }

    private static void LogAndShow(System.Exception? ex, string source)
    {
        var msg = $"[{source}] {ex}";
        try { File.WriteAllText(ErrorLogPath, msg); } catch { }
        MessageBox.Show(msg, "IdeaMaker 异常", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
