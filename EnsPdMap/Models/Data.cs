using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EnsPdMap.Models;

public static class AppState
{
    public static string FirstName = "Richenelle";
    public static string Initials = "MR";
    public static string Filiere = "GI";
    public static string Niveau = "N3";
    public static bool IsVisitor = false;

    // Passage d'informations entre écrans
    public static string PendingQuery = "";
    public static string PlanTarget = "";

    public static bool UltraEco = true;

    public static string Norm(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var d = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in d)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().ToLowerInvariant();
    }

    public static bool IsValidMatricule(string s)
    {
        return !string.IsNullOrWhiteSpace(s) && Regex.IsMatch(s.Trim(), @"^\d{2}[A-Za-z]\d{4,5}$");
    }

    public static bool IsInstitutionalMail(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        s = s.Trim().ToLowerInvariant();
        return s.EndsWith("@univ-douala.cm") && s.Length > "@univ-douala.cm".Length;
    }

    public static void SignInStudent()
    {
        IsVisitor = false;
    }

    public static void SignInVisitor()
    {
        IsVisitor = true;
        FirstName = "visiteur";
        Initials = "V";
    }

    public static void Register(string fullName, string filiere, string niveau)
    {
        IsVisitor = false;
        Filiere = filiere;
        Niveau = niveau;
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0)
        {
            FirstName = parts[parts.Length - 1];
            var first = char.ToUpperInvariant(parts[0][0]).ToString();
            var last = char.ToUpperInvariant(parts[parts.Length - 1][0]).ToString();
            Initials = parts.Length > 1 ? first + last : first;
        }
    }
}
