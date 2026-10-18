using EnsPdMap.Controls;
using EnsPdMap.Helpers;
using EnsPdMap.Models;
using Microsoft.Maui.Controls; // AbsoluteLayout, AbsoluteLayout.SetLayoutBounds, ...
using Microsoft.Maui.Layouts;   // Ajouté pour AbsoluteLayoutFlags
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EnsPdMap.Pages;

public partial class PlanPage : ContentPage
{
    class Blk
    {
        public string ServiceId = "";
        public Border View;
        public Icon Pin;
        public Label Text;
    }

    readonly List<Blk> _blocks = new List<Blk>();
    readonly List<View> _routeViews = new List<View>();
    Service _sel;
    int _floor = 1;
    double _scale = 1.0;

    // Coordonnées en proportion de la carte : x, y, largeur, hauteur
    static readonly double[][] Decor =
    {
        new[] { 0.00, 0.00, 0.42, 0.10 },
        new[] { 0.58, 0.60, 0.42, 0.08 },
        new[] { 0.58, 0.49, 0.18, 0.08 },
        new[] { 0.80, 0.49, 0.20, 0.08 },
        new[] { 0.00, 0.56, 0.32, 0.06 }
    };

    static readonly string[] BlockIds = { "sco", "dgi", "cai", "med" };
    static readonly double[][] BlockRects =
    {
        new[] { 0.00, 0.15, 0.42, 0.13 },
        new[] { 0.58, 0.12, 0.42, 0.16 },
        new[] { 0.00, 0.34, 0.32, 0.17 },
        new[] { 0.58, 0.34, 0.42, 0.11 }
    };

    bool _expanded;
    bool _animating;
    double _collapsed = 230;
    double _lastPanY;

    const double EntranceX = 0.50;
    const double EntranceY = 0.62;

    public PlanPage()
    {
        InitializeComponent();
        BuildMap();
        Select("sco");
        Sheet.SizeChanged += OnSheetSizeChanged;
    }

    void OnSheetSizeChanged(object sender, EventArgs e)
    {
        if (ExtraBox.Height > 0)
        {
            _collapsed = ExtraBox.Height + ExtraBox.Margin.Top;
            if (!_expanded && !_animating)
                Sheet.TranslationY = _collapsed;
        }
    }

    async Task SetExpanded(bool value)
    {
        try
        {
            _expanded = value;
            _animating = true;
            await Sheet.TranslateTo(0, value ? 0 : _collapsed, 260, Easing.CubicOut);
        }
        catch (Exception ex)
        {
            CrashLog.Write(ex, "Fiche");
        }
        finally
        {
            _animating = false;
        }
    }

    async void OnSheetHandle(object sender, TappedEventArgs e)
    {
        await SetExpanded(!_expanded);
    }

    async void OnSheetPan(object sender, PanUpdatedEventArgs e)
    {
        if (e.StatusType == GestureStatus.Started)
        {
            _lastPanY = 0;
        }
        else if (e.StatusType == GestureStatus.Running)
        {
            _lastPanY = e.TotalY;
        }
        else if (e.StatusType == GestureStatus.Completed)
        {
            if (_lastPanY < -20) await SetExpanded(true);
            else if (_lastPanY > 20) await SetExpanded(false);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!string.IsNullOrEmpty(AppState.PlanTarget))
        {
            string id = AppState.PlanTarget;
            AppState.PlanTarget = "";
            Select(id);
        }
    }

    static Rect B(double x, double y, double w, double h)
    {
        // Avec AbsoluteLayoutFlags.All, x et y sont proportionnels à l'espace restant
        return new Rect(w >= 1 ? 0 : x / (1 - w), h >= 1 ? 0 : y / (1 - h), w, h);
    }

    void Place(View v, double x, double y, double w, double h)
    {
        AbsoluteLayout.SetLayoutFlags(v, AbsoluteLayoutFlags.All);
        AbsoluteLayout.SetLayoutBounds(v, B(x, y, w, h));
        MapLayer.Children.Add(v);
    }

    void BuildMap()
    {
        MapLayer.Children.Clear();
        _blocks.Clear();

        for (int i = 0; i < Decor.Length; i++)
        {
            bool park = i < 2;
            var d = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
                Stroke = new SolidColorBrush(park ? Pal.Park : Pal.BlockLine),
                StrokeThickness = 1,
                BackgroundColor = park ? Pal.Park : Pal.Block,
                InputTransparent = true
            };
            Place(d, Decor[i][0], Decor[i][1], Decor[i][2], Decor[i][3]);
        }

