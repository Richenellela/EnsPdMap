using EnsPdMap.Controls;
using EnsPdMap.Models;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EnsPdMap.Helpers;

public static class Ui
{
    public static Border Chip(string text, bool selected, Action onTap)
    {
        var chip = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
            Stroke = new SolidColorBrush(selected ? Pal.Ink : Pal.Line),
            StrokeThickness = 1.5,
            BackgroundColor = selected ? Pal.Ink : Colors.White,
            Padding = new Thickness(20, 0),
            HeightRequest = 40,
            Content = new Label
            {
                Text = text,
                FontAttributes = FontAttributes.Bold,
                FontSize = 15,
                TextColor = selected ? Colors.White : Pal.Ink,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            }
        };
        chip.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(onTap) });
        return chip;
    }

    public static View ProcedureRow(Procedure p, bool divider)
    {
        var badge = new Border
        {
            WidthRequest = 56,
            HeightRequest = 56,
            StrokeThickness = 0,
            BackgroundColor = Pal.Sand,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
            VerticalOptions = LayoutOptions.Center
        };
        var badgeText = new VerticalStackLayout
        {
            Spacing = 0,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        badgeText.Children.Add(new Label { Text = "Porte", FontSize = 10, TextColor = Pal.GoldDark, HorizontalTextAlignment = TextAlignment.Center });
        badgeText.Children.Add(new Label { Text = p.DoorNumber, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Pal.GoldDark, HorizontalTextAlignment = TextAlignment.Center });
        badge.Content = badgeText;

        var texts = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        texts.Children.Add(new Label { Text = p.Title, FontSize = 17, FontAttributes = FontAttributes.Bold });
        texts.Children.Add(new Label { Text = p.Summary, FontSize = 14, TextColor = Pal.Muted });

        var row = new Grid { ColumnSpacing = 14, Padding = new Thickness(0, 14) };
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        row.Add(badge, 0, 0);
        row.Add(texts, 1, 0);
        row.Add(new Icon { Glyph = "chevron", IconColor = Pal.Muted, Size = 18, VerticalOptions = LayoutOptions.Center }, 2, 0);

        string id = p.Id;
        row.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Nav.GoAsync("procedure?id=" + id))
        });

        var box = new VerticalStackLayout { Spacing = 0 };
        box.Children.Add(row);
        if (divider) box.Children.Add(new BoxView { HeightRequest = 1, Color = Pal.Line });
        return box;
    }

    public static Border RoundButton(string glyph, Color bg, Color fg, Color stroke, Action onTap)
    {
        var b = new Border
        {
            WidthRequest = 52,
            HeightRequest = 52,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(26) },
            Stroke = new SolidColorBrush(stroke),
            StrokeThickness = 1.5,
            BackgroundColor = bg,
            Content = new Icon { Glyph = glyph, IconColor = fg, Size = 22 }
        };
        b.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(onTap) });
        return b;
    }
}
