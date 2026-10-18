using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EnsPdMap.Models;

public class Piece
{
    public string Name { get; set; } = "";
    public string Detail { get; set; } = "";
    public bool Ready { get; set; }
}

public class Step
{
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
}

public class Procedure
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
    public string FicheFile { get; set; } = "";
    public string ServiceId { get; set; } = "";
    public string FormSize { get; set; } = "";
    public string LocateLabel { get; set; } = "";
    public List<Piece> Pieces { get; set; } = new List<Piece>();
    public List<Step> Steps { get; set; } = new List<Step>();

    public string Summary
    {
        get
        {
            string pieces = Pieces.Count + (Pieces.Count > 1 ? " pièces" : " pièce");
            return string.IsNullOrEmpty(Delay) ? pieces : pieces + " · " + Delay;
        }
    }
}

public class Service
{
    public string Id { get; set; } = "";
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Short { get; set; } = "";
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
    public List<string[]> Hours { get; set; } = new List<string[]>();

    public string FloorName
    {
        get
        {
            if (Floor == 0) return "RDC";
            if (Floor == 1) return "1er étage";
            return Floor + "e étage";
        }
    }
}

public static class Data
{
    public static readonly List<Service> Services = new List<Service>
    {
        new Service
        {
            Id = "sco", Code = "SCO", Name = "Scolarité ENSPD", Short = "Scolarité",
            SheetTitle = "Scolarité · Porte 104",
            Description = "Attestations, relevés, inscriptions", Category = "Scolarité",
            Block = "Bloc A · Direction / Scolarité", Floor = 1,
            ListLoc = "Bloc A · Porte 104", Place = "Bloc A · 1er étage · Aile Nord",
            Landmark = "Repère : palier de l'escalier central du Bloc A",
            Status = "Ouvert", Crowd = "Modérée", ClosesAt = "15:30",
            Manager = "Mme Carole Essomba, Chef de service",
            Phone = "+237 6 99 12 34 56", Email = "scolarite@univ-douala.cm",
            Queue = "File : 6 personnes · environ 12 min", Distance = 120, Minutes = 2,
            Hours = new List<string[]>
            {
                new[] { "Lundi – Jeudi", "08:00 – 15:30" },
                new[] { "Vendredi", "08:00 – 13:00" },
                new[] { "Samedi – Dimanche", "Fermé" }
            }
        },
        new Service
        {
            Id = "dgi", Code = "DÉP", Name = "Département Génie Informatique", Short = "Génie Informatique",
            SheetTitle = "Génie Informatique · Bloc C",
            Description = "Stages, notes, responsables de filière", Category = "Départements",
            Block = "Bloc C · Génie Informatique", Floor = 1,
            ListLoc = "Bloc C · 1er étage", Place = "Bloc C · 1er étage",
            Landmark = "Repère : couloir principal du Bloc C",
            Status = "Ouvert", Crowd = "Faible", ClosesAt = "16:00",
            Manager = "Chef de département GI",
            Phone = "+237 6 00 00 00 01", Email = "genie.informatique@univ-douala.cm",
            Queue = "File : 1 personne · environ 2 min", Distance = 210, Minutes = 3,
            Hours = new List<string[]>
            {
                new[] { "Lundi – Jeudi", "08:00 – 16:00" },
                new[] { "Vendredi", "08:00 – 13:00" },
                new[] { "Samedi – Dimanche", "Fermé" }
            }
        },
        new Service
        {
            Id = "med", Code = "MÉD", Name = "Centre médical CMSU", Short = "CMSU",
            SheetTitle = "Centre médical CMSU",
            Description = "Consultations et certificats médicaux", Category = "Médical",
            Block = "Bâtiment santé · Entrée Est", Floor = 0,
            ListLoc = "Entrée Est · Bâtiment santé", Place = "Bâtiment santé · Entrée Est",
            Landmark = "Repère : après l'entrée Est du campus",
            Status = "Fermé", Crowd = "Faible", ClosesAt = "",
            Manager = "Médecin-chef du CMSU",
            Phone = "+237 6 00 00 00 03", Email = "cmsu@univ-douala.cm",
            Queue = "Fermé actuellement", Distance = 300, Minutes = 4,
            Hours = new List<string[]>
            {
                new[] { "Lundi – Jeudi", "08:00 – 12:00" },
                new[] { "Vendredi", "08:00 – 12:00" },
                new[] { "Samedi – Dimanche", "Fermé" }
            }
        }
    };

