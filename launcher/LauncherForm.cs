using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Relay.Launcher;

internal sealed class LauncherForm : Form
{
    private static readonly Color Ink = Color.FromArgb(31, 31, 31);
    private static readonly Color Muted = Color.FromArgb(95, 99, 104);
    private static readonly Color Border = Color.FromArgb(232, 234, 237);
    private static readonly Color BorderStrong = Color.FromArgb(218, 220, 224);
    private static readonly Color HeaderBg = Color.FromArgb(248, 249, 250);
    private static readonly Color Subtle = Color.FromArgb(241, 243, 244);
    private static readonly Color Blue = Color.FromArgb(26, 115, 232);
    private static readonly Color BlueHover = Color.FromArgb(23, 101, 204);
    private static readonly Color Success = Color.FromArgb(30, 142, 62);
    private static readonly Color Danger = Color.FromArgb(217, 48, 37);
    private static readonly Color Track = Color.FromArgb(232, 234, 237);
    private static readonly Color LogBg = Color.FromArgb(32, 33, 36);
    private static readonly Color LogInk = Color.FromArgb(232, 234, 237);

    private const int BaseWidth = 640;
    private const int BaseHeight = 320;
    private const int ExpandedHeight = 540;

    private readonly string[] startupArgs;
    private readonly LauncherPaths paths = LauncherPaths.Discover();
    private readonly Label stageLabel = new();
    private readonly Label detailLabel = new();
    private readonly Label elapsedLabel = new();
    private readonly Label footerLabel = new();
    private readonly Label logTitle = new();
    private readonly Panel logHost = new();
    private readonly RichTextBox logBox = new();
    private readonly RoundedProgress progress = new();
    private readonly RoundedButton primaryButton = new();
    private readonly RoundedButton secondaryButton = new();
    private readonly LinkLabel detailsLink = new();
    private readonly LinkLabel openLogLink = new();
    private readonly System.Windows.Forms.Timer uiTimer = new() { Interval = 500 };
    private readonly Stopwatch clock = new();

    private CancellationTokenSource? runCts;
    private RunLog? runLog;
    private bool busy;
    private bool detailsOpen;
    private bool launched;
    private bool hasError;
    private string lastStage = string.Empty;
    private DateTime lastStageAt = DateTime.MinValue;

    public LauncherForm(string[] args)
    {
        startupArgs = args;
        BuildUi();
        Shown += OnShown;
        FormClosing += OnFormClosing;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ApplyWindowSize();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        Native.EnableRoundedCorners(Handle);
    }

    private void BuildUi()
    {
        SuspendLayout();

        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96f, 96f);
        ClientSize = new Size(BaseWidth, BaseHeight);
        MinimumSize = new Size(BaseWidth, BaseHeight);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = true;
        BackColor = Color.White;
        Text = "Relay Launcher";
        Icon = LoadAppIcon();
        DoubleBuffered = true;

        var header = new Panel
        {
            Bounds = new Rectangle(0, 0, BaseWidth, 98),
            BackColor = HeaderBg,
        };
        Controls.Add(header);

        var logo = new PictureBox
        {
            Bounds = new Rectangle(30, 21, 58, 58),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent,
            Image = LoadLogo(),
        };
        header.Controls.Add(logo);

        header.Controls.Add(MakeLabel("Relay", new Rectangle(102, 18, 320, 36), 20f, FontStyle.Bold, Ink));
        header.Controls.Add(MakeLabel("Agent workspace", new Rectangle(104, 52, 320, 22), 9.5f, FontStyle.Regular, Muted));
        header.Controls.Add(MakeLabel("v0.1.0", new Rectangle(500, 24, 110, 20), 8.5f, FontStyle.Regular, Muted, ContentAlignment.TopRight));
        header.Controls.Add(new Panel
        {
            Bounds = new Rectangle(0, 97, BaseWidth, 1),
            BackColor = Border,
        });