        for (int i = 0; i < BlockIds.Length; i++)
        {
            var svc = Data.GetService(BlockIds[i]);
            var pin = new Icon { Glyph = "pin", IconColor = Pal.Navy, Size = 28, IsVisible = false, HorizontalOptions = LayoutOptions.Center };
            var label = new Label
            {
                Text = svc.Short,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Pal.Navy,
                HorizontalTextAlignment = TextAlignment.Center
            };
            var inner = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
            inner.Children.Add(pin);
            inner.Children.Add(label);

            var block = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
                Stroke = new SolidColorBrush(Pal.BlockLine),
                StrokeThickness = 1,
                BackgroundColor = Pal.Block,
                Content = inner
            };
            string id = BlockIds[i];
            block.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => Select(id)) });
            Place(block, BlockRects[i][0], BlockRects[i][1], BlockRects[i][2], BlockRects[i][3]);
            _blocks.Add(new Blk { ServiceId = id, View = block, Pin = pin, Text = label });
        }

        // Entrée principale
        var entrance = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
            StrokeThickness = 0,
            BackgroundColor = Pal.Navy,
            Padding = new Thickness(10, 4),
            InputTransparent = true,
            Content = new Label { Text = "Entrée", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Colors.White }
        };
        AbsoluteLayout.SetLayoutFlags(entrance, AbsoluteLayoutFlags.PositionProportional);
        AbsoluteLayout.SetLayoutBounds(entrance, new Rect(EntranceX, EntranceY + 0.01, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
        MapLayer.Children.Add(entrance);
    }

    void DrawRoute()
    {
        foreach (var v in _routeViews) MapLayer.Children.Remove(v);
        _routeViews.Clear();
        if (_sel == null) return;

        int idx = Array.IndexOf(BlockIds, _sel.Id);
        if (idx < 0) return;
        double[] r = BlockRects[idx];
        double cy = r[1] + r[3] / 2;
        bool left = r[0] < 0.5;
        double edge = left ? r[0] + r[2] : r[0];

        // Segment vertical sur la route centrale
        var vert = new BoxView { Color = Pal.Navy, CornerRadius = 2, InputTransparent = true };
        Place(vert, EntranceX - 0.005, cy, 0.010, EntranceY - cy);

        // Segment horizontal vers le bâtiment
        double hx = left ? edge : EntranceX;
        double hw = Math.Abs(EntranceX - edge);
        var horiz = new BoxView { Color = Pal.Navy, CornerRadius = 2, InputTransparent = true };
        Place(horiz, hx, cy - 0.003, hw, 0.006);

        _routeViews.Add(vert);
        _routeViews.Add(horiz);
    }

    void Select(string id)
    {
        _sel = Data.GetService(id);
        _floor = _sel.Floor;

        foreach (var b in _blocks)
        {
            bool selected = b.ServiceId == _sel.Id;
            b.View.BackgroundColor = selected ? Pal.Gold : Pal.Block;
            b.View.Stroke = new SolidColorBrush(selected ? Pal.Navy : Pal.BlockLine);
            b.View.StrokeThickness = selected ? 3 : 1;
            b.Pin.IsVisible = selected;
            b.Text.TextColor = selected ? Colors.White : Pal.Navy;
        }

        DrawRoute();
        UpdateFloors();
        FillSheet();
    }

    void UpdateFloors()
    {
        FloorChips.Children.Clear();
        string[] names = { "RDC", "1er étage", "2e étage" };
        for (int i = 0; i < names.Length; i++)
        {
            int floor = i;
            FloorChips.Children.Add(Ui.Chip(names[i], floor == _floor, () => { _floor = floor; UpdateFloors(); }));
        }
        foreach (var b in _blocks)
        {
            var svc = Data.GetService(b.ServiceId);
            b.View.Opacity = svc.Floor == _floor ? 1.0 : 0.4;
        }
    }

    void FillSheet()
    {
        SheetTitle.Text = _sel.SheetTitle;
        SheetSub.Text = _sel.Block + " · " + _sel.FloorName;
        SheetDistance.Text = _sel.Distance + " m · environ " + _sel.Minutes + " min depuis l'entrée";
        bool open = _sel.Status == "Ouvert";
        StatusLabel.Text = _sel.Status;
        StatusLabel.TextColor = open ? Pal.Green : Pal.Red;
        StatusBadge.BackgroundColor = open ? Pal.GreenSoft : Pal.RedSoft;

        SheetQueue.Text = _sel.Queue;
        SheetQueue.TextColor = open ? Pal.GoldDark : Pal.Red;
        SheetQueueBox.BackgroundColor = open ? Pal.Sand : Pal.RedSoft;
        SheetHours.Text = _sel.Hours.Count > 0 ? _sel.Hours[0][1] : "—";
        SheetPhone.Text = _sel.Phone;
        SheetMail.Text = _sel.Email;
    }

    async void OnSearch(object sender, EventArgs e)
    {
        string q = AppState.Norm((SearchEntry.Text ?? "").Trim());
        if (q.Length == 0) return;
        foreach (var s in Data.Services)
        {
            string hay = AppState.Norm(s.Name + " " + s.Description + " " + s.ListLoc + " " + s.Short);
            if (hay.Contains(q))
            {
                Select(s.Id);
                return;
            }
        }
        await DisplayAlert("Recherche", "Aucun bureau ne correspond à « " + SearchEntry.Text + " ».", "OK");
    }

    void ApplyScale()
    {
        MapLayer.Scale = _scale;
    }

    void OnZoomIn(object sender, TappedEventArgs e)
    {
        _scale = Math.Min(1.8, _scale + 0.2);
        ApplyScale();
    }

    void OnZoomOut(object sender, TappedEventArgs e)
    {
        _scale = Math.Max(0.8, _scale - 0.2);
        ApplyScale();
    }

    void OnLocate(object sender, TappedEventArgs e)
    {
        _scale = 1.0;
        ApplyScale();
    }

    async void OnRoute(object sender, TappedEventArgs e)
    {
        await DisplayAlert("Itinéraire",
            "Depuis l'entrée principale, suivez l'allée centrale jusqu'à " + _sel.Block + " (" + _sel.FloorName + ").\n\n" +
            _sel.Distance + " m · environ " + _sel.Minutes + " min à pied.",
            "OK");
    }

    async void OnSheet(object sender, TappedEventArgs e)
    {
        await Nav.GoAsync("bureau?id=" + _sel.Id);
    }
}
