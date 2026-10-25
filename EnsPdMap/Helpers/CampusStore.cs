using System.Text.Json;
using EnsPdMap.Models;
using Microsoft.Maui.Storage;

namespace EnsPdMap.Helpers;

/// <summary>Étalonnage du plan : enregistré sur le téléphone (prioritaire) ou reçu de l'API (partagé).</summary>
public static class CampusStore
{
    const string Key = "calibration_json";
    static readonly JsonSerializerOptions Json = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public static Calibration Current
    {
        get
        {
            try
            {
                string raw = Preferences.Default.Get(Key, "");
                if (!string.IsNullOrEmpty(raw))
                {
                    var local = JsonSerializer.Deserialize<Calibration>(raw, Json);
                    if (local != null && local.Calibrated) return local;
                }
            }
            catch (Exception)
            {
                // on retombe sur l'étalonnage partagé
            }
            return Data.SharedCalibration ?? new Calibration();
        }
    }

    public static bool IsLocal
    {
        get { return !string.IsNullOrEmpty(Preferences.Default.Get(Key, "")); }
    }

    public static void Save(Calibration c)
    {
        Preferences.Default.Set(Key, JsonSerializer.Serialize(c, Json));
    }

    public static void Reset()
    {
        Preferences.Default.Remove(Key);
    }
}
