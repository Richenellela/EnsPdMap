using EnsPdMap.Controls;
using EnsPdMap.Helpers;
using EnsPdMap.Models;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EnsPdMap.Pages;

[QueryProperty(nameof(ItemId), "id")]
public partial class ProcedurePage : ContentPage
{
    Procedure _p;
    int _open = -1;
    int _faqOpen = -1;
    bool _bookmarked;

    string _itemId;

    public string ItemId
    {
        get { return _itemId; }
        set
        {
            _itemId = value;
            _p = Data.GetProcedure(value);
            _open = -1;
            _faqOpen = -1;
            Load();
        }
    }

    public ProcedurePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        PageFx.FadeIn(this);
    }

    void Load()
    {
        CatLabel.Text = _p.Category;
        TitleLabel.Text = _p.Title;
        OverviewLabel.Text = _p.Overview;
        DelayLabel.Text = string.IsNullOrEmpty(_p.Delay) ? "—" : _p.Delay;
        CostLabel.Text = _p.Cost;
        OfficeLabel.Text = _p.Office;
        AudienceLabel.Text = _p.Audience;
        WhereLabel.Text = _p.Where;
        LocateLabel.Text = _p.LocateLabel;
        LocateIcon.Glyph = string.IsNullOrEmpty(_p.ServiceId) ? "download" : "nav";

        _bookmarked = ProgressStore.IsBookmarked(_p.Id);
        ApplyBookmark();

        BuildPieces();
        BuildSteps();
        BuildFaq();
    }

    void ApplyBookmark()
    {
        BookmarkBtn.BackgroundColor = _bookmarked ? Pal.Navy : Colors.White;
        BookmarkBtn.Stroke = new SolidColorBrush(_bookmarked ? Pal.Navy : Pal.Line);
        BookmarkBtn.StrokeThickness = _bookmarked ? 0 : 1.5;
        BookmarkIcon.IconColor = _bookmarked ? Pal.Gold : Pal.Ink;
    }

    // ---------- pièces ----------

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
            if (!string.IsNullOrEmpty(item.How))
                texts.Children.Add(new Label { Text = item.How, FontSize = 13, TextColor = Pal.Muted, FontAttributes = FontAttributes.Italic });

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
                    ProgressStore.Update(_p);
                    BuildPieces();
                })
            });
            PiecesList.Children.Add(row);
        }

        ReadyLabel.Text = ready + " / " + _p.Pieces.Count + " prêtes";
        ReadyBar.Progress = _p.Pieces.Count == 0 ? 0 : (double)ready / _p.Pieces.Count;
    }

    // ---------- parcours détaillé ----------

    Border ActionButton(string glyph, string text, bool filled, Action tap)
    {
        var content = new HorizontalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        content.Children.Add(new Icon { Glyph = glyph, IconColor = filled ? Colors.White : Pal.Navy, Size = 16, VerticalOptions = LayoutOptions.Center });
        content.Children.Add(new Label
        {
            Text = text,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = filled ? Colors.White : Pal.Navy,
            VerticalOptions = LayoutOptions.Center
        });

        var button = new Border
        {
            HeightRequest = 44,
            Padding = new Thickness(14, 0),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Stroke = new SolidColorBrush(Pal.Navy),
            StrokeThickness = 1.5,
            BackgroundColor = filled ? Pal.Navy : Colors.White,
            Content = content
        };
        button.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(tap) });
        return button;
    }

    View NoteBox(string label, string text, Color background, Color foreground)
    {
        var l = new Label
        {
            FormattedText = new FormattedString
            {
                Spans =
                {
                    new Span { Text = label + " ", FontAttributes = FontAttributes.Bold, TextColor = foreground },
                    new Span { Text = text, TextColor = foreground }
                }
            },
            FontSize = 13
        };
        return new Border
        {
            StrokeThickness = 0,
            BackgroundColor = background,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = new Thickness(12, 10),
            Content = l
        };
    }

    void BuildSteps()
    {
        StepsList.Children.Clear();

        int current = -1;
        for (int k = 0; k < _p.Steps.Count; k++)
        {
            if (!_p.Steps[k].Done) { current = k; break; }
        }

        for (int i = 0; i < _p.Steps.Count; i++)
        {
            int index = i;
            var step = _p.Steps[i];
            bool done = step.Done;
            bool isCurrent = i == current;
            bool last = i == _p.Steps.Count - 1;
            bool open = i == _open;

            var circle = new Border
            {
                WidthRequest = 36,
                HeightRequest = 36,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) },
                Stroke = new SolidColorBrush(isCurrent ? Pal.Gold : (done ? Pal.Green : Pal.Line)),
                StrokeThickness = 1.5,
                BackgroundColor = isCurrent ? Pal.Gold : (done ? Pal.Green : Colors.White),
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
                    TextColor = isCurrent ? Colors.White : Pal.Muted,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

            var left = new Grid { VerticalOptions = LayoutOptions.Fill, WidthRequest = 36 };
            left.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            left.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            left.Add(circle, 0, 0);
            if (!last)
                left.Add(new BoxView { WidthRequest = 2, Color = Pal.Line, HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 4) }, 0, 1);

            // en-tête (toucher = déplier)
            var header = new Grid { ColumnSpacing = 8 };
            header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            var titles = new VerticalStackLayout { Spacing = 2 };
            titles.Children.Add(new Label
            {
                Text = step.Title,
                FontSize = 17,
                FontAttributes = FontAttributes.Bold,
                TextColor = done ? Pal.Muted : Pal.Ink
            });
            if (!string.IsNullOrEmpty(step.Duration))
                titles.Children.Add(new Label { Text = step.Duration, FontSize = 13, TextColor = Pal.Muted });
            header.Add(titles, 0, 0);
            header.Add(new Icon
            {
                Glyph = "chevron",
                IconColor = Pal.Muted,
                Size = 16,
                Rotation = open ? 90 : 0,
                VerticalOptions = LayoutOptions.Start
            }, 1, 0);
            header.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    _open = _open == index ? -1 : index;
                    BuildSteps();
                })
            });

            var texts = new VerticalStackLayout { Spacing = 8, Padding = new Thickness(0, 6, 0, 18) };
            texts.Children.Add(header);

            if (open)
            {
                if (!string.IsNullOrEmpty(step.Detail))
                    texts.Children.Add(new Label { Text = step.Detail, FontSize = 15 });
                if (!string.IsNullOrEmpty(step.Tip))
                    texts.Children.Add(NoteBox("Astuce :", step.Tip, Pal.GreenSoft, Pal.Green));
                if (!string.IsNullOrEmpty(step.Warning))
                    texts.Children.Add(NoteBox("Attention :", step.Warning, Pal.RedSoft, Pal.Red));

                var actions = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };
                if (!string.IsNullOrEmpty(step.ServiceId))
                {
                    string serviceId = step.ServiceId;
                    var plan = ActionButton("nav", "Voir sur le plan", true, () =>
                    {
                        AppState.PlanTarget = serviceId;
                        _ = Nav.GoAsync("//main/plan");
                    });
                    plan.Margin = new Thickness(0, 0, 8, 8);
                    actions.Children.Add(plan);
                }
                if (!string.IsNullOrEmpty(step.ProcedureId))
                {
                    string procedureId = step.ProcedureId;
                    var linked = ActionButton("doc", "Voir la démarche", false, () => { _ = Nav.GoAsync("procedure?id=" + procedureId); });
                    linked.Margin = new Thickness(0, 0, 8, 8);
                    actions.Children.Add(linked);
                }
                var toggle = ActionButton(done ? "back" : "check", done ? "Marquer non fait" : "Marquer comme fait", false, () =>
                {
                    step.Done = !step.Done;
                    ProgressStore.Update(_p);
                    BuildSteps();
                });
                toggle.Margin = new Thickness(0, 0, 8, 8);
                actions.Children.Add(toggle);
                texts.Children.Add(actions);
            }

            var row = new Grid { ColumnSpacing = 14 };
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            row.Add(left, 0, 0);
            row.Add(texts, 1, 0);
            StepsList.Children.Add(row);
        }
    }

    // ---------- questions fréquentes ----------

    void BuildFaq()
    {
        FaqList.Children.Clear();
        for (int i = 0; i < _p.Faq.Count; i++)
        {
            int index = i;
            var item = _p.Faq[i];
            bool open = i == _faqOpen;

            var content = new VerticalStackLayout { Spacing = 8 };
            var head = new Grid { ColumnSpacing = 8 };
            head.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            head.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            head.Add(new Label { Text = item.Q, FontSize = 15, FontAttributes = FontAttributes.Bold }, 0, 0);
            head.Add(new Icon { Glyph = "chevron", IconColor = Pal.Muted, Size = 16, Rotation = open ? 90 : 0, VerticalOptions = LayoutOptions.Start }, 1, 0);
            content.Children.Add(head);
            if (open)
                content.Children.Add(new Label { Text = item.A, FontSize = 14, TextColor = Pal.Muted });

            var card = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
                Stroke = new SolidColorBrush(Pal.Line),
                StrokeThickness = 1.5,
                BackgroundColor = Colors.White,
                Padding = new Thickness(14, 12),
                Content = content
            };
            card.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    _faqOpen = _faqOpen == index ? -1 : index;
                    BuildFaq();
                })
            });
            FaqList.Children.Add(card);
        }
    }

    // ---------- barre du haut et boutons ----------

    async void OnBack(object sender, TappedEventArgs e)
    {
        await Nav.GoAsync("..");
    }

    async void OnShare(object sender, TappedEventArgs e)
    {
        try
        {
            string text = _p.Title + "\n" + _p.Overview + "\nBureau : " + _p.Office + " · EnsPdMap, ENSPD.";
            await Share.Default.RequestAsync(new ShareTextRequest { Title = _p.Title, Text = text });
        }
        catch (Exception)
        {
            await DisplayAlert("Partager", "Le partage n'est pas disponible sur cet appareil.", "OK");
        }
    }

    void OnBookmark(object sender, TappedEventArgs e)
    {
        _bookmarked = !_bookmarked;
        ProgressStore.Update(_p, _bookmarked);
        ApplyBookmark();
    }

    async void OnDownload(object sender, TappedEventArgs e)
    {
        await Fiches.DownloadAsync(this, _p);
    }

    async void OnLocate(object sender, TappedEventArgs e)
    {
        if (string.IsNullOrEmpty(_p.ServiceId))
        {
            await Fiches.DownloadAsync(this, _p);
            return;
        }
        AppState.PlanTarget = _p.ServiceId;
        await Nav.GoAsync("//main/plan");
    }
}
