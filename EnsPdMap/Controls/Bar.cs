using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EnsPdMap.Controls;

/// <summary>Barre de progression arrondie : Progress (0 à 1), FillColor, TrackColor.</summary>
public class Bar : ContentView
{
    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(nameof(Progress), typeof(double), typeof(Bar), 0d, propertyChanged: OnChanged);

    public static readonly BindableProperty FillColorProperty =
        BindableProperty.Create(nameof(FillColor), typeof(Color), typeof(Bar), Colors.Green, propertyChanged: OnChanged);

    public static readonly BindableProperty TrackColorProperty =
        BindableProperty.Create(nameof(TrackColor), typeof(Color), typeof(Bar), Colors.LightGray, propertyChanged: OnChanged);

    public double Progress
    {
        get { return (double)GetValue(ProgressProperty); }
        set { SetValue(ProgressProperty, value); }
    }

    public Color FillColor
    {
        get { return (Color)GetValue(FillColorProperty); }
        set { SetValue(FillColorProperty, value); }
    }

    public Color TrackColor
    {
        get { return (Color)GetValue(TrackColorProperty); }
        set { SetValue(TrackColorProperty, value); }
    }

    static void OnChanged(BindableObject b, object oldValue, object newValue)
    {
        ((Bar)b).Build();
    }

    public Bar()
    {
        Build();
    }

    void Build()
    {
        double p = Math.Max(0.0001, Math.Min(0.9999, Progress));
        var grid = new Grid { ColumnSpacing = 0, BackgroundColor = TrackColor };
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(p, GridUnitType.Star)));
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1 - p, GridUnitType.Star)));
        grid.Add(new BoxView { Color = FillColor }, 0, 0);

        Content = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(4) },
            HeightRequest = 8,
            BackgroundColor = TrackColor,
            Content = grid
        };
    }
}
