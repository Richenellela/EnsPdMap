using EnsPdMap.Models;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EnsPdMap.Controls;

/// <summary>Barre de navigation du bas : Accueil, Plan, Services, Hors ligne.</summary>
public class BottomNav : ContentView
{
    public static readonly BindableProperty ActiveProperty =
        BindableProperty.Create(nameof(Active), typeof(string), typeof(BottomNav), "accueil",
            propertyChanged: OnActiveChanged);

    public string Active
    {
        get { return (string)GetValue(ActiveProperty); }
        set { SetValue(ActiveProperty, value); }
    }

    static void OnActiveChanged(BindableObject b, object oldValue, object newValue)
    {
        ((BottomNav)b).Build();
    }

    public BottomNav()
    {
        Build();
    }

    void Build()
    {
        var items = new[]
        {
            new[] { "accueil", "home", "Accueil" },
            new[] { "plan", "map", "Plan" },
            new[] { "annuaire", "building", "Services" },
            new[] { "horsligne", "wifioff", "Hors ligne" }
        };

        var grid = new Grid { BackgroundColor = Colors.White, Padding = new Thickness(6, 8, 6, 8), ColumnSpacing = 0 };
        for (int i = 0; i < items.Length; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        for (int i = 0; i < items.Length; i++)
        {
            string key = items[i][0];
            string glyph = items[i][1];
            string text = items[i][2];
            bool sel = key == Active;

            var pill = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(22) },
                BackgroundColor = sel ? Pal.Navy : Colors.Transparent,
                Padding = new Thickness(20, 8),
                HorizontalOptions = LayoutOptions.Center,
                Content = new Icon { Glyph = glyph, IconColor = sel ? Colors.White : Pal.Muted, Size = 22 }
            };

            var label = new Label
            {
                Text = text,
                FontSize = 12,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = sel ? Pal.Navy : Pal.Muted,
                FontAttributes = sel ? FontAttributes.Bold : FontAttributes.None
            };

            var cell = new VerticalStackLayout { Spacing = 2 };
            cell.Children.Add(pill);
            cell.Children.Add(label);
            cell.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    if (key != Active) await Shell.Current.GoToAsync("//" + key);
                })
            });
            grid.Add(cell, i, 0);
        }

        var root = new VerticalStackLayout { Spacing = 0 };
        root.Children.Add(new BoxView { HeightRequest = 1, Color = Pal.Line });
        root.Children.Add(grid);
        Content = root;
    }
}
