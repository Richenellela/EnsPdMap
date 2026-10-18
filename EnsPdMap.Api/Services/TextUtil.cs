using System.Globalization;
using System.Text;

namespace EnsPdMap.Api.Services;

public static class TextUtil
{
    /// <summary>Minuscules sans accents, pour des recherches tolérantes.</summary>
    public static string Norm(string? s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        string decomposed = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().ToLowerInvariant();
    }
}
