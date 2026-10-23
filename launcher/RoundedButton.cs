using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Relay.Launcher;

internal sealed class RoundedButton : Button
{
    public Color FillColor { get; set; } = Color.FromArgb(26, 115, 232);
    public Color HoverColor { get; set; } = Color.FromArgb(23, 101, 204);
    public Color TextColor { get; set; } = Color.White;
    public Color BorderColor { get; set; } = Color.Transparent;
    public Color DisabledFill { get; set; } = Color.FromArgb(241, 243, 244);
    public Color DisabledText { get; set; } = Color.FromArgb(128, 134, 139);
    public int Radius { get; set; } = 10;

    private bool hovered;

    public RoundedButton()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = Color.Transparent;
        Cursor = Cursors.Hand;
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        TabStop = true;
        Size = new Size(150, 40);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        hovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        hovered = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        Invalidate();
        base.OnEnabledChanged(e);
    }

    protected override void OnGotFocus(EventArgs e)
    {
        Invalidate();
        base.OnGotFocus(e);
    }

    protected override void OnLostFocus(EventArgs e)
    {
        Invalidate();
        base.OnLostFocus(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedRect(bounds, Radius);
        using var fill = new SolidBrush(Enabled ? (hovered ? HoverColor : FillColor) : DisabledFill);
        graphics.FillPath(fill, path);

        if (Enabled && BorderColor.A > 0)
        {
            using var border = new Pen(BorderColor);
            graphics.DrawPath(border, path);
        }

        if (Focused)
        {
            using var focus = new Pen(Color.FromArgb(90, 26, 115, 232), 2f);
            var focusBounds = Rectangle.Inflate(bounds, -3, -3);
            using var focusPath = RoundedRect(focusBounds, Math.Max(4, Radius - 2));
            graphics.DrawPath(focus, focusPath);
        }

        TextRenderer.DrawText(
            graphics,
            Text,
            Font,
            bounds,
            Enabled ? TextColor : DisabledText,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix);
    }

    internal static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = Math.Max(1, radius * 2);
        if (diameter >= bounds.Width || diameter >= bounds.Height)
        {
            path.AddEllipse(bounds);
            return path;
        }

        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
        path.AddArc(arc, 180, 90);

        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);

        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);

        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);

        path.CloseFigure();
        return path;
    }
}
