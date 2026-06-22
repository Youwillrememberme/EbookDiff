using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace EbookDiff;

/// <summary>
/// 应用程序入口。负责捕获未处理异常并写入日志文件，避免软件无声崩溃。
/// </summary>
public partial class App : Application
{
    private static readonly string LogDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EbookDiff");

    private static readonly string LogPath = Path.Combine(LogDir, "crash.log");

    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        base.OnStartup(e);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        WriteLog(e.Exception);
        MessageBox.Show(
            $"发生未预期的错误。详细信息已写入：\n{LogPath}\n\n{e.Exception.Message}",
            "EbookDiff",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            WriteLog(ex);
    }

    private static void WriteLog(Exception ex)
    {
        try
        {
            Directory.CreateDirectory(LogDir);
            File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}\n\n");
        }
        catch
        {
            // 日志写不进去也别再抛了，避免无限递归
        }
    }
}
