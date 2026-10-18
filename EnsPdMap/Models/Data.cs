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
    public string ServiceId { get; set; } = "";
    public string FormSize { get; set; } = "";
    public string LocateLabel { get; set; } = "";
    public List<Piece> Pieces { get; set; } = new List<Piece>();
    public List<Step> Steps { get; set; } = new List<Step>();

    public string Summary
    {
        get { return Pieces.Count + " pièces · " + Delay; }
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
            Id = "cai", Code = "CAI", Name = "Caisse & Quitus", Short = "Caisse",
            SheetTitle = "Caisse & Quitus · Porte 012",
            Description = "Frais, reçus, quitus UBA", Category = "Caisse",
            Block = "Bloc administratif", Floor = 0,
            ListLoc = "Bloc administratif · Porte 012", Place = "Bloc administratif · RDC",
            Landmark = "Repère : à droite de l'entrée principale",
            Status = "Ouvert", Crowd = "Forte", ClosesAt = "14:00",
            Manager = "Responsable de la caisse",
            Phone = "+237 6 00 00 00 02", Email = "caisse@univ-douala.cm",
            Queue = "File : 14 personnes · environ 35 min", Distance = 90, Minutes = 2,
            Hours = new List<string[]>
            {
                new[] { "Lundi – Jeudi", "08:00 – 14:00" },
                new[] { "Vendredi", "08:00 – 12:00" },
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
            Delay = "48 h", Cost = "2 500 F", Office = "P. 104", DoorNumber = "104", ServiceId = "sco",
            FormSize = "184 Ko", LocateLabel = "Localiser la Scolarité",
            Pieces = new List<Piece>
            {
                new Piece { Name = "Photocopie de la CNI", Ready = true },
                new Piece { Name = "Quitus de paiement UBA", Detail = "Année académique en cours", Ready = true },
                new Piece { Name = "Deux timbres fiscaux", Detail = "1 000 FCFA chacun" },
                new Piece { Name = "Copie du dernier relevé de notes" }
            },
            Steps = new List<Step>
            {
                new Step { Title = "Télécharger et remplir le formulaire", Detail = "PDF compact · 184 Ko" },
                new Step { Title = "Valider le quitus", Detail = "Caisse · Bloc administratif · RDC" },
                new Step { Title = "Déposer le dossier", Detail = "Scolarité · Bloc A · Porte 104" },
                new Step { Title = "Retirer l'attestation sous 48 h", Detail = "Scolarité · Porte 104" }
            }
        },
        new Procedure
        {
            Id = "releve", Category = "Scolarité", Title = "Demande de relevé de notes",
            Delay = "72 h", Cost = "1 000 F", Office = "P. 104", DoorNumber = "104", ServiceId = "sco",
            FormSize = "156 Ko", LocateLabel = "Localiser la Scolarité",
            Pieces = new List<Piece>
            {
                new Piece { Name = "Photocopie de la CNI" },
                new Piece { Name = "Quitus de paiement UBA", Detail = "Année académique en cours" },
                new Piece { Name = "Demande manuscrite" }
            },
            Steps = new List<Step>
            {
                new Step { Title = "Télécharger et remplir le formulaire", Detail = "PDF compact · 156 Ko" },
                new Step { Title = "Déposer le dossier", Detail = "Scolarité · Bloc A · Porte 104" },
                new Step { Title = "Retirer le relevé sous 72 h", Detail = "Scolarité · Porte 104" }
            }
        },
        new Procedure
        {
            Id = "quitus", Category = "Caisse", Title = "Obtention du quitus UBA",
            Delay = "24 h", Cost = "Gratuit", Office = "P. 012", DoorNumber = "012", ServiceId = "cai",
            FormSize = "92 Ko", LocateLabel = "Localiser la Caisse",
            Pieces = new List<Piece>
            {
                new Piece { Name = "Reçu de paiement des frais" },
                new Piece { Name = "Photocopie de la CNI" }
            },
            Steps = new List<Step>
            {
                new Step { Title = "Présenter le reçu à la Caisse", Detail = "Caisse · Bloc administratif · Porte 012" },
                new Step { Title = "Retirer le quitus sous 24 h", Detail = "Caisse · Porte 012" }
            }
        }
    };

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
