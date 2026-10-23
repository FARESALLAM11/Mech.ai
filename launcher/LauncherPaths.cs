using System;
using System.IO;

namespace Relay.Launcher;

internal sealed class LauncherPaths
{
    public string Root { get; }
    public string LauncherDir { get; }
    public string LogFile { get; }
    public string TauriCli { get; }
    public string TargetReleaseDir { get; }

    private LauncherPaths(string root, string launcherDir)
    {
        Root = root;
        LauncherDir = launcherDir;
        LogFile = Path.Combine(launcherDir, "last-launch.log");
        TauriCli = Path.Combine(root, "node_modules", ".bin", "tauri.cmd");
        TargetReleaseDir = Path.Combine(root, "src-tauri", "target", "release");
    }

    public static LauncherPaths Discover()
    {
        var executable = Environment.ProcessPath;
        var start = executable is null ? AppContext.BaseDirectory : Path.GetDirectoryName(executable)!;
        var directory = new DirectoryInfo(start);

        for (var depth = 0; depth < 5 && directory is not null; depth++, directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src-tauri", "tauri.conf.json")))
            {
                return new LauncherPaths(directory.FullName, start);
            }
        }

        return new LauncherPaths(start, start);
    }

    public string? FindAppExe()
    {
        string[] candidates =
        {
            Path.Combine(TargetReleaseDir, "relay-core.exe"), Path.Combine(TargetReleaseDir, "mech-core.exe"),
            Path.Combine(TargetReleaseDir, "Relay.exe"),
            Path.Combine(TargetReleaseDir, "Relay.exe"),
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate)) return candidate;
        }

        return null;
    }
}
