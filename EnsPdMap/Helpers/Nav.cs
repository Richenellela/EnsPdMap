namespace EnsPdMap.Helpers;

/// <summary>Navigation protégée : une erreur affiche un message au lieu de fermer l'application.</summary>
public static class Nav
{
    public static async Task GoAsync(string route)
    {
        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            CrashLog.Write(ex, "Navigation " + route);
            try
            {
                var page = Shell.Current != null ? Shell.Current.CurrentPage : null;
                if (page != null)
                    await page.DisplayAlert("Erreur de navigation", ex.Message, "OK");
            }
            catch (Exception)
            {
                // rien de plus à faire
            }
        }
    }
}
