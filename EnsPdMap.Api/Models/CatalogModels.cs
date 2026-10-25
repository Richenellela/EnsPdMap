namespace EnsPdMap.Api.Models;

// Forme du fichier catalogue.json (identique côté application mobile)

public record HourRow
{
    public string Day { get; init; } = "";
    public string Time { get; init; } = "";
    public List<int> Dow { get; init; } = new();
}

public record ServiceInfo
{
    public string Id { get; init; } = "";
    public string Code { get; init; } = "";
    public string Name { get; init; } = "";
    public string ShortName { get; init; } = "";
    public string SheetTitle { get; init; } = "";
    public string Description { get; init; } = "";
    public string Category { get; init; } = "";
    public string Block { get; init; } = "";
    public int Floor { get; init; }
    public string ListLoc { get; init; } = "";
    public string Place { get; init; } = "";
    public string Landmark { get; init; } = "";
    public string Crowd { get; init; } = "";
    public string Manager { get; init; } = "";
    public string Phone { get; init; } = "";
    public string Email { get; init; } = "";
    public string Queue { get; init; } = "";
    public List<HourRow> Hours { get; init; } = new();
}

public record PieceInfo
{
    public string Name { get; init; } = "";
    public string Detail { get; init; } = "";
    public string How { get; init; } = "";
}

public record StepInfo
{
    public string Title { get; init; } = "";
    public string Detail { get; init; } = "";
    public string ServiceId { get; init; } = "";
    public string Duration { get; init; } = "";
    public string Tip { get; init; } = "";
    public string Warning { get; init; } = "";
    public string ProcedureId { get; init; } = "";
}

public record FaqInfo
{
    public string Q { get; init; } = "";
    public string A { get; init; } = "";
}

public record ProcedureInfo
{
    public string Id { get; init; } = "";
    public string Category { get; init; } = "";
    public string Title { get; init; } = "";
    public string Overview { get; init; } = "";
    public string Audience { get; init; } = "";
    public string Where { get; init; } = "";
    public string Delay { get; init; } = "";
    public string Cost { get; init; } = "";
    public string Office { get; init; } = "";
    public string BadgeTop { get; init; } = "";
    public string BadgeMain { get; init; } = "";
    public string ServiceId { get; init; } = "";
    public string LocateLabel { get; init; } = "";
    public List<PieceInfo> Pieces { get; init; } = new();
    public List<StepInfo> Steps { get; init; } = new();
    public List<FaqInfo> Faq { get; init; } = new();
}

public record PlaceInfo
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Kind { get; init; } = "";
    public double X { get; init; }
    public double Y { get; init; }
    public double W { get; init; }
    public double H { get; init; }
    public double Angle { get; init; }
    public string ServiceId { get; init; } = "";
    public string EntranceNode { get; init; } = "";
}

public record NodeInfo
{
    public string Id { get; init; } = "";
    public double X { get; init; }
    public double Y { get; init; }
}

public record EdgeInfo
{
    public string A { get; init; } = "";
    public string B { get; init; } = "";
}

public record CampusInfo
{
    public string Name { get; init; } = "";
    public string Note { get; init; } = "";
    public string Entrance { get; init; } = "";
    public List<PlaceInfo> Places { get; init; } = new();
    public List<NodeInfo> Nodes { get; init; } = new();
    public List<EdgeInfo> Edges { get; init; } = new();
}

public record CatalogFile
{
    public int Version { get; init; }
    public List<ServiceInfo> Services { get; init; } = new();
    public List<ProcedureInfo> Procedures { get; init; } = new();
    public CampusInfo Campus { get; init; } = new();
    public CalibrationDto? Calibration { get; init; }
}

public record CalibrationDto
{
    public double OriginLat { get; init; }
    public double OriginLon { get; init; }
    public double AnchorX { get; init; }
    public double AnchorY { get; init; }
    public double RotationDeg { get; init; }
    public double Scale { get; init; } = 1;
}

public record CatalogResponse
{
    public int Version { get; init; }
    public List<ServiceInfo> Services { get; init; } = new();
    public List<ProcedureInfo> Procedures { get; init; } = new();
    public CampusInfo Campus { get; init; } = new();
    public CalibrationDto? Calibration { get; init; }
}
