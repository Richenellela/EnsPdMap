using System.Text.Json;
using EnsPdMap.Helpers;
using Microsoft.Maui.Storage;

namespace EnsPdMap.Models;

public class ProgressRecord
{
    public List<int> ReadyPieces { get; set; } = new List<int>();
    public List<int> DoneSteps { get; set; } = new List<int>();
    public bool Bookmarked { get; set; }
}

/// <summary>Avancement de l'étudiant : enregistré sur le téléphone, et envoyé à l'API quand il est connecté.</summary>
public static class ProgressStore
{
    const string Key = "progress_json";
    static Dictionary<string, ProgressRecord> _map;
    static readonly JsonSerializerOptions Json = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    static Dictionary<string, ProgressRecord> Map
    {
        get
        {
            if (_map == null)
            {
                try
                {
                    string raw = Preferences.Default.Get(Key, "");
                    _map = string.IsNullOrEmpty(raw)
                        ? new Dictionary<string, ProgressRecord>()
                        : JsonSerializer.Deserialize<Dictionary<string, ProgressRecord>>(raw, Json) ?? new Dictionary<string, ProgressRecord>();
                }
                catch (Exception)
                {
                    _map = new Dictionary<string, ProgressRecord>();
                }
            }
            return _map;
        }
    }

    static void Save()
    {
        try
        {
            Preferences.Default.Set(Key, JsonSerializer.Serialize(Map, Json));
        }
        catch (Exception)
        {
            // pas bloquant
        }
    }

    public static ProgressRecord Get(string procedureId)
    {
        ProgressRecord r;
        return Map.TryGetValue(procedureId, out r) ? r : new ProgressRecord();
    }

    public static void ApplyTo(Procedure p)
    {
        var r = Get(p.Id);
        for (int i = 0; i < p.Pieces.Count; i++) p.Pieces[i].Ready = r.ReadyPieces.Contains(i);
        for (int i = 0; i < p.Steps.Count; i++) p.Steps[i].Done = r.DoneSteps.Contains(i);
    }

    public static bool IsBookmarked(string procedureId)
    {
        return Get(procedureId).Bookmarked;
    }

    /// <summary>Enregistre l'état actuel de la démarche (pièces, étapes, favori) et l'envoie à l'API.</summary>
    public static void Update(Procedure p, bool? bookmarked = null)
    {
        var r = Get(p.Id);
        r.ReadyPieces = new List<int>();
        r.DoneSteps = new List<int>();
        for (int i = 0; i < p.Pieces.Count; i++) if (p.Pieces[i].Ready) r.ReadyPieces.Add(i);
        for (int i = 0; i < p.Steps.Count; i++) if (p.Steps[i].Done) r.DoneSteps.Add(i);
        if (bookmarked.HasValue) r.Bookmarked = bookmarked.Value;
        Map[p.Id] = r;
        Save();
        _ = ApiClient.PushProgressAsync(p.Id, r);
    }

    /// <summary>Reprend l'avancement enregistré sur le serveur (il remplace celui du téléphone).</summary>
    public static void MergeFromServer(IEnumerable<ApiProgress> items)
    {
        foreach (var it in items)
        {
            Map[it.ProcedureId] = new ProgressRecord
            {
                ReadyPieces = it.ReadyPieces ?? new List<int>(),
                DoneSteps = it.DoneSteps ?? new List<int>(),
                Bookmarked = it.Bookmarked
            };
        }
        Save();
        foreach (var p in Data.Procedures) ApplyTo(p);
    }
}

public class ApiProgress
{
    public string ProcedureId { get; set; } = "";
    public List<int> ReadyPieces { get; set; } = new List<int>();
    public List<int> DoneSteps { get; set; } = new List<int>();
    public bool Bookmarked { get; set; }
}
