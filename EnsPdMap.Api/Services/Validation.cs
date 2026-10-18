using System.Text.RegularExpressions;
using EnsPdMap.Api.Models;

namespace EnsPdMap.Api.Services;

public static class Validation
{
    static readonly Regex MatriculeRegex = new Regex(@"^\d{2}[A-Za-z]\d{4,5}$", RegexOptions.Compiled);
    static readonly string[] Filieres = { "GI", "GE", "GM", "GC", "GP" };
    static readonly string[] Niveaux = { "N1", "N2", "N3", "N4", "N5" };
    public const string MailDomain = "@univ-douala.cm";

    public static bool IsMatricule(string? s)
    {
        return !string.IsNullOrWhiteSpace(s) && MatriculeRegex.IsMatch(s.Trim());
    }

    public static bool IsInstitutionalMail(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        string mail = s.Trim().ToLowerInvariant();
        return mail.EndsWith(MailDomain) && mail.Length > MailDomain.Length;
    }

    public static Dictionary<string, string[]> Register(RegisterRequest r)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(r.FullName) || r.FullName.Trim().Length < 3)
            errors["fullName"] = new[] { "Le nom complet est obligatoire (3 caractères minimum)." };
        if (!IsMatricule(r.Matricule))
            errors["matricule"] = new[] { "Matricule invalide (format attendu : 24G01622)." };
        if (!IsInstitutionalMail(r.Email))
            errors["email"] = new[] { "Utilisez une adresse " + MailDomain + "." };
        if (!Filieres.Contains((r.Filiere ?? "").Trim().ToUpperInvariant()))
            errors["filiere"] = new[] { "Filière invalide (GI, GE, GM, GC ou GP)." };
        if (!Niveaux.Contains((r.Niveau ?? "").Trim().ToUpperInvariant()))
            errors["niveau"] = new[] { "Niveau invalide (N1 à N5)." };
        if (string.IsNullOrEmpty(r.Password) || r.Password.Length < 8)
            errors["password"] = new[] { "Le mot de passe doit contenir au moins 8 caractères." };
        return errors;
    }
}
