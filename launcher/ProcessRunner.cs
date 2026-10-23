using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Relay.Launcher;

internal static class ProcessRunner
{
    private static readonly Regex Ansi = new(
        "\u001B\\[[0-9;?]*[ -/]*[@-~]",
        RegexOptions.Compiled);

    public static async Task<int> RunAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        Action<string> onLine,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        startInfo.Environment["NO_COLOR"] = "1";
        startInfo.Environment["FORCE_COLOR"] = "0";
        startInfo.Environment["CARGO_TERM_COLOR"] = "never";
        startInfo.Environment["CARGO_BUILD_JOBS"] = "2";
        startInfo.Environment["CARGO_INCREMENTAL"] = "0";

        using var process = new Process { StartInfo = startInfo };
        if (!process.Start())
        {
            throw new LauncherException("Could not start " + Path.GetFileName(fileName) + ".");
        }

        using var registration = cancellationToken.Register(() =>
        {
            try
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
            }
            catch
            {
                // The process may already be gone.
            }
        });

        var output = PumpAsync(process.StandardOutput, onLine, cancellationToken);
        var error = PumpAsync(process.StandardError, onLine, cancellationToken);
        await Task.WhenAll(output, error);
        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }

    private static async Task PumpAsync(StreamReader reader, Action<string> onLine, CancellationToken cancellationToken)
    {
        var buffer = new char[2048];
        var line = new StringBuilder();

        void Flush()
        {
            if (line.Length == 0) return;
            var text = Ansi.Replace(line.ToString(), string.Empty)
                .Replace("\b", string.Empty)
                .TrimEnd();
            line.Clear();
            if (text.Length > 0) onLine(text);
        }

        while (true)
        {
            int read;
            try
            {
                read = await reader.ReadAsync(buffer.AsMemory(), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (IOException)
            {
                break;
            }

            if (read == 0) break;

            for (var index = 0; index < read; index++)
            {
                var character = buffer[index];
                if (character is '\r' or '\n')
                {
                    Flush();
                }
                else
                {
                    line.Append(character);
                }
            }

            if (line.Length > 4096) Flush();
        }

        Flush();
    }
}