    public static readonly List<Procedure> Procedures = new List<Procedure>
    {
        new Procedure
        {
            Id = "attestation", Category = "Scolarité", Title = "Retrait d'attestation de scolarité",
            Delay = "Variable", Cost = "Gratuit", Office = "P. 104", DoorNumber = "104",
            BadgeTop = "Porte", BadgeMain = "104", ServiceId = "sco",
            FormSize = "3 Ko", FicheFile = "fiche-attestation.pdf", LocateLabel = "Localiser la Scolarité",
            Pieces = new List<Piece>
            {
                new Piece { Name = "Quitus de l'année en cours", Detail = "Année académique en cours" }
            },
            Steps = new List<Step>
            {
                new Step { Title = "Rassembler ses quitus", Detail = "Quitus de l'année en cours" },
                new Step { Title = "Déposer la demande", Detail = "Scolarité · Bloc A · Porte 104" },
                new Step { Title = "Retirer l'attestation", Detail = "Scolarité · Porte 104 · le lendemain ou dans la semaine, selon la disponibilité du directeur" }
            }
        },
        new Procedure
        {
            Id = "releve", Category = "Scolarité", Title = "Demande de relevé de notes",
            Delay = "Variable", Cost = "Gratuit", Office = "P. 104", DoorNumber = "104",
            BadgeTop = "Porte", BadgeMain = "104", ServiceId = "sco",
            FormSize = "4 Ko", FicheFile = "fiche-releve.pdf", LocateLabel = "Localiser la Scolarité",
            Pieces = new List<Piece>
            {
                new Piece { Name = "Quitus de l'année précédente", Detail = "Année académique antérieure" },
                new Piece { Name = "Reçus de paiement", Detail = "À faire cacheter au bureau de la Scolarité" },
                new Piece { Name = "Photocopies certifiées", Detail = "À faire certifier à la Scolarité" }
            },
            Steps = new List<Step>
            {
                new Step { Title = "Rassembler quitus et reçus", Detail = "Quitus de l'année précédente" },
                new Step { Title = "Faire cacheter les reçus", Detail = "Scolarité · Bloc A · Porte 104" },
                new Step { Title = "Photocopier et faire certifier", Detail = "Certification par la Scolarité · Porte 104" },
                new Step { Title = "Déposer le dossier", Detail = "Scolarité · Porte 104" },
                new Step { Title = "Retirer le relevé de notes", Detail = "Scolarité · Porte 104 · le lendemain ou dans la semaine, selon la disponibilité du directeur" }
            }
        },
        new Procedure
        {
            Id = "quitus", Category = "Scolarité", Title = "Obtention des quitus",
            Delay = "", Cost = "Gratuit", Office = "En ligne", DoorNumber = "",
            BadgeTop = "En", BadgeMain = "ligne", ServiceId = "",
            FormSize = "3 Ko", FicheFile = "fiche-quitus.pdf", LocateLabel = "Télécharger la fiche",
            Pieces = new List<Piece>
            {
                new Piece { Name = "Quitus médical", Detail = "2 exemplaires" },
                new Piece { Name = "Quitus tranche 1", Detail = "2 exemplaires" },
                new Piece { Name = "Quitus tranche 2", Detail = "2 exemplaires" }
            },
            Steps = new List<Step>
            {
                new Step { Title = "Se connecter à son espace", Detail = "Site de l'université · compte étudiant" },
                new Step { Title = "Télécharger les 3 quitus", Detail = "Médical · Tranche 1 · Tranche 2" },
                new Step { Title = "Imprimer 2 exemplaires de chaque", Detail = "2 exemplaires de chaque quitus" },
                new Step { Title = "Payer à la banque UBA", Detail = "Seule banque pour les frais de scolarité · hors application" },
                new Step { Title = "Récupérer les reçus de paiement", Detail = "À conserver pour les autres démarches" }
            }
        },
        new Procedure
        {
            Id = "stage", Category = "Scolarité", Title = "Dépôt de la lettre de mise en stage",
            Delay = "Variable", Cost = "Gratuit", Office = "Étage 1", DoorNumber = "",
            BadgeTop = "1er", BadgeMain = "étage", ServiceId = "sco",
            FormSize = "3 Ko", FicheFile = "fiche-stage.pdf", LocateLabel = "Localiser la Scolarité",
            Pieces = new List<Piece>
            {
                new Piece { Name = "Quitus de l'année en cours", Detail = "Année académique en cours" },
                new Piece { Name = "Lettre de mise en stage", Detail = "À déposer" }
            },
            Steps = new List<Step>
            {
                new Step { Title = "Rassembler ses quitus", Detail = "Quitus de l'année en cours" },
                new Step { Title = "Déposer la lettre de mise en stage", Detail = "Scolarité · 1er étage · repérer la salle grâce à l'intitulé sur la porte" }
            }
        }
    };

    /// <summary>Remplace le catalogue local par celui reçu de l'API.</summary>
    public static void Replace(List<Service> services, List<Procedure> procedures)
    {
        Services.Clear();
        Services.AddRange(services);
        Procedures.Clear();
        Procedures.AddRange(procedures);
    }

    public static Service GetService(string id)
    {
        foreach (var s in Services)
            if (s.Id == id) return s;
        return Services[0];
    }

    public static Procedure GetProcedure(string id)
    {
        foreach (var p in Procedures)
            if (p.Id == id) return p;
        return Procedures[0];
    }
}

public static class AppState
{
    public static string FirstName = "Richenelle";
    public static string Initials = "MR";
    public static string Filiere = "GI";
    public static string Niveau = "N3";
    public static bool IsVisitor = false;

    // Passage d'informations entre écrans
    public static string PendingQuery = "";
    public static string PlanTarget = "";

    public static bool UltraEco = true;

    public static string Norm(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var d = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in d)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().ToLowerInvariant();
    }

    public static bool IsValidMatricule(string s)
    {
        return !string.IsNullOrWhiteSpace(s) && Regex.IsMatch(s.Trim(), @"^\d{2}[A-Za-z]\d{4,5}$");
    }

    public static bool IsInstitutionalMail(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        s = s.Trim().ToLowerInvariant();
        return s.EndsWith("@univ-douala.cm") && s.Length > "@univ-douala.cm".Length;
    }

    public static void SignInStudent()
    {
        IsVisitor = false;
    }

    public static void SignInVisitor()
    {
        IsVisitor = true;
        FirstName = "visiteur";
        Initials = "V";
    }

    public static void Register(string fullName, string filiere, string niveau)
    {
        IsVisitor = false;
        Filiere = filiere;
        Niveau = niveau;
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0)
        {
            FirstName = parts[parts.Length - 1];
            var first = char.ToUpperInvariant(parts[0][0]).ToString();
            var last = char.ToUpperInvariant(parts[parts.Length - 1][0]).ToString();
            Initials = parts.Length > 1 ? first + last : first;
        }
    }
}
