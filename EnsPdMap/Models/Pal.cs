using Microsoft.Maui.Graphics;

namespace EnsPdMap.Models;

public static class Pal
{
    public static readonly Color Navy = Color.FromArgb("#0A2342");
    public static readonly Color Gold = Color.FromArgb("#C08B1C");
    public static readonly Color GoldDark = Color.FromArgb("#7A5A10");
    public static readonly Color Sand = Color.FromArgb("#F5E6C8");
    public static readonly Color Green = Color.FromArgb("#14603A");
    public static readonly Color GreenSoft = Color.FromArgb("#DCEBE2");
    public static readonly Color Bg = Color.FromArgb("#F7F8FA");
    public static readonly Color Ink = Color.FromArgb("#0F172A");
    public static readonly Color Muted = Color.FromArgb("#5B6778");
    public static readonly Color Line = Color.FromArgb("#D9DEE8");
    public static readonly Color Red = Color.FromArgb("#B42318");
    public static readonly Color RedSoft = Color.FromArgb("#FBEDEC");
    public static readonly Color Block = Color.FromArgb("#CAD6E2");
    public static readonly Color BlockLine = Color.FromArgb("#9AA9BC");
    public static readonly Color MapBg = Color.FromArgb("#EDF2F6");
    public static readonly Color Park = Color.FromArgb("#DDEBDD");
    public static readonly Color Badge = Color.FromArgb("#DDE6EF");

    public static Color Crowd(string c)
    {
        if (c == "Forte") return Red;
        if (c == "Modérée") return GoldDark;
        return Green;
    }

    public static Color Status(string s)
    {
        return s == "Ouvert" ? Green : Red;
    }
}
