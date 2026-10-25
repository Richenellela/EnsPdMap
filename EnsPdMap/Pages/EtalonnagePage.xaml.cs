using System.Globalization;
using EnsPdMap.Helpers;
using EnsPdMap.Models;
using Microsoft.Maui.Graphics;

namespace EnsPdMap.Pages;

public partial class EtalonnagePage : ContentPage
{
    public EtalonnagePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            Locator.Changed += OnLocatorChanged;
            UpdateStatus();
            UpdateInfo();
            await Locator.StartAsync();
            UpdateInfo();
        }
        catch (Exception ex)
        {
            CrashLog.Write(ex, "Etalonnage");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Locator.Changed -= OnLocatorChanged;
        Locator.Stop();
    }

    void OnLocatorChanged()
    {
        UpdateInfo();
    }

    void UpdateInfo()
    {
        if (Locator.HasFix)
        {
            string lat = Locator.Lat.ToString("0.00000", CultureInfo.InvariantCulture);
            string lon = Locator.Lon.ToString("0.00000", CultureInfo.InvariantCulture);
            GpsInfo.Text = lat + ", " + lon + " · précision " + Math.Round(Locator.Accuracy) + " m";
            GpsInfo.TextColor = Locator.Accuracy <= 15 ? Pal.Green : Pal.GoldDark;
        }
        else
        {
            GpsInfo.Text = string.IsNullOrEmpty(Locator.Status) ? "Recherche du signal GPS…" : Locator.Status;
            GpsInfo.TextColor = Pal.Muted;
        }
    }

    void UpdateStatus()
    {
        var cal = CampusStore.Current;
        if (CampusStore.IsLocal && cal.Calibrated)
        {
            bool two = Math.Abs(cal.RotationDeg) > 0.01 || Math.Abs(cal.Scale - 1) > 0.001;
            CalStatus.Text = two
                ? "Plan étalonné (2 points) · rotation " + Math.Round(cal.RotationDeg) + "° · échelle " + cal.Scale.ToString("0.00", CultureInfo.InvariantCulture)
                : "Plan étalonné (1 point : entrée principale).";
            CalStatus.TextColor = Pal.Green;
            StatusBox.BackgroundColor = Pal.GreenSoft;
        }
        else if (cal.Calibrated)
        {
            CalStatus.Text = "Étalonnage partagé par d'autres étudiants utilisé.";
            CalStatus.TextColor = Pal.Green;
            StatusBox.BackgroundColor = Pal.GreenSoft;
        }
        else
        {
            CalStatus.Text = "Plan non étalonné.";
            CalStatus.TextColor = Pal.GoldDark;
            StatusBox.BackgroundColor = Pal.Sand;
        }
    }

    async Task<bool> HasUsableFix()
    {
        if (!Locator.HasFix)
        {
            await DisplayAlert("Pas de signal GPS", "Attendez que la position apparaisse en haut de l'écran (en extérieur, GPS activé).", "OK");
            return false;
        }
        if (Locator.Accuracy > 30)
        {
            return await DisplayAlert("Précision faible",
                "La précision actuelle est de " + Math.Round(Locator.Accuracy) + " m. Continuer quand même ?", "Continuer", "Attendre");
        }
        return true;
    }

    async void OnAnchor1(object sender, EventArgs e)
    {
        if (!await HasUsableFix()) return;

        var entrance = Data.Campus.Node(Data.Campus.Entrance);
        if (entrance == null) return;

        CampusStore.Save(Geo.FromOneAnchor(Locator.Lat, Locator.Lon, new PointD(entrance.X, entrance.Y)));
        UpdateStatus();
        await DisplayAlert("Entrée enregistrée", "Le plan est posé sur votre position. Pour régler l'orientation, faites l'étape 2.", "OK");
    }

    async void OnAnchor2(object sender, EventArgs e)
    {
        if (!CampusStore.IsLocal || !CampusStore.Current.Calibrated)
        {
            await DisplayAlert("Étape 1 d'abord", "Faites d'abord l'étape 1 à l'entrée principale.", "OK");
            return;
        }
        if (!await HasUsableFix()) return;

        var first = CampusStore.Current;
        var move = Geo.EastNorth(first.OriginLat, first.OriginLon, Locator.Lat, Locator.Lon);
        double meters = Math.Sqrt(move.X * move.X + move.Y * move.Y);
        if (meters < 15)
        {
            await DisplayAlert("Trop près de l'entrée", "Vous êtes à " + Math.Round(meters) + " m de l'entrée : éloignez-vous d'au moins 15 m, devant la porte du Bloc A.", "OK");
            return;
        }

        var place = Data.Campus.PlaceForService("sco");
        var door = place != null ? Data.Campus.Node(place.EntranceNode) : null;
        if (door == null) return;

        CampusStore.Save(Geo.FromTwoAnchors(first, Locator.Lat, Locator.Lon, new PointD(door.X, door.Y)));
        UpdateStatus();
        await DisplayAlert("Orientation enregistrée", "Le plan est maintenant orienté. Retournez sur le plan pour vous guider.", "OK");
    }

    async void OnShare(object sender, EventArgs e)
    {
        var cal = CampusStore.Current;
        if (!cal.Calibrated)
        {
            await DisplayAlert("Rien à partager", "Étalonnez d'abord le plan.", "OK");
            return;
        }
        var result = await ApiClient.PushCalibrationAsync(cal);
        if (result.Ok)
            await DisplayAlert("Partagé", "Les autres étudiants utiliseront cet étalonnage s'ils n'ont pas le leur.", "OK");
        else
            await DisplayAlert("Partage impossible", result.NetworkError ? "Serveur injoignable." : result.Message, "OK");
    }

    async void OnReset(object sender, EventArgs e)
    {
        bool ok = await DisplayAlert("Réinitialiser", "Supprimer l'étalonnage de ce téléphone ?", "Supprimer", "Annuler");
        if (!ok) return;
        CampusStore.Reset();
        UpdateStatus();
    }

    async void OnBack(object sender, TappedEventArgs e)
    {
        await Nav.GoAsync("..");
    }
}
