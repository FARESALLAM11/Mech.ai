using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Relay.Launcher;

internal static class Native
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string? className, string windowName);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int command);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    public static void ActivateWindow(string title)
    {
        var handle = FindWindow(null, title);
        if (handle == IntPtr.Zero) return;
        ShowWindow(handle, 9);
        SetForegroundWindow(handle);
    }

    public static void EnableRoundedCorners(IntPtr handle)
    {
        try
        {
            var preference = 2; // DWMWCP_ROUND
            DwmSetWindowAttribute(handle, 33, ref preference, sizeof(int));
        }
        catch
        {
            // Older Windows builds simply keep square corners.
        }
    }

    public static Process? FindRunningApp()
    {
        foreach (var name in new[] { "Relay-core", "Relay", "Relay" })
        {
            try
            {
                var match = Process.GetProcessesByName(name)
                    .FirstOrDefault(process => process.MainWindowHandle != IntPtr.Zero);
                if (match is not null) return match;
            }
            catch
            {
                // Ignore processes we cannot inspect.
            }
        }
        return null;
    }

    public static void FocusApp(Process process)
    {
        var handle = process.MainWindowHandle;
        if (handle == IntPtr.Zero) return;
        ShowWindow(handle, 9);
        SetForegroundWindow(handle);
    }
}
