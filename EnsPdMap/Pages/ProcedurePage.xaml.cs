using EnsPdMap.Controls;
using EnsPdMap.Models;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EnsPdMap.Pages;

[QueryProperty(nameof(ItemId), "id")]
public partial class ProcedurePage : ContentPage
{
    Procedure _p;
    int _current;
    bool _bookmarked = true;

    public string ItemId
    {
        set
        {
            _p = Data.GetProcedure(value);
            _current = 0;
            Load();
        }
    }

    public ProcedurePage()
    {
        InitializeComponent();
    }

    void Load()
    {
        CatLabel.Text = _p.Category;
        TitleLabel.Text = _p.Title;
        DelayLabel.Text = _p.Delay;
        CostLabel.Text = _p.Cost;
        OfficeLabel.Text = _p.Office;
        FormSizeLabel.Text = _p.FormSize;
        LocateLabel.Text = _p.LocateLabel;
        BuildPieces();
        BuildSteps();
    }

    void BuildPieces()
    {
        PiecesList.Children.Clear();
        int ready = 0;
        foreach (var piece in _p.Pieces)
        {
            if (piece.Ready) ready++;
            var item = piece;

            var box = new Border
            {
                WidthRequest = 30,
                HeightRequest = 30,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
                Stroke = new SolidColorBrush(item.Ready ? Pal.Green : Pal.Ink),
                StrokeThickness = 2,
                BackgroundColor = item.Ready ? Pal.Green : Colors.White,
                VerticalOptions = LayoutOptions.Start,
                Content = new Icon { Glyph = "check", IconColor = Colors.White, Size = 18, IsVisible = item.Ready }
            };

            var texts = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
            texts.Children.Add(new Label
            {
                Text = item.Name,
                FontSize = 18,
                FontAttributes = item.Ready ? FontAttributes.None : FontAttributes.Bold,
                TextColor = item.Ready ? Pal.Muted : Pal.Ink
            });
            if (!string.IsNullOrEmpty(item.Detail))
                texts.Children.Add(new Label { Text = item.Detail, FontSize = 14, TextColor = Pal.Muted });

            var row = new Grid { ColumnSpacing = 14, Padding = new Thickness(0, 9) };
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            row.Add(box, 0, 0);
            row.Add(texts, 1, 0);
            row.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    item.Ready = !item.Ready;
                    BuildPieces();
                })
            });
            PiecesList.Children.Add(row);
        }

        ReadyLabel.Text = ready + " / " + _p.Pieces.Count + " prêtes";
        ReadyBar.Progress = _p.Pieces.Count == 0 ? 0 : (double)ready / _p.Pieces.Count;
    }

    void BuildSteps()
    {
        StepsList.Children.Clear();
        for (int i = 0; i < _p.Steps.Count; i++)
        {
            int index = i;
            var step = _p.Steps[i];
            bool done = i < _current;
            bool current = i == _current;
            bool last = i == _p.Steps.Count - 1;

            var circle = new Border
            {
                WidthRequest = 36,
                HeightRequest = 36,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) },
                Stroke = new SolidColorBrush(current ? Pal.Gold : (done ? Pal.Green : Pal.Line)),
                StrokeThickness = 1.5,
                BackgroundColor = current ? Pal.Gold : (done ? Pal.Green : Colors.White),
                VerticalOptions = LayoutOptions.Start
            };
            if (done)
                circle.Content = new Icon { Glyph = "check", IconColor = Colors.White, Size = 18 };
            else
                circle.Content = new Label
                {
                    Text = (i + 1).ToString(),
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = current ? Colors.White : Pal.Muted,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

            var left = new Grid { VerticalOptions = LayoutOptions.Fill, WidthRequest = 36 };
            left.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            left.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            left.Add(circle, 0, 0);
            if (!last)
            {
                var connector = new BoxView { WidthRequest = 2, Color = Pal.Line, HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 4) };
                left.Add(connector, 0, 1);
            }

            var texts = new VerticalStackLayout { Spacing = 2, Padding = new Thickness(0, 0, 0, 18) };
            texts.Children.Add(new Label { Text = step.Title, FontSize = 17, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 6, 0, 0) });
            texts.Children.Add(new Label { Text = step.Detail, FontSize = 14, TextColor = Pal.Muted });

            var row = new Grid { ColumnSpacing = 14 };
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            row.Add(left, 0, 0);
            row.Add(texts, 1, 0);
            row.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    _current = index;
                    BuildSteps();
                })
            });
            StepsList.Children.Add(row);
        }
    }

    async void OnBack(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    async void OnShare(object sender, TappedEventArgs e)
    {
        try
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = _p.Title,
                Text = _p.Title + " — délai " + _p.Delay + ", coût " + _p.Cost + ", bureau " + _p.Office + " (EnsPdMap, ENSPD)."
            });
        }
        catch (Exception)
        {
            await DisplayAlert("Partager", "Le partage n'est pas disponible sur cet appareil.", "OK");
        }
    }

    void OnBookmark(object sender, TappedEventArgs e)
    {
        _bookmarked = !_bookmarked;
        BookmarkBtn.BackgroundColor = _bookmarked ? Pal.Navy : Colors.White;
        BookmarkBtn.Stroke = new SolidColorBrush(_bookmarked ? Pal.Navy : Pal.Line);
        BookmarkBtn.StrokeThickness = _bookmarked ? 0 : 1.5;
        BookmarkIcon.IconColor = _bookmarked ? Pal.Gold : Pal.Ink;
    }

    async void OnDownload(object sender, TappedEventArgs e)
    {
        await DisplayAlert("Formulaire PDF compact",
            "Le formulaire (" + _p.FormSize + ") est disponible hors ligne. Le téléchargement réel sera branché avec le back-end.",
            "OK");
    }

    async void OnLocate(object sender, TappedEventArgs e)
    {
        AppState.PlanTarget = _p.ServiceId;
        await Shell.Current.GoToAsync("//plan");
    }
}
