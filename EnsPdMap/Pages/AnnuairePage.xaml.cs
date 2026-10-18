using EnsPdMap.Controls;
using EnsPdMap.Helpers;
using EnsPdMap.Models;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EnsPdMap.Pages;

public partial class AnnuairePage : ContentPage
{
    static readonly string[] Filters = { "Tous", "Scolarité", "Départements", "Médical" };
    string _filter = "Tous";
    string _query = "";

    public AnnuairePage()
    {
        InitializeComponent();
        Rebuild();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!string.IsNullOrEmpty(AppState.PendingQuery))
        {
            _query = AppState.PendingQuery;
            AppState.PendingQuery = "";
            SearchBox.IsVisible = true;
            SearchEntry.Text = _query;
            Rebuild();
        }
    }

    void OnToggleSearch(object sender, TappedEventArgs e)
    {
        SearchBox.IsVisible = !SearchBox.IsVisible;
        if (!SearchBox.IsVisible)
        {
            _query = "";
            SearchEntry.Text = "";
            Rebuild();
        }
        else
        {
            SearchEntry.Focus();
        }
    }

    void OnQueryChanged(object sender, TextChangedEventArgs e)
    {
        _query = e.NewTextValue ?? "";
        Rebuild();
    }

    void Rebuild()
    {
        FilterStack.Children.Clear();
        foreach (var f in Filters)
        {
            string value = f;
            FilterStack.Children.Add(Ui.Chip(f, f == _filter, () => { _filter = value; Rebuild(); }));
        }

        ServiceList.Children.Clear();
        string q = AppState.Norm(_query.Trim());
        int count = 0;
        foreach (var s in Data.Services)
        {
            if (_filter != "Tous" && s.Category != _filter) continue;
            if (q.Length > 0)
            {
                string hay = AppState.Norm(s.Name + " " + s.Description + " " + s.ListLoc + " " + s.Category);
                if (!hay.Contains(q)) continue;
            }
            count++;
            ServiceList.Children.Add(BuildRow(s));
            ServiceList.Children.Add(new BoxView { HeightRequest = 1, Color = Pal.Line });
        }
        EmptyLabel.IsVisible = count == 0;
    }

    View BuildRow(Service s)
    {
        var badge = new Border
        {
            WidthRequest = 56,
            HeightRequest = 56,
            StrokeThickness = 0,
            BackgroundColor = Pal.Badge,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = s.Code,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Pal.Navy,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };

        var texts = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        texts.Children.Add(new Label { Text = s.Name, FontSize = 17, FontAttributes = FontAttributes.Bold });
        texts.Children.Add(new Label { Text = s.Description, FontSize = 14, TextColor = Pal.Muted });
        texts.Children.Add(new Label { Text = s.ListLoc, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Pal.Muted });

        var status = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        status.Children.Add(new Label { Text = s.Status, FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Pal.Status(s.Status), HorizontalTextAlignment = TextAlignment.End });
        status.Children.Add(new Label { Text = s.Crowd, FontSize = 13, TextColor = Pal.Crowd(s.Crowd), HorizontalTextAlignment = TextAlignment.End });

        var row = new Grid { ColumnSpacing = 12, Padding = new Thickness(0, 16) };
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        row.Add(badge, 0, 0);
        row.Add(texts, 1, 0);
        row.Add(status, 2, 0);
        row.Add(new Icon { Glyph = "chevron", IconColor = Pal.Muted, Size = 18, VerticalOptions = LayoutOptions.Center }, 3, 0);

        string id = s.Id;
        row.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Nav.GoAsync("bureau?id=" + id))
        });
        return row;
    }
}
