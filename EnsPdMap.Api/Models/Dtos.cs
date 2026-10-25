namespace EnsPdMap.Api.Models;

public record RegisterRequest(string FullName, string Matricule, string Email, string Filiere, string Niveau, string Password);

public record LoginRequest(string Identifier, string Password);

public record StudentDto(int Id, string FullName, string Matricule, string Email, string Filiere, string Niveau);

public record AuthResponse(string Token, DateTime ExpiresAt, StudentDto Student);

public record ProgressDto(string ProcedureId, List<int> ReadyPieces, List<int> DoneSteps, bool Bookmarked);

public record ProgressUpdate(List<int>? ReadyPieces, List<int>? DoneSteps, bool Bookmarked);

public record CrowdRequest(string Level);

public record SearchResult(List<ServiceInfo> Services, List<ProcedureInfo> Procedures);

public static class StudentMapper
{
    public static StudentDto ToDto(this Student s)
    {
        return new StudentDto(s.Id, s.FullName, s.Matricule, s.Email, s.Filiere, s.Niveau);
    }
}