        stageLabel.Text = "Preparing Relay";
        stageLabel.Bounds = new Rectangle(30, 112, 450, 26);
        stageLabel.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
        stageLabel.ForeColor = Ink;
        stageLabel.BackColor = Color.Transparent;
        Controls.Add(stageLabel);

        elapsedLabel.Bounds = new Rectangle(500, 118, 110, 20);
        elapsedLabel.Font = new Font("Segoe UI", 8.5f);
        elapsedLabel.ForeColor = Muted;
        elapsedLabel.BackColor = Color.Transparent;
        elapsedLabel.TextAlign = ContentAlignment.TopRight;
        Controls.Add(elapsedLabel);

        detailLabel.Text = "Checking the app and its build tools.";
        detailLabel.Bounds = new Rectangle(30, 140, 580, 36);
        detailLabel.Font = new Font("Segoe UI", 9.25f);
        detailLabel.ForeColor = Muted;
        detailLabel.BackColor = Color.Transparent;
        detailLabel.AutoEllipsis = true;
        Controls.Add(detailLabel);

        progress.Bounds = new Rectangle(30, 184, 580, 6);
        progress.TrackColor = Track;
        progress.BarColor = Blue;
        Controls.Add(progress);

        ConfigureLink(detailsLink);
        detailsLink.Text = "Details";
        detailsLink.Bounds = new Rectangle(30, 202, 74, 24);
        detailsLink.LinkClicked += (_, _) => SetDetails(!detailsOpen);
        Controls.Add(detailsLink);

        ConfigureLink(openLogLink);
        openLogLink.Text = "Open log";
        openLogLink.Bounds = new Rectangle(108, 202, 90, 24);
        openLogLink.Visible = false;
        openLogLink.LinkClicked += (_, _) => OpenLog();
        Controls.Add(openLogLink);

        primaryButton.Text = "Launch Relay";
        primaryButton.Bounds = new Rectangle(430, 196, 180, 40);
        primaryButton.FillColor = Blue;
        primaryButton.HoverColor = BlueHover;
        primaryButton.Click += (_, _) => _ = RunFlowAsync(false);
        Controls.Add(primaryButton);

        secondaryButton.Text = "Rebuild";
        secondaryButton.Bounds = new Rectangle(320, 196, 100, 40);
        secondaryButton.FillColor = Color.White;
        secondaryButton.HoverColor = Subtle;
        secondaryButton.TextColor = Ink;
        secondaryButton.BorderColor = BorderStrong;
        secondaryButton.Click += OnSecondaryClick;
        Controls.Add(secondaryButton);

        footerLabel.Text = "Runs locally · Your project never leaves this PC";
        footerLabel.Bounds = new Rectangle(30, 266, 500, 20);
        footerLabel.Font = new Font("Segoe UI", 8.5f);
        footerLabel.ForeColor = Muted;
        footerLabel.BackColor = Color.Transparent;
        Controls.Add(footerLabel);

        logTitle.Text = "Build output";
        logTitle.Bounds = new Rectangle(30, 248, 300, 22);
        logTitle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        logTitle.ForeColor = Ink;
        logTitle.BackColor = Color.Transparent;
        logTitle.Visible = false;
        Controls.Add(logTitle);

        logHost.Bounds = new Rectangle(30, 272, 580, 232);
        logHost.BackColor = LogBg;
        logHost.Padding = new Padding(10, 8, 10, 8);
        logHost.Visible = false;
        Controls.Add(logHost);

        logBox.Dock = DockStyle.Fill;
        logBox.BorderStyle = BorderStyle.None;
        logBox.BackColor = LogBg;
        logBox.ForeColor = LogInk;
        logBox.Font = new Font("Cascadia Mono", 8.25f);
        logBox.ReadOnly = true;
        logBox.WordWrap = false;
        logBox.DetectUrls = false;
        logBox.ScrollBars = RichTextBoxScrollBars.Vertical;
        logHost.Controls.Add(logBox);

