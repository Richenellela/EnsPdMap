namespace EnsPdMap.Api.Models;

public record RegisterRequest(string FullName, string Matricule, string Email, string Filiere, string Niveau, string Password);

public record LoginRequest(string Identifier, string Password);

public record StudentDto(int Id, string FullName, string Matricule, string Email, string Filiere, string Niveau);

public record AuthResponse(string Token, DateTime ExpiresAt, StudentDto Student);

public record HourDto(string Day, string Time);

public record ServiceDto(
    string Id, string Code, string Name, string ShortName, string SheetTitle, string Description,
    string Category, string Block, int Floor, string FloorName, string ListLoc, string Place,
    string Landmark, string Status, string Crowd, string ClosesAt, string Manager, string Phone,
    string Email, string Queue, int Distance, int Minutes, List<HourDto> Hours);

public record PieceDto(string Name, string Detail);

public record StepDto(string Title, string Detail);

public record ProcedureDto(
    string Id, string Category, string Title, string Delay, string Cost, string Office,
    string DoorNumber, string ServiceId, string FormSize, string LocateLabel,
    List<PieceDto> Pieces, List<StepDto> Steps);

public record SearchResult(List<ServiceDto> Services, List<ProcedureDto> Procedures);
