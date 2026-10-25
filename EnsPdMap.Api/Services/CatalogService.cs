using System.Text.Json;
using EnsPdMap.Api.Models;

namespace EnsPdMap.Api.Services;

/// <summary>Charge catalogue.json (services, démarches, campus) en mémoire au démarrage.</summary>
public class CatalogService
{
    public CatalogFile Catalog { get; }

    public CatalogService()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "catalogue.json");
        if (!File.Exists(path))
            throw new FileNotFoundException("catalogue.json introuvable : " + path);

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Catalog = JsonSerializer.Deserialize<CatalogFile>(File.ReadAllText(path), options)
                  ?? throw new InvalidOperationException("catalogue.json est vide ou invalide.");
    }

    /// <summary>Applique les signalements d'affluence des 45 dernières minutes aux services.</summary>
    public List<ServiceInfo> ServicesWithCrowd(IEnumerable<CrowdReport> recent, DateTime nowUtc)
    {
        var result = new List<ServiceInfo>();
        var reports = recent.ToList();
        foreach (var service in Catalog.Services)
        {
            var mine = reports.Where(r => r.ServiceId == service.Id).ToList();
            if (mine.Count == 0)
            {
                result.Add(service);
                continue;
            }

            var level = mine.GroupBy(r => r.Level)
                .OrderByDescending(g => g.Count())
                .ThenByDescending(g => g.Max(r => r.CreatedAt))
                .First().Key;
            int minutes = Math.Max(1, (int)Math.Round((nowUtc - mine.Max(r => r.CreatedAt)).TotalMinutes));
            string who = mine.Count + (mine.Count > 1 ? " étudiants" : " étudiant");
            result.Add(service with { Crowd = level, Queue = "Signalé par " + who + " · il y a " + minutes + " min" });
        }
        return result;
    }
}
