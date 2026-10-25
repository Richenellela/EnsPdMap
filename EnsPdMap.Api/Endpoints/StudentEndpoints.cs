using System.Security.Claims;
using EnsPdMap.Api.Data;
using EnsPdMap.Api.Models;
using EnsPdMap.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace EnsPdMap.Api.Endpoints;

/// <summary>Fonctions liées au compte : avancement des démarches, affluence signalée, étalonnage du plan.</summary>
public static class StudentEndpoints
{
    static int? UserId(ClaimsPrincipal user)
    {
        string? sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
        return int.TryParse(sub, out int id) ? id : null;
    }

    static string Join(IEnumerable<int> values)
    {
        return string.Join(",", values.Distinct().OrderBy(v => v));
    }

    static List<int> Split(string csv)
    {
        var list = new List<int>();
        foreach (var part in csv.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (int.TryParse(part, out int v)) list.Add(v);
        }
        return list;
    }

    public static void MapStudentEndpoints(this WebApplication app)
    {
        // ---- Avancement des démarches ----
        app.MapGet("/api/me/progress", async (ClaimsPrincipal user, AppDbContext db) =>
        {
            int? id = UserId(user);
            if (id == null) return Results.Unauthorized();

            var rows = await db.Progress.AsNoTracking().Where(p => p.StudentId == id.Value).ToListAsync();
            var dtos = rows.Select(p => new ProgressDto(p.ProcedureId, Split(p.ReadyPieces), Split(p.DoneSteps), p.Bookmarked)).ToList();
            return Results.Ok(dtos);
        }).RequireAuthorization();

        app.MapPut("/api/me/progress/{procedureId}", async (string procedureId, ProgressUpdate update, ClaimsPrincipal user, AppDbContext db, CatalogService catalog) =>
        {
            int? id = UserId(user);
            if (id == null) return Results.Unauthorized();

            var procedure = catalog.Catalog.Procedures.FirstOrDefault(p => p.Id == procedureId);
            if (procedure == null) return Results.NotFound(new { message = "Démarche inconnue." });

            // on ne garde que des indices qui existent vraiment
            var ready = (update.ReadyPieces ?? new List<int>()).Where(i => i >= 0 && i < procedure.Pieces.Count);
            var done = (update.DoneSteps ?? new List<int>()).Where(i => i >= 0 && i < procedure.Steps.Count);

            var row = await db.Progress.FirstOrDefaultAsync(p => p.StudentId == id.Value && p.ProcedureId == procedureId);
            if (row == null)
            {
                row = new StudentProgress { StudentId = id.Value, ProcedureId = procedureId };
                db.Progress.Add(row);
            }
            row.ReadyPieces = Join(ready);
            row.DoneSteps = Join(done);
            row.Bookmarked = update.Bookmarked;
            row.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return Results.Ok(new ProgressDto(procedureId, Split(row.ReadyPieces), Split(row.DoneSteps), row.Bookmarked));
        }).RequireAuthorization();

        // ---- Affluence signalée par les étudiants ----
        app.MapPost("/api/services/{id}/crowd", async (string id, CrowdRequest request, ClaimsPrincipal user, AppDbContext db, CatalogService catalog) =>
        {
            int? studentId = UserId(user);
            if (studentId == null) return Results.Unauthorized();
            if (catalog.Catalog.Services.All(s => s.Id != id)) return Results.NotFound(new { message = "Service inconnu." });
            if (request == null || !CatalogEndpoints.CrowdLevels.Contains(request.Level))
                return Results.BadRequest(new { message = "Niveau d'affluence invalide (Faible, Modérée ou Forte)." });

            var previous = await db.CrowdReports.Where(r => r.StudentId == studentId.Value && r.ServiceId == id).ToListAsync();
            db.CrowdReports.RemoveRange(previous);
            db.CrowdReports.Add(new CrowdReport { ServiceId = id, StudentId = studentId.Value, Level = request.Level, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();

            DateTime now = DateTime.UtcNow;
            var recent = await db.CrowdReports.AsNoTracking().Where(r => r.CreatedAt >= now.AddMinutes(-45)).ToListAsync();
            var service = catalog.ServicesWithCrowd(recent, now).First(s => s.Id == id);
            return Results.Ok(service);
        }).RequireAuthorization();

        // ---- Étalonnage du plan ----
        app.MapPost("/api/campus/calibration", async (CalibrationDto dto, ClaimsPrincipal user, AppDbContext db) =>
        {
            int? studentId = UserId(user);
            if (studentId == null) return Results.Unauthorized();

            bool valid = dto.OriginLat is >= -90 and <= 90 && dto.OriginLon is >= -180 and <= 180
                         && dto.Scale is >= 0.5 and <= 2.0 && !(dto.OriginLat == 0 && dto.OriginLon == 0);
            if (!valid) return Results.BadRequest(new { message = "Étalonnage invalide." });

            db.Calibrations.Add(new CampusCalibration
            {
                OriginLat = dto.OriginLat, OriginLon = dto.OriginLon, AnchorX = dto.AnchorX, AnchorY = dto.AnchorY,
                RotationDeg = dto.RotationDeg, Scale = dto.Scale, UpdatedBy = studentId.Value, UpdatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
            return Results.Ok(dto);
        }).RequireAuthorization();
    }
}
