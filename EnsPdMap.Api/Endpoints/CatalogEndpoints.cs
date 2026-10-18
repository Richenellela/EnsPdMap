using EnsPdMap.Api.Data;
using EnsPdMap.Api.Models;
using EnsPdMap.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace EnsPdMap.Api.Endpoints;

public static class CatalogEndpoints
{
    static bool MatchService(ServiceEntity s, string q)
    {
        string hay = TextUtil.Norm(s.Name + " " + s.ShortName + " " + s.Description + " " + s.ListLoc + " " + s.Category);
        return hay.Contains(q);
    }

    static bool MatchProcedure(ProcedureEntity p, string q)
    {
        string hay = TextUtil.Norm(p.Title + " " + p.Category + " " + string.Join(" ", p.Pieces.Select(x => x.Name)));
        return hay.Contains(q);
    }

    public static void MapCatalogEndpoints(this WebApplication app)
    {
        app.MapGet("/api/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));

        // Services (publics : le mode visiteur n'a pas besoin de compte)
        app.MapGet("/api/services", async (AppDbContext db, string? category, string? q) =>
        {
            var items = await db.Services.AsNoTracking().Include(s => s.Hours).OrderBy(s => s.SortOrder).ToListAsync();

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
            return Results.Ok(items.Select(s => s.ToDto()).ToList());
        });

        app.MapGet("/api/services/{id}", async (string id, AppDbContext db) =>
        {
            var service = await db.Services.AsNoTracking().Include(s => s.Hours).FirstOrDefaultAsync(s => s.Id == id);
            return service == null ? Results.NotFound() : Results.Ok(service.ToDto());
        });

        // Démarches
        app.MapGet("/api/procedures", async (AppDbContext db) =>
        {
            var items = await db.Procedures.AsNoTracking().Include(p => p.Pieces).Include(p => p.Steps)
                .OrderBy(p => p.SortOrder).ToListAsync();
            return Results.Ok(items.Select(p => p.ToDto()).ToList());
        });

        app.MapGet("/api/procedures/{id}", async (string id, AppDbContext db) =>
        {
            var procedure = await db.Procedures.AsNoTracking().Include(p => p.Pieces).Include(p => p.Steps)
                .FirstOrDefaultAsync(p => p.Id == id);
            return procedure == null ? Results.NotFound() : Results.Ok(procedure.ToDto());
        });

        // Recherche globale : un bureau, une démarche, une pièce
        app.MapGet("/api/search", async (AppDbContext db, string? q) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.Ok(new SearchResult(new List<ServiceDto>(), new List<ProcedureDto>()));

            string query = TextUtil.Norm(q.Trim());
            var services = await db.Services.AsNoTracking().Include(s => s.Hours).OrderBy(s => s.SortOrder).ToListAsync();
            var procedures = await db.Procedures.AsNoTracking().Include(p => p.Pieces).Include(p => p.Steps)
                .OrderBy(p => p.SortOrder).ToListAsync();

            var result = new SearchResult(
                services.Where(s => MatchService(s, query)).Select(s => s.ToDto()).ToList(),
                procedures.Where(p => MatchProcedure(p, query)).Select(p => p.ToDto()).ToList());
            return Results.Ok(result);
        });
    }
}
