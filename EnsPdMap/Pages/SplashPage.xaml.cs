using EnsPdMap.Helpers;

namespace EnsPdMap.Pages;

/// <summary>Écran d'ouverture animé : logo, nom, accroche, puis passage à la connexion.</summary>
public partial class SplashPage : ContentPage
{
    bool _started;

    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_started) return;
        _started = true;

        try
        {
            await Task.WhenAll(Logo.FadeTo(1, 320), Logo.ScaleTo(1, 520, Easing.SpringOut));
            await AppName.FadeTo(1, 220);
            await Tagline.FadeTo(1, 220);
            await Dots.FadeTo(1, 120);

            // petit chargement animé
            var dots = new[] { Dot1, Dot2, Dot3 };
            for (int round = 0; round < 2; round++)
            {
                foreach (var dot in dots)
                {
                    await dot.FadeTo(1, 90);
                    await dot.FadeTo(0.4, 90);
                }
            }
        }
        catch (Exception)
        {
            // l'animation est facultative
        }

        await Nav.GoAsync("//login");
    }
}
