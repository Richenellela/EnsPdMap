namespace EnsPdMap.Api.Models;

public static class Mappers
{
    public static StudentDto ToDto(this Student s)
    {
        return new StudentDto(s.Id, s.FullName, s.Matricule, s.Email, s.Filiere, s.Niveau);
    }

    static string FloorName(int floor)
    {
        if (floor == 0) return "RDC";
        if (floor == 1) return "1er étage";
        return floor + "e étage";
    }

    public static ServiceDto ToDto(this ServiceEntity s)
    {
        var hours = s.Hours.OrderBy(h => h.SortOrder).Select(h => new HourDto(h.Day, h.Time)).ToList();
        return new ServiceDto(
            s.Id, s.Code, s.Name, s.ShortName, s.SheetTitle, s.Description,
            s.Category, s.Block, s.Floor, FloorName(s.Floor), s.ListLoc, s.Place,
            s.Landmark, s.Status, s.Crowd, s.ClosesAt, s.Manager, s.Phone,
            s.Email, s.Queue, s.Distance, s.Minutes, hours);
    }

    public static ProcedureDto ToDto(this ProcedureEntity p)
    {
        var pieces = p.Pieces.OrderBy(x => x.SortOrder).Select(x => new PieceDto(x.Name, x.Detail)).ToList();
        var steps = p.Steps.OrderBy(x => x.SortOrder).Select(x => new StepDto(x.Title, x.Detail)).ToList();
        return new ProcedureDto(
            p.Id, p.Category, p.Title, p.Delay, p.Cost, p.Office,
            p.DoorNumber, p.BadgeTop, p.BadgeMain, p.ServiceId, p.FormSize, p.FicheFile, p.LocateLabel, pieces, steps);
    }
}
