using EnsPdMap.Api.Data;
using EnsPdMap.Api.Models;
using EnsPdMap.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace EnsPdMap.Api.Endpoints;

public static class CatalogEndpoints
{
    public static readonly string[] CrowdLevels = { "Faible", "Modérée", "Forte" };

    static async Task<List<ServiceInfo>> LoadServices(AppDbContext db, CatalogService catalog)
    {
        DateTime now = DateTime.UtcNow;
        DateTime cutoff = now.AddMinutes(-45);
        var recent = await db.CrowdReports.AsNoTracking().Where(r => r.CreatedAt >= cutoff).ToListAsync();
        return catalog.ServicesWithCrowd(recent, now);
    }

    static async Task<CalibrationDto?> LoadCalibration(AppDbContext db)
    {
        var c = await db.Calibrations.AsNoTracking().OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync();
        if (c == null) return null;
        return new CalibrationDto
        {
            OriginLat = c.OriginLat, OriginLon = c.OriginLon, AnchorX = c.AnchorX,
            AnchorY = c.AnchorY, RotationDeg = c.RotationDeg, Scale = c.Scale
        };
    }

    static bool MatchService(ServiceInfo s, string q)
    {
        return TextUtil.Norm(s.Name + " " + s.ShortName + " " + s.Description + " " + s.ListLoc + " " + s.Category).Contains(q);
    }

    static bool MatchProcedure(ProcedureInfo p, string q)
    {
        string pieces = string.Join(" ", p.Pieces.Select(x => x.Name));
        string steps = string.Join(" ", p.Steps.Select(x => x.Title + " " + x.Detail));
        return TextUtil.Norm(p.Title + " " + p.Category + " " + p.Overview + " " + pieces + " " + steps).Contains(q);
    }

    public static void MapCatalogEndpoints(this WebApplication app)
    {
        app.MapGet("/api/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));

        // Tout le catalogue en une requête (utilisé par l'application pour se synchroniser)
        app.MapGet("/api/catalogue", async (AppDbContext db, CatalogService catalog) =>
        {
            var response = new CatalogResponse
            {
                Version = catalog.Catalog.Version,
                Services = await LoadServices(db, catalog),
                Procedures = catalog.Catalog.Procedures,
                Campus = catalog.Catalog.Campus,
                Calibration = await LoadCalibration(db) ?? catalog.Catalog.Calibration
            };
            return Results.Ok(response);
        });

        app.MapGet("/api/services", async (AppDbContext db, CatalogService catalog, string? category, string? q) =>
        {
            var items = await LoadServices(db, catalog);
            if (!string.IsNullOrWhiteSpace(category) && TextUtil.Norm(category) != "tous")
            {
                string c = TextUtil.Norm(category);
                items = items.Where(s => TextUtil.Norm(s.Category) == c).ToList();
            }
            if (!string.IsNullOrWhiteSpace(q))
            {
                string query = TextUtil.Norm(q.Trim());
                items = items.Where(s => MatchService(s, query)).ToList();
            }
            return Results.Ok(items);
        });

        app.MapGet("/api/services/{id}", async (string id, AppDbContext db, CatalogService catalog) =>
        {
            var service = (await LoadServices(db, catalog)).FirstOrDefault(s => s.Id == id);
            return service == null ? Results.NotFound() : Results.Ok(service);
        });

        app.MapGet("/api/procedures", (CatalogService catalog) => Results.Ok(catalog.Catalog.Procedures));

        app.MapGet("/api/procedures/{id}", (string id, CatalogService catalog) =>
        {
            var procedure = catalog.Catalog.Procedures.FirstOrDefault(p => p.Id == id);
            return procedure == null ? Results.NotFound() : Results.Ok(procedure);
        });

        app.MapGet("/api/campus", async (AppDbContext db, CatalogService catalog) =>
        {
            return Results.Ok(new { campus = catalog.Catalog.Campus, calibration = await LoadCalibration(db) ?? catalog.Catalog.Calibration });
        });

        app.MapGet("/api/search", async (AppDbContext db, CatalogService catalog, string? q) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.Ok(new SearchResult(new List<ServiceInfo>(), new List<ProcedureInfo>()));

            string query = TextUtil.Norm(q.Trim());
            var services = (await LoadServices(db, catalog)).Where(s => MatchService(s, query)).ToList();
            var procedures = catalog.Catalog.Procedures.Where(p => MatchProcedure(p, query)).ToList();
            return Results.Ok(new SearchResult(services, procedures));
        });
    }
}