        uiTimer.Tick += (_, _) =>
        {
            if (!clock.IsRunning) return;
            elapsedLabel.Text = clock.Elapsed.TotalMinutes >= 1
                ? $"{(int)clock.Elapsed.TotalMinutes}:{clock.Elapsed.Seconds:00}"
                : $"{clock.Elapsed.TotalSeconds:0}s";
        };

        ResumeLayout(false);
        PerformLayout();
    }

    private async void OnShown(object? sender, EventArgs e)
    {
        if (startupArgs.Any(arg => arg.Equals("--no-launch", StringComparison.OrdinalIgnoreCase)))
        {
            SetStage("Ready", "Press Launch Relay to open your workspace.");
            Ui(() =>
            {
                progress.IsIndeterminate = false;
                progress.Value = 0;
            });
            return;
        }

        var forceRebuild = startupArgs.Any(arg =>
            arg.Equals("--rebuild", StringComparison.OrdinalIgnoreCase));
        await RunFlowAsync(forceRebuild);
    }

    private async Task RunFlowAsync(bool forceRebuild)
    {
        if (busy || launched) return;

        busy = true;
        hasError = false;
        runCts?.Dispose();
        runCts = new CancellationTokenSource();
        var token = runCts.Token;

        runLog?.Dispose();
        runLog = new RunLog(paths.LogFile);
        runLog.Start();
        lastStage = string.Empty;
        lastStageAt = DateTime.MinValue;

        Ui(() =>
        {
            logBox.Clear();
            progress.BarColor = Blue;
            progress.Value = 0;
            progress.IsIndeterminate = true;
            primaryButton.Text = "Working…";
            primaryButton.Enabled = false;
            secondaryButton.Text = "Cancel";
            secondaryButton.Enabled = true;
            openLogLink.Visible = false;
            elapsedLabel.Text = "0s";
            clock.Restart();
            uiTimer.Start();
        });

        try
        {
            SetStage("Checking your setup", "Looking for the desktop app and the tools needed to build it.");

            if (!forceRebuild)
            {
                var existing = paths.FindAppExe();
                if (existing is not null)
                {
                    await StartAppAsync(existing, token);
                    return;
                }
            }

            var environment = LauncherEnvironment.Detect();
            EnsureBuildSpace();
            AppendLog("node    " + environment.Node);
            AppendLog("npm     " + environment.Npm);
            AppendLog("cargo   " + environment.Cargo);
            AppendLog("vs      " + environment.VcVars);

            if (!File.Exists(paths.TauriCli))
            {
                await InstallDependenciesAsync(environment, token);
            }

            await BuildAsync(environment, token);

            var app = paths.FindAppExe();
            if (app is null)
            {
                throw new LauncherException(
                    "The build finished, but Relay's executable wasn't produced. Check the build output for details.");
            }

            await StartAppAsync(app, token);
        }
        catch (OperationCanceledException)
        {
            SetStage("Build cancelled", "You stopped the build. Try again whenever you're ready.");
            Ui(() =>
            {
                progress.IsIndeterminate = false;
                progress.Value = 0;
                elapsedLabel.Text = string.Empty;
            });
        }
        catch (LauncherException exception)
        {
            Fail(exception.Message);
        }
        catch (Exception exception)
        {
            AppendLog(exception.ToString());
            Fail("Something went wrong while starting Relay. " + exception.Message);
        }
        finally
        {
            busy = false;
            clock.Stop();
            uiTimer.Stop();
            if (!launched)
            {
                Ui(() =>
                {
                    progress.IsIndeterminate = false;
                    primaryButton.Text = hasError ? "Try again" : "Launch Relay";
                    primaryButton.Enabled = true;
                    secondaryButton.Text = "Rebuild";
                    secondaryButton.Enabled = true;
                });
            }
        }
    }

    private async Task InstallDependenciesAsync(LauncherEnvironment environment, CancellationToken token)
    {
        SetStage(
            "Installing dependencies",
            "Downloading the Node packages Relay needs. This only happens once.");
        AppendLog("> npm install");

        var arguments = "/d /s /c \"\"" + environment.Npm +
                        "\" install --no-audit --no-fund --progress=false\"";
        var exitCode = await ProcessRunner.RunAsync(
            "cmd.exe", arguments, paths.Root, HandleInstallLine, token);

        if (exitCode != 0)
        {
            throw new LauncherException(
                $"Installing dependencies failed (npm exited with code {exitCode}).");
        }

        if (!File.Exists(paths.TauriCli))
        {
            throw new LauncherException("Dependencies were installed, but the Tauri CLI is still missing.");
        }

        SetStage("Dependencies ready", "Starting the build…");
    }

    private async Task BuildAsync(LauncherEnvironment environment, CancellationToken token)
    {
        var runningApp = Native.FindRunningApp();
        if (runningApp is not null)
        {
            throw new LauncherException(
                "Relay is already running. Close its window, then press Rebuild again.");
        }

        SetStage(
            "Building Relay",
            "Compiling the Rust engine and packaging the interface. The first build can take a few minutes.");

        var command = "call \"" + environment.VcVars + "\" >nul && " +
                      "\"node_modules\\.bin\\tauri.cmd\" build --no-bundle";
        var arguments = "/d /s /c " + command;
        AppendLog("> tauri build --no-bundle");

        var exitCode = await ProcessRunner.RunAsync(
            "cmd.exe", arguments, paths.Root, HandleBuildLine, token);

        if (exitCode != 0)
        {
            throw new LauncherException(
                "The build didn't complete. Open the details below for the first error.");
        }

        SetStage("Build complete", "Relay is ready to launch.");
    }

    private void EnsureBuildSpace()
    {
        try
        {
            var root = Path.GetPathRoot(paths.Root);
            if (string.IsNullOrEmpty(root)) return;

            var drive = new DriveInfo(root);
            const long required = 4L * 1024 * 1024 * 1024;
            if (drive.AvailableFreeSpace >= required) return;

            var free = Math.Round(drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0, 1);
            throw new LauncherException(
                $"Relay needs at least 4 GB free on {drive.Name.TrimEnd('\\')} to build for the first time. " +
                $"Only {free} GB is available right now. Free up some space, then press Try again.");
        }
        catch (LauncherException)
        {
            throw;
        }
        catch
        {
            // If the drive cannot be queried, let the build report the real error.
        }
    }
    private async Task StartAppAsync(string executable, CancellationToken token)
    {
        var running = Native.FindRunningApp();
        if (running is not null)
        {
            Native.FocusApp(running);
            launched = true;
            Close();
            return;
        }

        SetStage("Starting Relay", "Opening your workspace…");
        Ui(() =>
        {
            progress.IsIndeterminate = false;
            progress.Value = 100;
            progress.BarColor = Success;
        });

        var startInfo = new ProcessStartInfo(executable)
        {
            UseShellExecute = true,
            WorkingDirectory = paths.Root,
        };
        var process = Process.Start(startInfo) ??
                      throw new LauncherException("Windows couldn't start Relay.");
        AppendLog("Started " + executable);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            if (token.IsCancellationRequested) throw;
            launched = true;
            Close();
            return;
        }

        var exitCode = -1;
        try
        {
            exitCode = process.ExitCode;
        }
        catch
        {
            // The exit code is unavailable if the handle was already released.
        }

        if (exitCode == 0)
        {
            launched = true;
            Close();
            return;
        }

        throw new LauncherException(
            $"Relay closed right after starting (exit code {exitCode}). Check the log or rebuild the app.");
    }

    private void HandleInstallLine(string line)
    {
        AppendLog(line);
        if (line.Contains("added ", StringComparison.OrdinalIgnoreCase))
        {
            SetStage("Dependencies installed", line.Trim(), throttle: true);
        }
        else if (line.Contains("Downloading", StringComparison.OrdinalIgnoreCase) ||
                 line.Contains("Fetching", StringComparison.OrdinalIgnoreCase))
        {
            SetStage("Installing dependencies", "Downloading Node packages…", throttle: true);
        }
        else if (line.Contains("npm error", StringComparison.OrdinalIgnoreCase))
        {
            SetStage("Installing dependencies", line.Trim(), error: true);
        }
    }

    private void HandleBuildLine(string line)
    {
        AppendLog(line);

        var match = Regex.Match(line, @"Building\s*\[[^\]]*\]\s*(\d+)/(\d+)");
        if (match.Success &&
            int.TryParse(match.Groups[1].Value, out var done) &&
            int.TryParse(match.Groups[2].Value, out var total) &&
            total > 0)
        {
            var percent = (int)Math.Round(100.0 * done / total);
            Ui(() =>
            {
                progress.IsIndeterminate = false;
                progress.Value = percent;
                progress.BarColor = Blue;
            });
            SetStage("Building the desktop engine", $"Compiling Rust crates… {percent}%", throttle: true);
            return;
        }

        if (line.Contains("Compiling ", StringComparison.Ordinal))
        {
            SetStage("Building the desktop engine", "Compiling Rust crates…", throttle: true);
        }
        else if (line.Contains("building for production", StringComparison.OrdinalIgnoreCase) ||
                 line.StartsWith("vite v", StringComparison.OrdinalIgnoreCase))
        {
            SetStage("Building the interface", "Bundling the chat workspace with Vite…", throttle: true);
        }
        else if (line.Contains("Bundling", StringComparison.OrdinalIgnoreCase) ||
                 line.Contains("Built application", StringComparison.OrdinalIgnoreCase))
        {
            SetStage("Packaging Relay", "Preparing the app executable…", throttle: true);
        }
        else if (line.Contains("Finished", StringComparison.OrdinalIgnoreCase))
        {
            Ui(() =>
            {
                progress.IsIndeterminate = false;
                progress.Value = 100;
            });
            SetStage("Finishing up", "The app is built. Launching in a moment…");
        }
        else if (line.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                 !line.Contains("0 errors", StringComparison.OrdinalIgnoreCase))
        {
            SetStage("Building Relay", line.Trim(), error: true);
        }
    }

    private void Fail(string message)
    {
        hasError = true;
        SetStage("Couldn't start Relay", message, error: true);
        AppendLog("ERROR  " + message);
        Ui(() =>
        {
            progress.IsIndeterminate = false;
            progress.Value = 100;
            progress.BarColor = Danger;
            openLogLink.Visible = File.Exists(paths.LogFile);
            secondaryButton.Text = "Rebuild";
            secondaryButton.Enabled = true;
        });
        SetDetails(true);
    }

    private void SetStage(string stage, string detail, bool error = false, bool throttle = false)
    {
        if (throttle)
        {
            var now = DateTime.UtcNow;
            if (stage == lastStage && (now - lastStageAt).TotalMilliseconds < 400) return;
            lastStage = stage;
            lastStageAt = now;
        }

        Ui(() =>
        {
            stageLabel.Text = stage;
            stageLabel.ForeColor = error ? Danger : Ink;
            detailLabel.Text = detail;
            detailLabel.ForeColor = error ? Danger : Muted;
        });
    }

    private void ApplyWindowSize()
    {
        var scale = DeviceDpi / 96f;
        var width = (int)Math.Round(BaseWidth * scale);
        var height = (int)Math.Round((detailsOpen ? ExpandedHeight : BaseHeight) * scale);
        MinimumSize = new Size(width, height);
        ClientSize = new Size(width, height);
    }
    private void SetDetails(bool open)
    {
        detailsOpen = open;
        logTitle.Visible = open;
        logHost.Visible = open;
        footerLabel.Visible = !open;
        detailsLink.Text = open ? "Hide details" : "Details";
        ApplyWindowSize();

        if (open)
        {
            logBox.SelectionStart = logBox.TextLength;
            logBox.ScrollToCaret();
        }
    }

    private void OnSecondaryClick(object? sender, EventArgs e)
    {
        if (busy)
        {
            secondaryButton.Enabled = false;
            secondaryButton.Text = "Stopping…";
            SetStage("Cancelling", "Stopping the running build…");
            runCts?.Cancel();
            return;
        }

        _ = RunFlowAsync(true);
    }

    private void OpenLog()
    {
        try
        {
            if (!File.Exists(paths.LogFile)) return;
            Process.Start(new ProcessStartInfo(paths.LogFile) { UseShellExecute = true });
        }
        catch
        {
            MessageBox.Show(this, "The log file could not be opened.", "Relay",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void AppendLog(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return;
        runLog?.Write(line);

        Ui(() =>
        {
            if (logBox.TextLength > 350_000)
            {
                logBox.Clear();
                logBox.AppendText("… earlier output trimmed …" + Environment.NewLine);
            }
            logBox.AppendText(line + Environment.NewLine);
            logBox.SelectionStart = logBox.TextLength;
            logBox.ScrollToCaret();
        });
    }

    private void Ui(Action action)
    {
        if (IsDisposed || Disposing) return;
        try
        {
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }
        catch (InvalidOperationException)
        {
            // The window went away while an update was in flight.
        }
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (busy && !launched)
        {
            var answer = MessageBox.Show(
                this,
                "Relay is still building. Stop the build and close the launcher?",
                "Relay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (answer != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            runCts?.Cancel();
        }

        uiTimer.Stop();
        runLog?.Dispose();
    }

    private static Label MakeLabel(
        string text,
        Rectangle bounds,
        float size,
        FontStyle style,
        Color color,
        ContentAlignment alignment = ContentAlignment.MiddleLeft)
    {
        return new Label
        {
            Text = text,
            Bounds = bounds,
            Font = new Font("Segoe UI", size, style),
            ForeColor = color,
            BackColor = Color.Transparent,
            TextAlign = alignment,
            AutoSize = false,
            UseCompatibleTextRendering = false,
        };
    }

    private static void ConfigureLink(LinkLabel link)
    {
        link.Font = new Font("Segoe UI", 9f);
        link.LinkColor = Blue;
        link.ActiveLinkColor = BlueHover;
        link.VisitedLinkColor = Blue;
        link.LinkBehavior = LinkBehavior.HoverUnderline;
        link.BackColor = Color.Transparent;
        link.TextAlign = ContentAlignment.MiddleLeft;
        link.AutoSize = false;
    }

    private static Image? LoadLogo()
    {
        try
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("relay-logo.png");
            if (stream is not null)
            {
                using var image = Image.FromStream(stream);
                return new Bitmap(image);
            }

            var localPath = Path.Combine(AppContext.BaseDirectory, "assets", "relay-logo.png");
            if (File.Exists(localPath))
            {
                return new Bitmap(localPath);
            }

            var rootPath = Path.Combine(AppContext.BaseDirectory, "..", "assets", "relay-logo.png");
            if (File.Exists(rootPath))
            {
                return new Bitmap(rootPath);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static Icon LoadAppIcon()
    {
        try
        {
            return Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
        }
        catch
        {
            return SystemIcons.Application;
        }
    }
}

internal sealed class RunLog : IDisposable
{
    private readonly string path;
    private readonly object gate = new();
    private StreamWriter? writer;

    public RunLog(string path)
    {
        this.path = path;
    }

    public void Start()
    {
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            writer = new StreamWriter(path, false, new UTF8Encoding(false)) { AutoFlush = true };
            writer.WriteLine($"Relay launcher · {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
        catch
        {
            writer = null;
        }
    }

    public void Write(string line)
    {
        lock (gate)
        {
            try
            {
                writer?.WriteLine(line);
            }
            catch
            {
                // Logging must never take the launcher down.
            }
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            try
            {
                writer?.Dispose();
            }
            catch
            {
                // Ignore close errors.
            }
            writer = null;
        }
    }
}
