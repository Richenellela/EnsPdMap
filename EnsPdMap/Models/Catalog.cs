using System.Text.Json;
using System.Text.Json.Serialization;
using EnsPdMap.Helpers;
using Microsoft.Maui.Storage;

namespace EnsPdMap.Models;

// Ces classes ont la même forme que catalogue.json (partagé avec l'API).

public class Piece
{
    public string Name { get; set; } = "";
    public string Detail { get; set; } = "";
    public string How { get; set; } = "";
    [JsonIgnore] public bool Ready { get; set; }
}

public class Step
{
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
    public string ServiceId { get; set; } = "";
    public string Duration { get; set; } = "";
    public string Tip { get; set; } = "";
    public string Warning { get; set; } = "";
    public string ProcedureId { get; set; } = "";
    [JsonIgnore] public bool Done { get; set; }
}

public class Faq
{
    public string Q { get; set; } = "";
    public string A { get; set; } = "";
}

public class Procedure
{
    public string Id { get; set; } = "";
    public string Category { get; set; } = "";
    public string Title { get; set; } = "";
    public string Overview { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Where { get; set; } = "";
    public string Delay { get; set; } = "";
    public string Cost { get; set; } = "";
    public string Office { get; set; } = "";
    public string BadgeTop { get; set; } = "";
    public string BadgeMain { get; set; } = "";
    public string ServiceId { get; set; } = "";
    public string LocateLabel { get; set; } = "";
    public List<Piece> Pieces { get; set; } = new List<Piece>();
    public List<Step> Steps { get; set; } = new List<Step>();
    public List<Faq> Faq { get; set; } = new List<Faq>();

    [JsonIgnore]
    public string FicheFile
    {
        get { return "fiche-" + Id + ".pdf"; }
    }

    [JsonIgnore]
    public string Summary
    {
        get
        {
            string pieces = Pieces.Count + (Pieces.Count > 1 ? " pièces" : " pièce");
            return string.IsNullOrEmpty(Delay) ? pieces : pieces + " · " + Delay;
        }
    }

    [JsonIgnore]
    public bool AllPiecesReady
    {
        get { return Pieces.Count > 0 && Pieces.All(p => p.Ready); }
    }
}

public class HourRow
{
    public string Day { get; set; } = "";
    public string Time { get; set; } = "";
    public List<int> Dow { get; set; } = new List<int>();
}

public class Service
{
    public string Id { get; set; } = "";
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string SheetTitle { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public string Block { get; set; } = "";
    public int Floor { get; set; }
    public string ListLoc { get; set; } = "";
    public string Place { get; set; } = "";
    public string Landmark { get; set; } = "";
    public string Crowd { get; set; } = "Faible";
    public string Manager { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Queue { get; set; } = "";
    public List<HourRow> Hours { get; set; } = new List<HourRow>();

    [JsonIgnore] public string Short { get { return ShortName; } }

    [JsonIgnore]
    public string FloorName
    {
        get
        {
            if (Floor == 0) return "RDC";
            if (Floor == 1) return "1er étage";
            return Floor + "e étage";
        }
    }

    /// <summary>Ouvert / Fermé calculé d'après les horaires et l'heure du téléphone.</summary>
    [JsonIgnore]
    public string Status
    {
        get { return OpeningHours.IsOpen(this, DateTime.Now) ? "Ouvert" : "Fermé"; }
    }

    [JsonIgnore]
    public string ClosesAt
    {
        get { return OpeningHours.ClosesAt(this, DateTime.Now); }
    }

    [JsonIgnore]
    public string NextOpening
    {
        get { return OpeningHours.NextOpening(this, DateTime.Now, false); }
    }

    [JsonIgnore]
    public string NextOpeningShort
    {
        get { return OpeningHours.NextOpening(this, DateTime.Now, true); }
    }
}

public class CampusPlace
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public double W { get; set; }
    public double H { get; set; }
    public double Angle { get; set; }
    public string ServiceId { get; set; } = "";
    public string EntranceNode { get; set; } = "";
}

public class CampusNode
{
    public string Id { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
}

public class CampusEdge
{
    public string A { get; set; } = "";
    public string B { get; set; } = "";
}

public class Campus
{
    public string Name { get; set; } = "";
    public string Note { get; set; } = "";
    public string Entrance { get; set; } = "";
    public List<CampusPlace> Places { get; set; } = new List<CampusPlace>();
    public List<CampusNode> Nodes { get; set; } = new List<CampusNode>();
    public List<CampusEdge> Edges { get; set; } = new List<CampusEdge>();

