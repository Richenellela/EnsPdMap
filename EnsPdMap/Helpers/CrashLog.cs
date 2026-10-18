using Microsoft.Maui.Storage;

namespace EnsPdMap.Helpers;

/// <summary>Écrit la dernière erreur dans un fichier pour l'afficher au prochain démarrage.</summary>
public static class CrashLog
{
    static string FilePath
    {
        get { return System.IO.Path.Combine(FileSystem.AppDataDirectory, "dernier-plantage.log"); }
    }

    public static void Write(Exception ex, string source)
    {
        try
        {
            File.WriteAllText(FilePath, DateTime.Now.ToString("g") + " [" + source + "]" + Environment.NewLine + ex);
        }
        catch (Exception)
        {
            // le journal ne doit jamais provoquer d'erreur
        }
    }

    public static string ReadAndClear()
    {
        try
        {
            if (!File.Exists(FilePath)) return null;
            string text = File.ReadAllText(FilePath);
            File.Delete(FilePath);
            return text;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
