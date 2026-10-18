using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using MPath = Microsoft.Maui.Controls.Shapes.Path;

namespace EnsPdMap.Controls;

/// <summary>Icône vectorielle (tracé) teintable : Glyph + IconColor + Size.</summary>
public class Icon : ContentView
{
    public static readonly BindableProperty GlyphProperty =
        BindableProperty.Create(nameof(Glyph), typeof(string), typeof(Icon), "", propertyChanged: OnChanged);

    public static readonly BindableProperty IconColorProperty =
        BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(Icon), Colors.Black, propertyChanged: OnChanged);

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(double), typeof(Icon), 22d, propertyChanged: OnChanged);

    public string Glyph
    {
        get { return (string)GetValue(GlyphProperty); }
        set { SetValue(GlyphProperty, value); }
    }

    public Color IconColor
    {
        get { return (Color)GetValue(IconColorProperty); }
        set { SetValue(IconColorProperty, value); }
    }

    public double Size
    {
        get { return (double)GetValue(SizeProperty); }
        set { SetValue(SizeProperty, value); }
    }

    static void OnChanged(BindableObject b, object oldValue, object newValue)
    {
        ((Icon)b).Refresh();
    }

    readonly MPath _path;

    public Icon()
    {
        _path = new MPath
        {
            Aspect = Stretch.Uniform,
            StrokeLineCap = PenLineCap.Round,
            StrokeLineJoin = PenLineJoin.Round,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        Content = _path;
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;
        Refresh();
    }

    void Refresh()
    {
        if (_path == null) return;
        var size = Size;
        WidthRequest = size;
        HeightRequest = size;
        Padding = new Thickness(Math.Max(1, size / 14));
        _path.Stroke = new SolidColorBrush(IconColor);
        _path.StrokeThickness = Math.Max(1.4, size / 11);

        string d;
        if (Glyph != null && Paths.TryGetValue(Glyph, out d))
        {
            try
            {
                _path.Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(d);
            }
            catch
            {
                _path.Data = null;
            }
        }
        else
        {
            _path.Data = null;
        }
    }

    static readonly Dictionary<string, string> Paths = new Dictionary<string, string>
    {
        { "home", "M3 11 L12 3 L21 11 M5 9.5 L5 21 L19 21 L19 9.5 M10 21 L10 14 L14 14 L14 21" },
        { "search", "M18 11 C18 14.87 14.87 18 11 18 C7.13 18 4 14.87 4 11 C4 7.13 7.13 4 11 4 C14.87 4 18 7.13 18 11 Z M16 16 L21 21" },
        { "mail", "M3 6 L21 6 L21 18 L3 18 Z M3 7 L12 13 L21 7" },
        { "eye", "M2 12 C5 6 8.5 5 12 5 C15.5 5 19 6 22 12 C19 18 15.5 19 12 19 C8.5 19 5 18 2 12 Z M15 12 C15 13.66 13.66 15 12 15 C10.34 15 9 13.66 9 12 C9 10.34 10.34 9 12 9 C13.66 9 15 10.34 15 12 Z" },
        { "eyeoff", "M2 12 C5 6 8.5 5 12 5 C15.5 5 19 6 22 12 C19 18 15.5 19 12 19 C8.5 19 5 18 2 12 Z M15 12 C15 13.66 13.66 15 12 15 C10.34 15 9 13.66 9 12 C9 10.34 10.34 9 12 9 C13.66 9 15 10.34 15 12 Z M4 4 L20 20" },
        { "map", "M3 6 L9 4 L15 6 L21 4 L21 18 L15 20 L9 18 L3 20 Z M9 4 L9 18 M15 6 L15 20" },
        { "building", "M5 21 L5 4 L15 4 L15 21 M15 9 L19 9 L19 21 M3 21 L21 21 M8 8 L12 8 M8 12 L12 12 M8 16 L12 16" },
        { "doc", "M6 2 L14 2 L19 7 L19 22 L6 22 Z M14 2 L14 7 L19 7 M9 13 L16 13 M9 17 L16 17" },
        { "wifioff", "M3 3 L21 21 M8 16 C10 14.3 14 14.3 16 16 M4 11.5 C5.5 10.2 7 9.3 8.7 8.7 M20 11.5 C18.3 10 16.5 9 14.5 8.5 M12 20 L12.01 20" },
        { "back", "M19 12 L5 12 M12 19 L5 12 L12 5" },
        { "chevron", "M9 5 L16 12 L9 19" },
        { "check", "M4 12.5 L9.5 18 L20 6.5" },
        { "pin", "M12 22 C12 22 5 15.5 5 10 C5 6.13 8.13 3 12 3 C15.87 3 19 6.13 19 10 C19 15.5 12 22 12 22 Z M14.5 10 C14.5 11.38 13.38 12.5 12 12.5 C10.62 12.5 9.5 11.38 9.5 10 C9.5 8.62 10.62 7.5 12 7.5 C13.38 7.5 14.5 8.62 14.5 10 Z" },
        { "nav", "M3 11 L21 3 L13 21 L11 13 Z" },
        { "leaf", "M5 20 C5 10 10 5 20 4 C20 14 15 19 5 20 Z M5 20 L13 11" },
        { "bolt", "M13 2 L4 14 L11 14 L10 22 L20 9 L13 9 Z" },
        { "bookmark", "M6 3 L18 3 L18 21 L12 17 L6 21 Z" },
        { "phone", "M6 3 L9.5 3 L11 7.5 L9 9 C10.2 11.5 12.5 13.8 15 15 L16.5 13 L21 14.5 L21 18 C21 19.7 19.7 21 18 21 C10.8 21 3 13.2 3 6 C3 4.3 4.3 3 6 3 Z" },
        { "users", "M12.5 7 C12.5 8.93 10.93 10.5 9 10.5 C7.07 10.5 5.5 8.93 5.5 7 C5.5 5.07 7.07 3.5 9 3.5 C10.93 3.5 12.5 5.07 12.5 7 Z M3 20 L3 18 C3 16.3 4.3 15 6 15 L12 15 C13.7 15 15 16.3 15 18 L15 20 M17 4 C18.7 4.5 19.5 5.8 19.5 7 C19.5 8.2 18.7 9.5 17 10 M19 15.3 C20.6 15.8 21.5 17 21.5 18.5 L21.5 20" },
        { "user", "M20 21 L20 19 C20 16.8 18.2 15 16 15 L8 15 C5.8 15 4 16.8 4 19 L4 21 M16 7 C16 9.2 14.2 11 12 11 C9.8 11 8 9.2 8 7 C8 4.8 9.8 3 12 3 C14.2 3 16 4.8 16 7 Z" },
        { "share", "M12 15 L12 3 M8 7 L12 3 L16 7 M5 12 L5 21 L19 21 L19 12" },
        { "download", "M12 3 L12 15 M7 10 L12 15 L17 10 M4 18 L4 21 L20 21 L20 18" },
        { "plus", "M12 5 L12 19 M5 12 L19 12" },
        { "minus", "M5 12 L19 12" },
        { "locate", "M12 8 C14.2 8 16 9.8 16 12 C16 14.2 14.2 16 12 16 C9.8 16 8 14.2 8 12 C8 9.8 9.8 8 12 8 Z M12 2 L12 6 M12 18 L12 22 M2 12 L6 12 M18 12 L22 12" },
        { "ok", "M21 12 C21 16.97 16.97 21 12 21 C7.03 21 3 16.97 3 12 C3 7.03 7.03 3 12 3 C16.97 3 21 7.03 21 12 Z M8 12.5 L11 15.5 L16 9.5" },
        { "alert", "M21 12 C21 16.97 16.97 21 12 21 C7.03 21 3 16.97 3 12 C3 7.03 7.03 3 12 3 C16.97 3 21 7.03 21 12 Z M12 7.5 L12 13 M12 16.5 L12.01 16.5" }
    };
}