    public CampusNode Node(string id)
    {
        foreach (var n in Nodes)
            if (n.Id == id) return n;
        return null;
    }

    public CampusPlace PlaceForService(string serviceId)
    {
        foreach (var p in Places)
            if (p.ServiceId == serviceId && !string.IsNullOrEmpty(serviceId)) return p;
        return null;
    }
}

/// <summary>Relie le plan schématique (mètres) aux coordonnées GPS : schéma = Ancre + Échelle × Rotation × (position GPS − origine).</summary>
public class Calibration
{
    public double OriginLat { get; set; }
    public double OriginLon { get; set; }
    public double AnchorX { get; set; }
    public double AnchorY { get; set; }
    public double RotationDeg { get; set; }
    public double Scale { get; set; } = 1;

    [JsonIgnore]
    public bool Calibrated
    {
        get { return OriginLat != 0 || OriginLon != 0; }
    }
}

public class CatalogFile
{
    public int Version { get; set; }
    public List<Service> Services { get; set; } = new List<Service>();
    public List<Procedure> Procedures { get; set; } = new List<Procedure>();
    public Campus Campus { get; set; } = new Campus();
    public Calibration Calibration { get; set; }
}

public static class Data
{
    public static readonly List<Service> Services = new List<Service>();
    public static readonly List<Procedure> Procedures = new List<Procedure>();
    public static Campus Campus = new Campus();
    public static Calibration SharedCalibration;
    public static Calibration DefaultCalibration;   // fixée une fois pour toutes dans catalogue.json
    public static int Version;
    public static int Stamp;   // augmente à chaque remplacement du catalogue (les écrans se reconstruisent)
    public static string LoadError = "";

    static readonly JsonSerializerOptions Json = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    static string CachePath
    {
        get { return System.IO.Path.Combine(FileSystem.AppDataDirectory, "catalogue-cache.json"); }
    }

    /// <summary>Charge le catalogue : version intégrée à l'appli, ou dernière version reçue de l'API si plus récente.</summary>
    public static void Init()
    {
        try
        {
            CatalogFile embedded = ReadEmbedded();
            CatalogFile cached = ReadCache();
            DefaultCalibration = embedded != null ? embedded.Calibration : null;
            CatalogFile best = embedded;
            if (best == null || (cached != null && cached.Version > best.Version)) best = cached;

            if (best == null)
            {
                LoadError = "Catalogue introuvable.";
                return;
            }
            Apply(best, false);
        }
        catch (Exception ex)
        {
            LoadError = ex.Message;
            CrashLog.Write(ex, "Catalogue");
        }
    }

    static CatalogFile ReadEmbedded()
    {
        using (var stream = typeof(Data).Assembly.GetManifestResourceStream("catalogue.json"))
        {
            if (stream == null) return null;
            return JsonSerializer.Deserialize<CatalogFile>(stream, Json);
        }
    }

    static CatalogFile ReadCache()
    {
        try
        {
            if (!File.Exists(CachePath)) return null;
            return JsonSerializer.Deserialize<CatalogFile>(File.ReadAllText(CachePath), Json);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static void Apply(CatalogFile file, bool saveToCache)
    {
        Services.Clear();
        Services.AddRange(file.Services);
        Procedures.Clear();
        Procedures.AddRange(file.Procedures);
        Campus = file.Campus ?? new Campus();
        SharedCalibration = file.Calibration ?? DefaultCalibration;
        Version = file.Version;
        Stamp++;

        foreach (var p in Procedures)
            ProgressStore.ApplyTo(p);

        if (saveToCache)
        {
            try
            {
                File.WriteAllText(CachePath, JsonSerializer.Serialize(file, Json));
            }
            catch (Exception)
            {
                // le cache est facultatif
            }
        }
    }

    public static Service GetService(string id)
    {
        foreach (var s in Services)
            if (s.Id == id) return s;
        return Services.Count > 0 ? Services[0] : new Service();
    }

    public static Procedure GetProcedure(string id)
    {
        foreach (var p in Procedures)
            if (p.Id == id) return p;
        return Procedures.Count > 0 ? Procedures[0] : new Procedure();
    }
}
