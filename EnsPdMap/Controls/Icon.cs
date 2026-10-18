using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace EnsPdMap.Controls;

/// <summary>
/// Contrôle simple pour afficher un glyph/icône (texte).
/// Propriétés : Glyph, IconColor, Size.
/// </summary>
public class Icon : ContentView
{
    public static readonly BindableProperty GlyphProperty =
        BindableProperty.Create(nameof(Glyph), typeof(string), typeof(Icon), "");

    public static readonly BindableProperty IconColorProperty =
        BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(Icon), Colors.Black);

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(double), typeof(Icon), 20.0);

    public Icon()
    {
        var lbl = new Label
        {
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };

        lbl.SetBinding(Label.TextProperty, new Binding(nameof(Glyph), source: this));
        lbl.SetBinding(Label.TextColorProperty, new Binding(nameof(IconColor), source: this));
        lbl.SetBinding(Label.FontSizeProperty, new Binding(nameof(Size), source: this));

        Content = lbl;
    }

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public Color IconColor
    {
        get => (Color)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
}
