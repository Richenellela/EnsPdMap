using System.Security.Claims;
using EnsPdMap.Api.Data;
using EnsPdMap.Api.Models;
using EnsPdMap.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace EnsPdMap.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/api/auth");

        auth.MapPost("/register", async (RegisterRequest request, AppDbContext db, TokenService tokens) =>
        {
            var errors = Validation.Register(request);
            if (errors.Count > 0) return Results.ValidationProblem(errors);

            string matricule = request.Matricule.Trim().ToUpperInvariant();
            string email = request.Email.Trim().ToLowerInvariant();

            bool exists = await db.Students.AnyAsync(s => s.Matricule == matricule || s.Email == email);
            if (exists)
                return Results.Conflict(new { message = "Un compte existe déjà avec ce matricule ou cet e-mail." });

            var (hash, salt) = PasswordHasher.Hash(request.Password);
            var student = new Student
            {
                FullName = request.FullName.Trim(),
                Matricule = matricule,
                Email = email,
                Filiere = request.Filiere.Trim().ToUpperInvariant(),
                Niveau = request.Niveau.Trim().ToUpperInvariant(),
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            };
            db.Students.Add(student);
            await db.SaveChangesAsync();

            var (token, expires) = tokens.Create(student);
            return Results.Created("/api/me", new AuthResponse(token, expires, student.ToDto()));
        });

        auth.MapPost("/login", async (LoginRequest request, AppDbContext db, TokenService tokens) =>
        {
            if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrEmpty(request.Password))
                return Results.BadRequest(new { message = "Identifiant et mot de passe obligatoires." });

            string id = request.Identifier.Trim();
            string matricule = id.ToUpperInvariant();
            string email = id.ToLowerInvariant();

            var student = await db.Students.FirstOrDefaultAsync(s => s.Matricule == matricule || s.Email == email);
            if (student == null || !PasswordHasher.Verify(request.Password, student.PasswordHash, student.PasswordSalt))
                return Results.Json(new { message = "Identifiant ou mot de passe incorrect." }, statusCode: StatusCodes.Status401Unauthorized);

            var (token, expires) = tokens.Create(student);
            return Results.Ok(new AuthResponse(token, expires, student.ToDto()));
        });

        app.MapGet("/api/me", async (ClaimsPrincipal user, AppDbContext db) =>
        {
            string? sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
            if (!int.TryParse(sub, out int studentId)) return Results.Unauthorized();

            var student = await db.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null) return Results.NotFound();
            return Results.Ok(student.ToDto());
        }).RequireAuthorization();
    }
}
