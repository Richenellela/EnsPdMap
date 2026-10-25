namespace EnsPdMap.Models;

/// <summary>Calcule si un service est ouvert maintenant, d'après ses horaires par jour de la semaine.</summary>
public static class OpeningHours
{
    static readonly string[] DayNames = { "dimanche", "lundi", "mardi", "mercredi", "jeudi", "vendredi", "samedi" };
    static readonly string[] DayShort = { "dim.", "lun.", "mar.", "mer.", "jeu.", "ven.", "sam." };

    static bool TryToday(Service s, DateTime now, out TimeSpan open, out TimeSpan close)
    {
        return TryDay(s, (int)now.DayOfWeek, out open, out close);
    }

    static bool TryDay(Service s, int dow, out TimeSpan open, out TimeSpan close)
    {
        open = TimeSpan.Zero;
        close = TimeSpan.Zero;

        foreach (var row in s.Hours)
        {
            if (!row.Dow.Contains(dow)) continue;
            string[] parts = row.Time.Split('–', '-');
            if (parts.Length != 2) return false;
            return TimeSpan.TryParse(parts[0].Trim(), out open) && TimeSpan.TryParse(parts[1].Trim(), out close);
        }
        return false;
    }

    public static bool IsOpen(Service s, DateTime now)
    {
        TimeSpan open, close;
        if (!TryToday(s, now, out open, out close)) return false;
        return now.TimeOfDay >= open && now.TimeOfDay < close;
    }

    public static string ClosesAt(Service s, DateTime now)
    {
        TimeSpan open, close;
        if (!TryToday(s, now, out open, out close)) return "";
        return close.ToString(@"hh\:mm");
    }

    /// <summary>Message quand le service est fermé : « Ouvre demain à 08:00 » (ou version courte pour les listes).</summary>
    public static string NextOpening(Service s, DateTime now, bool shortForm)
    {
        for (int offset = 0; offset <= 7; offset++)
        {
            DateTime day = now.Date.AddDays(offset);
            TimeSpan open, close;
            if (!TryDay(s, (int)day.DayOfWeek, out open, out close)) continue;
            if (offset == 0 && now.TimeOfDay >= open) continue;

            string time = open.ToString(@"hh\:mm");
            int dow = (int)day.DayOfWeek;
            if (shortForm)
            {
                if (offset == 0) return "Ouvre à " + time;
                return "Ouvre " + (offset == 1 ? "demain" : DayShort[dow]) + " " + time;
            }
            string when = offset == 0 ? "aujourd'hui" : (offset == 1 ? "demain" : DayNames[dow]);
            return "Ouvre " + when + " à " + time;
        }
        return "";
    }
}
