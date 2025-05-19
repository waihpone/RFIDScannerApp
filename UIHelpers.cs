using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public static class UIHelpers
{
    public static Color HexToColor(string hex)
    {
        return ColorTranslator.FromHtml(hex);
    }

    public static void RoundCorners(Control control, int radius)
    {
        var bounds = control.ClientRectangle;
        var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
        path.AddArc(bounds.Right - radius, bounds.Y, radius, radius, 270, 90);
        path.AddArc(bounds.Right - radius, bounds.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - radius, radius, radius, 90, 90);
        path.CloseAllFigures();
        control.Region = new Region(path);
    }
}
