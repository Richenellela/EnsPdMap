using EnsPdMap.Models;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;

namespace EnsPdMap.Helpers;

/// <summary>Téléchargement des fiches PDF (fournies avec l'application, donc disponibles hors ligne).</summary>
public static class Fiches
{
    public static async Task<string> SaveAsync(string fileName)
    {
        string dir = System.IO.Path.Combine(FileSystem.AppDataDirectory, "fiches");
        Directory.CreateDirectory(dir);
        string target = System.IO.Path.Combine(dir, fileName);

        using (var input = await FileSystem.OpenAppPackageFileAsync(fileName))
        using (var output = File.Create(target))
        {
            await input.CopyToAsync(output);
        }
        return target;
    }

    public static async Task DownloadAsync(Page page, Procedure p)
    {
        try
        {
            string path = await SaveAsync(p.FicheFile);
            string open = "Ouvrir la fiche";
            string share = "Partager / enregistrer ailleurs";
            string choice = await page.DisplayActionSheet("Fiche téléchargée", "Fermer", null, open, share);

            if (choice == open)
            {
                try
                {
                    await Launcher.Default.OpenAsync(new OpenFileRequest(p.Title, new ReadOnlyFile(path, "application/pdf")));
                }
                catch (Exception)
                {
                    await page.DisplayAlert("Aucun lecteur PDF",
                        "Installez un lecteur PDF, ou utilisez « Partager / enregistrer ailleurs » pour conserver la fiche.", "OK");
                }
            }
            else if (choice == share)
            {
                await Share.Default.RequestAsync(new ShareFileRequest(p.Title, new ShareFile(path, "application/pdf")));
            }
        }
        catch (Exception ex)
        {
            CrashLog.Write(ex, "Fiche PDF");
            await page.DisplayAlert("Téléchargement impossible", ex.Message, "OK");
        }
    }
}
