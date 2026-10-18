namespace EnsPdMap.Models;

// Objets reçus de l'API (les noms JSON sont lus sans tenir compte de la casse)

public class ApiStudent
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Matricule { get; set; } = "";
    public string Email { get; set; } = "";
    public string Filiere { get; set; } = "";
    public string Niveau { get; set; } = "";
}

public class ApiAuthResponse
{
    public string Token { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public ApiStudent Student { get; set; }
}

public class ApiHour
{
    public string Day { get; set; } = "";
    public string Time { get; set; } = "";
}

public class ApiService
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
    public string Status { get; set; } = "";
    public string Crowd { get; set; } = "";
    public string ClosesAt { get; set; } = "";
    public string Manager { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Queue { get; set; } = "";
    public int Distance { get; set; }
    public int Minutes { get; set; }
    public List<ApiHour> Hours { get; set; } = new List<ApiHour>();
}

public class ApiPiece
{
    public string Name { get; set; } = "";
    public string Detail { get; set; } = "";
}

public class ApiStep
{
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
}

public class ApiProcedure
{
    public string Id { get; set; } = "";
    public string Category { get; set; } = "";
    public string Title { get; set; } = "";
    public string Delay { get; set; } = "";
    public string Cost { get; set; } = "";
    public string Office { get; set; } = "";
    public string DoorNumber { get; set; } = "";
    public string BadgeTop { get; set; } = "";
    public string BadgeMain { get; set; } = "";
    public string ServiceId { get; set; } = "";
    public string FormSize { get; set; } = "";
    public string FicheFile { get; set; } = "";
    public string LocateLabel { get; set; } = "";
    public List<ApiPiece> Pieces { get; set; } = new List<ApiPiece>();
    public List<ApiStep> Steps { get; set; } = new List<ApiStep>();
}
