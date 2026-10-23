using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Relay.Launcher;

internal sealed class RoundedProgress : Control
{
    private readonly System.Windows.Forms.Timer timer;
    private int marquee;
    private int value;

    public bool IsIndeterminate { get; set; }
    public Color TrackColor { get; set; } = Color.FromArgb(232, 234, 237);
    public Color BarColor { get; set; } = Color.FromArgb(26, 115, 232);
    public int Radius { get; set; } = 3;

    public int Value
    {
        get => value;
        set
        {
            this.value = Math.Clamp(value, 0, 100);
            Invalidate();
        }
    }

    public RoundedProgress()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);
        Height = 6;
        timer = new System.Windows.Forms.Timer { Interval = 33 };
        timer.Tick += (_, _) =>
        {
            if (!IsIndeterminate) return;
            marquee = (marquee + 2) % 140;
            Invalidate();
        };
        timer.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = new Rectangle(0, 0, Width, Height);
        using var trackPath = RoundedButton.RoundedRect(bounds, Radius);
        using var trackBrush = new SolidBrush(TrackColor);
        graphics.FillPath(trackBrush, trackPath);

        using var barBrush = new SolidBrush(BarColor);
        if (IsIndeterminate)
        {
            var width = Math.Max(70, Width / 4);
            var left = (int)((Width + width) * (marquee / 140f)) - width;
            var clip = graphics.Clip;
            graphics.SetClip(bounds);
            using var barPath = RoundedButton.RoundedRect(
                new Rectangle(left, 0, width, Height), Radius);
            graphics.FillPath(barBrush, barPath);
            graphics.Clip = clip;
        }
        else if (Value > 0)
        {
            var width = Math.Max(1, (int)Math.Round(Width * (Value / 100f)));
            using var barPath = RoundedButton.RoundedRect(new Rectangle(0, 0, width, Height), Radius);
            graphics.FillPath(barBrush, barPath);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            timer.Stop();
            timer.Dispose();
        }
        base.Dispose(disposing);
    }
}
