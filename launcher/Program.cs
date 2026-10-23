using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Relay.Launcher;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        try
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        }
        catch
        {
            // The runtime may already be DPI-aware.
        }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Application.ThreadException += (_, eventArgs) => ReportCrash(eventArgs.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
            ReportCrash(eventArgs.ExceptionObject as Exception);

        using var mutex = new Mutex(true, @"Local\Relay.Launcher.Window", out var isFirst);
        if (!isFirst)
        {
            Native.ActivateWindow("Relay Launcher");
            return;
        }

        try
        {
            Application.Run(new LauncherForm(args));
        }
        catch (Exception ex)
        {
            ReportCrash(ex);
        }
    }

    private static void ReportCrash(Exception? exception)
    {
        if (exception is null) return;
        try
        {
            var log = Path.Combine(Path.GetTempPath(), "Relay-launcher-crash.log");
            File.AppendAllText(log, $"{DateTime.Now:O}{Environment.NewLine}{exception}{Environment.NewLine}");
            MessageBox.Show(
                "The Relay launcher hit a problem." + Environment.NewLine + Environment.NewLine +
                exception.Message + Environment.NewLine + Environment.NewLine +
                "A crash report was written to:" + Environment.NewLine + log,
                "Relay",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch
        {
            // Nothing else we can do without a UI.
        }
    }
}
