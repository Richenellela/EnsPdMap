namespace EnsPdMap.Api.Models;

public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Matricule { get; set; } = "";
    public string Email { get; set; } = "";
    public string Filiere { get; set; } = "";
    public string Niveau { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string PasswordSalt { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class ServiceEntity
{
    public string Id { get; set; } = "";
    public int SortOrder { get; set; }
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
    public string Status { get; set; } = "Ouvert";
    public string Crowd { get; set; } = "Faible";
    public string ClosesAt { get; set; } = "";
    public string Manager { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Queue { get; set; } = "";
    public int Distance { get; set; }
    public int Minutes { get; set; }
    public List<HourEntity> Hours { get; set; } = new();
}

public class HourEntity
{
    public int Id { get; set; }
    public string ServiceId { get; set; } = "";
    public int SortOrder { get; set; }
    public string Day { get; set; } = "";
    public string Time { get; set; } = "";
}

public class ProcedureEntity
{
    public string Id { get; set; } = "";
    public int SortOrder { get; set; }
    public string Category { get; set; } = "";
    public string Title { get; set; } = "";
    public string Delay { get; set; } = "";
    public string Cost { get; set; } = "";
    public string Office { get; set; } = "";
    public string DoorNumber { get; set; } = "";
    public string ServiceId { get; set; } = "";
    public string FormSize { get; set; } = "";
    public string LocateLabel { get; set; } = "";
    public List<PieceEntity> Pieces { get; set; } = new();
    public List<StepEntity> Steps { get; set; } = new();
}

public class PieceEntity
{
    public int Id { get; set; }
    public string ProcedureId { get; set; } = "";
    public int SortOrder { get; set; }
    public string Name { get; set; } = "";
    public string Detail { get; set; } = "";
}

public class StepEntity
{
    public int Id { get; set; }
    public string ProcedureId { get; set; } = "";
    public int SortOrder { get; set; }
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
}
