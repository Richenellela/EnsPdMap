namespace EnsPdMap.Helpers;

/// <summary>Petite animation d'apparition des écrans (fondu rapide).</summary>
public static class PageFx
{
    public static async void FadeIn(ContentPage page)
    {
        try
        {
            var content = page.Content;
            if (content == null) return;
            content.Opacity = 0;
            await content.FadeTo(1, 130, Easing.CubicOut);
        }
        catch (Exception)
        {
            // l'animation est facultative
        }
    }
}
