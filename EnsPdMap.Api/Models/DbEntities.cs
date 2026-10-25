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

/// <summary>Avancement d'un étudiant dans une démarche (pièces prêtes, étapes faites, favori).</summary>
public class StudentProgress
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string ProcedureId { get; set; } = "";
    public string ReadyPieces { get; set; } = "";
    public string DoneSteps { get; set; } = "";
    public bool Bookmarked { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Signalement d'affluence fait par un étudiant devant un guichet.</summary>
public class CrowdReport
{
    public int Id { get; set; }
    public string ServiceId { get; set; } = "";
    public int StudentId { get; set; }
    public string Level { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

/// <summary>Étalonnage partagé du plan : relie le plan schématique aux coordonnées GPS réelles.</summary>
public class CampusCalibration
{
    public int Id { get; set; }
    public double OriginLat { get; set; }
    public double OriginLon { get; set; }
    public double AnchorX { get; set; }
    public double AnchorY { get; set; }
    public double RotationDeg { get; set; }
    public double Scale { get; set; } = 1;
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
