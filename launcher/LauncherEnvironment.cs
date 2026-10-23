using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace Relay.Launcher;

internal sealed class LauncherException : Exception
{
    public LauncherException(string message) : base(message) { }
}

internal sealed class LauncherEnvironment
{
    public required string Node { get; init; }
    public required string Npm { get; init; }
    public required string Cargo { get; init; }
    public required string VcVars { get; init; }

    public static LauncherEnvironment Detect()
    {
        var node = FindOnPath("node.exe") ?? Existing(@"C:\Program Files\nodejs\node.exe");
        if (node is null)
        {
            throw new LauncherException(
                "Node.js 18 or newer is required. Install it from nodejs.org, then open Relay again.");
        }

        var npm = FindOnPath("npm.cmd") ?? Existing(Path.Combine(Path.GetDirectoryName(node)!, "npm.cmd"));
        if (npm is null)
        {
            throw new LauncherException(
                "npm was not found next to Node.js. Reinstall Node.js 18 or newer, then try again.");
        }

        var cargo = FindOnPath("cargo.exe") ?? Existing(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cargo", "bin", "cargo.exe"));
        if (cargo is null)
        {
            throw new LauncherException(
                "Rust is required to build the desktop engine. Install it from rustup.rs, then open Relay again.");
        }

        var vcVars = FindVcVars();
        if (vcVars is null)
        {
            throw new LauncherException(
                "Visual Studio C++ build tools are required. Install Visual Studio Build Tools with the " +
                "\"Desktop development with C++\" workload, then open Relay again.");
        }

        if (!HasWebView2())
        {
            throw new LauncherException(
                "Microsoft Edge WebView2 is required to run Relay. Install the Evergreen WebView2 Runtime " +
                "from Microsoft, then open Relay again.");
        }

        return new LauncherEnvironment
        {
            Node = node,
            Npm = npm,
            Cargo = cargo,
            VcVars = vcVars,
        };
    }

    private static string? Existing(string path) => File.Exists(path) ? path : null;

    private static string? FindOnPath(string fileName)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var entry in path.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var directory = entry.Trim().Trim('"');
                if (directory.Length == 0) continue;
                var candidate = Path.Combine(directory, fileName);
                if (File.Exists(candidate)) return candidate;
            }
            catch
            {
                // A malformed PATH entry should not stop the search.
            }
        }
        return null;
    }

    private static string? FindVcVars()
    {
        var vswhere = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Microsoft Visual Studio", "Installer", "vswhere.exe");

        if (File.Exists(vswhere))
        {
            try
            {
                var startInfo = new ProcessStartInfo(vswhere)
                {
                    Arguments = "-latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                };
                using var process = Process.Start(startInfo);
                if (process is not null)
                {
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(5000);
                    var install = output.Split('\n')
                        .Select(line => line.Trim())
                        .FirstOrDefault(line => line.Length > 0);
                    if (install is not null)
                    {
                        var candidate = Path.Combine(install, "VC", "Auxiliary", "Build", "vcvars64.bat");
                        if (File.Exists(candidate)) return candidate;
                    }
                }
            }
            catch
            {
                // Fall through to the known install locations below.
            }
        }

        string[] knownRoots =
        {
            @"C:\Program Files\Microsoft Visual Studio\2022\Community",
            @"C:\Program Files\Microsoft Visual Studio\2022\BuildTools",
            @"C:\Program Files\Microsoft Visual Studio\2022\Professional",
            @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise",
            @"C:\Program Files\Microsoft Visual Studio\18\Community",
            @"C:\Program Files\Microsoft Visual Studio\18\BuildTools",
        };

        foreach (var root in knownRoots)
        {
            var candidate = Path.Combine(root, "VC", "Auxiliary", "Build", "vcvars64.bat");
            if (File.Exists(candidate)) return candidate;
        }

        return null;
    }

    private static bool HasWebView2()
    {
        const string client = @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}";
        foreach (var hive in new[] { RegistryHive.LocalMachine, RegistryHive.CurrentUser })
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                using var key = baseKey.OpenSubKey(client);
                var version = key?.GetValue("pv") as string;
                if (!string.IsNullOrWhiteSpace(version) && version != "0.0.0.0") return true;
            }
            catch
            {
                // Try the next location.
            }
        }

        var evergreen = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Microsoft", "EdgeWebView", "Application");
        try
        {
            return Directory.Exists(evergreen) &&
                   Directory.EnumerateDirectories(evergreen)
                       .Any(path => File.Exists(Path.Combine(path, "msedgewebview2.exe")));
        }
        catch
        {
            return false;
        }
    }
}
