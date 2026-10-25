using EnsPdMap.Controls;
using EnsPdMap.Helpers;
using EnsPdMap.Models;
using Microsoft.Maui.Graphics;

namespace EnsPdMap.Pages;

public partial class PlanPage : ContentPage
{
    Service _sel;
    CampusPlace _selPlace;
    RouteResult _route = new RouteResult();
    int _floor = 1;
    bool _guiding;
    bool _picking;          // l'utilisateur doit toucher la carte pour dire « je suis ici »
    bool _fittedOnUser;

    // position de l'utilisateur sur le plan
    bool _userHave;
    bool _userOutside;
    PointD _userPoint;
    double _userOutsideMeters;
    DateTime _lastRefresh = DateTime.MinValue;

    // fiche à tirer
    bool _expanded;
    double _lastPanY;

    public PlanPage()
    {
        InitializeComponent();
        Map.MapTapped += OnMapTapped;
        Map.InsetTop = 205;
        Map.InsetBottom = 270;
        Map.Campus = Data.Campus;
        UpdateNorth();
        SelectFirst();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        PageFx.FadeIn(this);
        try
        {
            Map.Campus = Data.Campus;
            Locator.Changed += OnLocatorChanged;

            string target = AppState.PlanTarget;
            AppState.PlanTarget = "";
            if (!string.IsNullOrEmpty(target)) Select(target, true);
            else if (_sel == null) SelectFirst();

            UpdateGpsPill();
            await Locator.StartAsync();
            RefreshUser(true);
        }
        catch (Exception ex)
        {
            CrashLog.Write(ex, "Plan");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Locator.Changed -= OnLocatorChanged;
        Locator.Stop();
        _picking = false;
    }

    void SelectFirst()
    {
        foreach (var s in Data.Services)
        {
            if (Data.Campus.PlaceForService(s.Id) != null)
            {
                Select(s.Id, false);
                return;
            }
        }
    }

    void UpdateNorth()
    {
        Map.NorthAngle = Math.PI / 2 + CampusStore.Current.RotationDeg * Math.PI / 180;
    }

    // ---------- position de l'utilisateur ----------

    void OnLocatorChanged()
    {
        if ((DateTime.UtcNow - _lastRefresh).TotalMilliseconds < 350) return;
        RefreshUser(false);
    }

    void RefreshUser(bool fit)
    {
        _lastRefresh = DateTime.UtcNow;
        var cal = CampusStore.Current;
        UpdateNorth();

        _userHave = false;
        _userOutside = false;
        double accuracy = 10;

        if (Locator.Demo)
        {
            _userHave = true;
            _userPoint = Locator.DemoPoint;
            accuracy = 4;
        }
        else if (Locator.HasFix && cal.Calibrated)
        {
            _userHave = true;
            _userPoint = Geo.ToSchematic(cal, Locator.Lat, Locator.Lon);
            accuracy = Locator.Accuracy * cal.Scale;
        }

        if (_userHave && !Locator.Demo)
        {
            var entrance = Data.Campus.Node(Data.Campus.Entrance);
            if (entrance != null)
            {
                _userOutsideMeters = PointD.Dist(_userPoint, new PointD(entrance.X, entrance.Y)) / Math.Max(0.1, cal.Scale);
                _userOutside = _userOutsideMeters > 400;
            }
        }

        Map.HasUser = _userHave && !_userOutside;
        Map.User = _userPoint;
        Map.UserAccuracy = accuracy;
        Map.UserHeading = (Locator.Demo || double.IsNaN(Locator.HeadingDeg))
            ? double.NaN
            : Geo.HeadingToSchematic(cal, Locator.HeadingDeg);

        // première fois qu'on te voit sur le campus : on cadre toi + l'itinéraire
        if (Map.HasUser && !_fittedOnUser)
        {
            _fittedOnUser = true;
            fit = true;
        }

        UpdateGpsPill();
        RecomputeRoute(fit);
    }

    void UpdateGpsPill()
    {
        var cal = CampusStore.Current;
        string text;
        Color color;
        string status = Locator.Status ?? "";
        bool gpsProblem = status.Contains("refusée") || status.Contains("désactivé") || status.Contains("indisponible");

        if (_picking)
        {
            text = "Touchez la carte à l'endroit où vous êtes";
            color = Pal.GoldDark;
        }
        else if (Locator.Demo)
        {
            text = "Mode démo · touchez la carte pour vous placer";
            color = Pal.GoldDark;
        }
        else if (gpsProblem && !Locator.HasFix)
        {
            text = status;
            color = Pal.Red;
        }
        else if (!cal.Calibrated)
        {
            text = Locator.HasFix
                ? "GPS prêt · touchez ici pour vous placer sur le plan"
                : "Plan non étalonné · touchez ici";
            color = Pal.GoldDark;
        }
        else if (!Locator.HasFix)
        {
            text = string.IsNullOrEmpty(status) ? "Recherche du signal GPS…" : status;
            color = Pal.Muted;
        }
        else if (_userOutside)
        {
            text = "Hors campus · à " + Geo.FormatDistance(_userOutsideMeters);
            color = Pal.GoldDark;
        }
        else
        {
            text = "GPS actif · précision " + Math.Round(Locator.Accuracy) + " m";
            color = Pal.Green;
        }

        GpsLabel.Text = text;
        GpsLabel.TextColor = color;
        GpsDot.Color = color;
    }

    // ---------- « je suis ici » : placer le plan en touchant la carte ----------

    async Task LocateMeAsync()
    {
        if (_userHave && !_userOutside)
        {
            Map.CenterOn(_userPoint);
            return;
        }
        if (Locator.Demo)
        {
            Map.FitAll();
            return;
        }

        if (!CampusStore.Current.Calibrated)
        {
            string here = "Je suis ici (je touche la carte)";
            string guided = "Étalonner à l'entrée (guidé)";
            string choice = await DisplayActionSheet("Vous placer sur le plan", "Annuler", null, here, guided);
            if (choice == here)
            {
                if (!Locator.HasFix)
                {
                    await DisplayAlert("Pas encore de signal GPS",
                        "Attendez que « GPS prêt » s'affiche (en extérieur, GPS activé), ou utilisez le mode démo dans l'onglet Hors ligne.", "OK");
                    return;
                }
                _picking = true;
                UpdateGpsPill();
            }
            else if (choice == guided)
            {
                await Nav.GoAsync("etalonnage");
            }
            return;
        }

        if (!Locator.HasFix)
        {
            await DisplayAlert("Pas encore de signal GPS",
                "Placez-vous à l'extérieur et vérifiez que la localisation du téléphone est activée. Sans GPS, utilisez le mode démo (onglet Hors ligne).", "OK");
            return;
        }
        Map.FitAll();
    }

    async Task ApplyPickedPosition(PointD point)
    {
        _picking = false;
        if (!Locator.HasFix)
        {
            UpdateGpsPill();
            return;
        }

        Calibration cal;
        var existing = CampusStore.Current;
        if (CampusStore.IsLocal && existing.Calibrated)
        {
            var move = Geo.EastNorth(existing.OriginLat, existing.OriginLon, Locator.Lat, Locator.Lon);
            double meters = Math.Sqrt(move.X * move.X + move.Y * move.Y);
            // assez loin du premier point : on règle aussi l'orientation du plan
            cal = meters >= 20
                ? Geo.FromTwoAnchors(existing, Locator.Lat, Locator.Lon, point)
                : Geo.FromOneAnchor(Locator.Lat, Locator.Lon, point);
        }
        else
        {
            cal = Geo.FromOneAnchor(Locator.Lat, Locator.Lon, point);
        }

        CampusStore.Save(cal);
        RefreshUser(true);
        await DisplayAlert("Position enregistrée",
            "Le plan est calé sur votre position. Marchez 20 m puis touchez de nouveau « je suis ici » pour affiner l'orientation.", "OK");
    }

    // ---------- sélection et itinéraire ----------

    void Select(string serviceId, bool fit)
    {
        _sel = Data.GetService(serviceId);
        _selPlace = Data.Campus.PlaceForService(_sel.Id);
        _floor = _sel.Floor;
        Map.SelectedPlaceId = _selPlace != null ? _selPlace.Id : "";
        Map.DestinationLabel = _sel.ShortName;
        UpdateFloors();
        RecomputeRoute(fit);
    }

    PointD StartPoint()
    {
        if (_userHave && !_userOutside) return _userPoint;
        var entrance = Data.Campus.Node(Data.Campus.Entrance);
        return entrance != null ? new PointD(entrance.X, entrance.Y) : new PointD(0, 0);
    }

    void RecomputeRoute(bool fit)
    {
        if (_sel == null) return;
        var cal = CampusStore.Current;

        _route = new RouteResult();
        Map.HasDestination = false;
        if (_selPlace != null && !string.IsNullOrEmpty(_selPlace.EntranceNode))
        {
            _route = Geo.Route(Data.Campus, cal, StartPoint(), _selPlace.EntranceNode, _sel.SheetTitle);
            var door = Data.Campus.Node(_selPlace.EntranceNode);
            if (door != null)
            {
                Map.HasDestination = true;
                Map.Destination = new PointD(door.X, door.Y);
            }
        }
        Map.RoutePoints = _route.Points;

        if (fit && Map.Width > 0)
        {
            var pts = new List<PointD>(_route.Points);
            if (pts.Count == 0 && Map.HasDestination) pts.Add(Map.Destination);
            Map.FitTo(pts, 50);
        }
        Map.Invalidate();

        FillSheet();
        if (_guiding) BuildGuide();
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

        var dimmed = new HashSet<string>();
        foreach (var place in Data.Campus.Places)
        {
            if (string.IsNullOrEmpty(place.ServiceId)) continue;
            if (Data.GetService(place.ServiceId).Floor != _floor) dimmed.Add(place.Id);
        }
        Map.Dimmed = dimmed;
        Map.Invalidate();
    }

    void FillSheet()
    {
        if (_sel == null) return;
        var cal = CampusStore.Current;

        SheetTitle.Text = _sel.SheetTitle;
        SheetSub.Text = _sel.Block + " · " + _sel.FloorName;

        bool open = _sel.Status == "Ouvert";
        StatusLabel.Text = _sel.Status;
        StatusLabel.TextColor = open ? Pal.Green : Pal.Red;
        StatusBadge.BackgroundColor = open ? Pal.GreenSoft : Pal.RedSoft;

        bool fromUser = _userHave && !_userOutside;
        if (_route.Length > 0)
        {
            int minutes = Math.Max(1, (int)Math.Ceiling(_route.Length / Geo.WalkMetersPerMinute));
            SheetDistance.Text = Geo.FormatDistance(_route.Length) + " · environ " + minutes + " min " +
                                 (fromUser ? "depuis votre position" : "depuis l'entrée");
        }
        else
        {
            SheetDistance.Text = "Itinéraire indisponible pour ce lieu";
        }

        // situation du lieu par rapport à l'utilisateur
        string situation = "";
        var door = _selPlace != null ? Data.Campus.Node(_selPlace.EntranceNode) : null;
        if (fromUser && door != null)
        {
            var target = new PointD(door.X, door.Y);
            situation = "Direction : " + Geo.Cardinal(Geo.TrueBearing(cal, _userPoint, target));
            if (!Locator.Demo && !double.IsNaN(Locator.HeadingDeg))
                situation += " · " + Geo.RelativeSide(Geo.HeadingToSchematic(cal, Locator.HeadingDeg), _userPoint, target);
        }
        else if (_userHave && _userOutside)
        {
            situation = "Vous êtes à " + Geo.FormatDistance(_userOutsideMeters) + " du campus : l'itinéraire part de l'entrée.";
        }
        else if (!cal.Calibrated && !Locator.Demo)
        {
            situation = "Touchez le point GPS en haut pour vous placer sur le plan.";
        }
        if (!open) situation = (situation.Length > 0 ? situation + " · " : "") + _sel.NextOpening;
        SheetSituation.Text = situation;

        SheetQueue.Text = open ? _sel.Queue : "Fermé · " + _sel.NextOpening;
        SheetQueue.TextColor = open ? Pal.GoldDark : Pal.Red;
        SheetQueueBox.BackgroundColor = open ? Pal.Sand : Pal.RedSoft;
        SheetHours.Text = _sel.Hours.Count > 0 ? _sel.Hours[0].Time : "—";
        SheetPhone.Text = _sel.Phone;
        SheetMail.Text = _sel.Email;
    }

    void BuildGuide()
    {
        GuideBox.IsVisible = true;
        GuideSteps.Children.Clear();

        bool fromUser = _userHave && !_userOutside;
        GuideTitle.Text = "Itinéraire pas à pas · " + Geo.FormatDistance(_route.Length);
        if (!fromUser)
        {
            GuideSteps.Children.Add(new Label
            {
                Text = "Départ : entrée principale du campus.",
                FontSize = 14,
                TextColor = Pal.Muted
            });
        }

        for (int i = 0; i < _route.Instructions.Count; i++)
        {
            var row = new Grid { ColumnSpacing = 10 };
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            row.Add(new Label
            {
                Text = (i + 1) + ".",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Pal.Gold,
                VerticalOptions = LayoutOptions.Start
            }, 0, 0);
            row.Add(new Label { Text = _route.Instructions[i], FontSize = 14 }, 1, 0);
            GuideSteps.Children.Add(row);
        }
    }

    // ---------- carte ----------

    async void OnMapTapped(object sender, MapTapArgs e)
    {
        try
        {
            if (_picking)
            {
                await ApplyPickedPosition(e.Point);
                return;
            }

            if (!string.IsNullOrEmpty(e.PlaceId))
            {
                foreach (var place in Data.Campus.Places)
                {
                    if (place.Id == e.PlaceId && !string.IsNullOrEmpty(place.ServiceId))
                    {
                        Select(place.ServiceId, true);
                        return;
                    }
                }
            }

            if (Locator.Demo) Locator.SetDemoPoint(e.Point);
        }
        catch (Exception ex)
        {
            CrashLog.Write(ex, "Plan tap");
        }
    }

    async void OnGpsPill(object sender, TappedEventArgs e)
    {
        await LocateMeAsync();
    }

    void OnZoomIn(object sender, TappedEventArgs e)
    {
        Map.ZoomBy(1.3);
    }

    void OnZoomOut(object sender, TappedEventArgs e)
    {
        Map.ZoomBy(1 / 1.3);
    }

    async void OnLocate(object sender, TappedEventArgs e)
    {
        await LocateMeAsync();
    }

    async void OnSearch(object sender, EventArgs e)
    {
        string q = AppState.Norm((SearchEntry.Text ?? "").Trim());
        if (q.Length == 0) return;
        foreach (var s in Data.Services)
        {
            string hay = AppState.Norm(s.Name + " " + s.Description + " " + s.ListLoc + " " + s.ShortName);
            if (hay.Contains(q) && Data.Campus.PlaceForService(s.Id) != null)
            {
                Select(s.Id, true);
                return;
            }
        }
        await DisplayAlert("Recherche", "Aucun bureau ne correspond à « " + SearchEntry.Text + " ».", "OK");
    }

    // ---------- boutons de la fiche ----------

    async void OnRoute(object sender, TappedEventArgs e)
    {
        _guiding = true;
        BuildGuide();
        if (Map.Width > 0) Map.FitTo(new List<PointD>(_route.Points), 50);
        await SetExpanded(true);
    }

    async void OnSheet(object sender, TappedEventArgs e)
    {
        if (_sel != null) await Nav.GoAsync("bureau?id=" + _sel.Id);
    }

    // ---------- fiche : tirer vers le haut = afficher le détail (défilable) ----------

    async Task SetExpanded(bool value)
    {
        try
        {
            if (value == _expanded && ExtraScroll.IsVisible == value) return;
            _expanded = value;
            SheetChevron.Rotation = value ? 90 : -90;

            if (value)
            {
                ExtraScroll.Opacity = 0;
                ExtraScroll.IsVisible = true;
                await ExtraScroll.FadeTo(1, 170, Easing.CubicOut);
            }
            else
            {
                await ExtraScroll.FadeTo(0, 110);
                ExtraScroll.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            CrashLog.Write(ex, "Fiche");
        }
    }

    async void OnSheetHandle(object sender, TappedEventArgs e)
    {
        await SetExpanded(!_expanded);
    }

    async void OnSheetSwipe(object sender, SwipedEventArgs e)
    {
        if (e.Direction == SwipeDirection.Up) await SetExpanded(true);
        else if (e.Direction == SwipeDirection.Down) await SetExpanded(false);
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
            if (_lastPanY < -12) await SetExpanded(true);
            else if (_lastPanY > 12) await SetExpanded(false);
        }
    }
}
