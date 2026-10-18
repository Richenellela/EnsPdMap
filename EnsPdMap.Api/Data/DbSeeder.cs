using EnsPdMap.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EnsPdMap.Api.Data;

/// <summary>Données de démarrage identiques à celles de l'application mobile (à remplacer par les vraies).</summary>
public static class DbSeeder
{
    static List<HourEntity> Hours(params string[] pairs)
    {
        var list = new List<HourEntity>();
        for (int i = 0; i + 1 < pairs.Length; i += 2)
            list.Add(new HourEntity { SortOrder = i / 2, Day = pairs[i], Time = pairs[i + 1] });
        return list;
    }

    public static void Seed(AppDbContext db)
    {
        // Le catalogue (services, démarches) est géré dans ce fichier : on le recrée à chaque démarrage.
        db.Pieces.ExecuteDelete();
        db.Steps.ExecuteDelete();
        db.Procedures.ExecuteDelete();
        db.Hours.ExecuteDelete();
        db.Services.ExecuteDelete();

        {
            db.Services.AddRange(
                new ServiceEntity
                {
                    Id = "sco", SortOrder = 0, Code = "SCO", Name = "Scolarité ENSPD", ShortName = "Scolarité",
                    SheetTitle = "Scolarité · Porte 104", Description = "Attestations, relevés, inscriptions",
                    Category = "Scolarité", Block = "Bloc A · Direction / Scolarité", Floor = 1,
                    ListLoc = "Bloc A · Porte 104", Place = "Bloc A · 1er étage · Aile Nord",
                    Landmark = "Repère : palier de l'escalier central du Bloc A",
                    Status = "Ouvert", Crowd = "Modérée", ClosesAt = "15:30",
                    Manager = "Mme Carole Essomba, Chef de service",
                    Phone = "+237 6 99 12 34 56", Email = "scolarite@univ-douala.cm",
                    Queue = "File : 6 personnes · environ 12 min", Distance = 120, Minutes = 2,
                    Hours = Hours("Lundi – Jeudi", "08:00 – 15:30", "Vendredi", "08:00 – 13:00", "Samedi – Dimanche", "Fermé")
                },
                new ServiceEntity
                {
                    Id = "dgi", SortOrder = 1, Code = "DÉP", Name = "Département Génie Informatique", ShortName = "Génie Informatique",
                    SheetTitle = "Génie Informatique · Bloc C", Description = "Stages, notes, responsables de filière",
                    Category = "Départements", Block = "Bloc C · Génie Informatique", Floor = 1,
                    ListLoc = "Bloc C · 1er étage", Place = "Bloc C · 1er étage",
                    Landmark = "Repère : couloir principal du Bloc C",
                    Status = "Ouvert", Crowd = "Faible", ClosesAt = "16:00",
                    Manager = "Chef de département GI",
                    Phone = "+237 6 00 00 00 01", Email = "genie.informatique@univ-douala.cm",
                    Queue = "File : 1 personne · environ 2 min", Distance = 210, Minutes = 3,
                    Hours = Hours("Lundi – Jeudi", "08:00 – 16:00", "Vendredi", "08:00 – 13:00", "Samedi – Dimanche", "Fermé")
                },
                new ServiceEntity
                {
                    Id = "med", SortOrder = 2, Code = "MÉD", Name = "Centre médical CMSU", ShortName = "CMSU",
                    SheetTitle = "Centre médical CMSU", Description = "Consultations et certificats médicaux",
                    Category = "Médical", Block = "Bâtiment santé · Entrée Est", Floor = 0,
                    ListLoc = "Entrée Est · Bâtiment santé", Place = "Bâtiment santé · Entrée Est",
                    Landmark = "Repère : après l'entrée Est du campus",
                    Status = "Fermé", Crowd = "Faible", ClosesAt = "",
                    Manager = "Médecin-chef du CMSU",
                    Phone = "+237 6 00 00 00 03", Email = "cmsu@univ-douala.cm",
                    Queue = "Fermé actuellement", Distance = 300, Minutes = 4,
                    Hours = Hours("Lundi – Jeudi", "08:00 – 12:00", "Vendredi", "08:00 – 12:00", "Samedi – Dimanche", "Fermé")
                });
        }

        {
            db.Procedures.AddRange(
                new ProcedureEntity
                {
                    Id = "attestation", SortOrder = 0, Category = "Scolarité", Title = "Retrait d'attestation de scolarité",
                    Delay = "Variable", Cost = "Gratuit", Office = "P. 104", DoorNumber = "104",
                    BadgeTop = "Porte", BadgeMain = "104", ServiceId = "sco",
                    FormSize = "3 Ko", FicheFile = "fiche-attestation.pdf", LocateLabel = "Localiser la Scolarité",
                    Pieces = new List<PieceEntity>
                    {
                        new PieceEntity { SortOrder = 0, Name = "Quitus de l'année en cours", Detail = "Année académique en cours" }
                    },
                    Steps = new List<StepEntity>
                    {
                        new StepEntity { SortOrder = 0, Title = "Rassembler ses quitus", Detail = "Quitus de l'année en cours" },
                        new StepEntity { SortOrder = 1, Title = "Déposer la demande", Detail = "Scolarité · Bloc A · Porte 104" },
                        new StepEntity { SortOrder = 2, Title = "Retirer l'attestation", Detail = "Scolarité · Porte 104 · le lendemain ou dans la semaine, selon la disponibilité du directeur" }
                    }
                },
                new ProcedureEntity
                {
                    Id = "releve", SortOrder = 1, Category = "Scolarité", Title = "Demande de relevé de notes",
                    Delay = "Variable", Cost = "Gratuit", Office = "P. 104", DoorNumber = "104",
                    BadgeTop = "Porte", BadgeMain = "104", ServiceId = "sco",
                    FormSize = "4 Ko", FicheFile = "fiche-releve.pdf", LocateLabel = "Localiser la Scolarité",
                    Pieces = new List<PieceEntity>
                    {
                        new PieceEntity { SortOrder = 0, Name = "Quitus de l'année précédente", Detail = "Année académique antérieure" },
                        new PieceEntity { SortOrder = 1, Name = "Reçus de paiement", Detail = "À faire cacheter au bureau de la Scolarité" },
                        new PieceEntity { SortOrder = 2, Name = "Photocopies certifiées", Detail = "À faire certifier à la Scolarité" }
                    },
                    Steps = new List<StepEntity>
                    {
                        new StepEntity { SortOrder = 0, Title = "Rassembler quitus et reçus", Detail = "Quitus de l'année précédente" },
                        new StepEntity { SortOrder = 1, Title = "Faire cacheter les reçus", Detail = "Scolarité · Bloc A · Porte 104" },
                        new StepEntity { SortOrder = 2, Title = "Photocopier et faire certifier", Detail = "Certification par la Scolarité · Porte 104" },
                        new StepEntity { SortOrder = 3, Title = "Déposer le dossier", Detail = "Scolarité · Porte 104" },
                        new StepEntity { SortOrder = 4, Title = "Retirer le relevé de notes", Detail = "Scolarité · Porte 104 · le lendemain ou dans la semaine, selon la disponibilité du directeur" }
                    }
                },
                new ProcedureEntity
                {
                    Id = "quitus", SortOrder = 2, Category = "Scolarité", Title = "Obtention des quitus",
                    Delay = "", Cost = "Gratuit", Office = "En ligne", DoorNumber = "",
                    BadgeTop = "En", BadgeMain = "ligne", ServiceId = "",
                    FormSize = "3 Ko", FicheFile = "fiche-quitus.pdf", LocateLabel = "Télécharger la fiche",
                    Pieces = new List<PieceEntity>
                    {
                        new PieceEntity { SortOrder = 0, Name = "Quitus médical", Detail = "2 exemplaires" },
                        new PieceEntity { SortOrder = 1, Name = "Quitus tranche 1", Detail = "2 exemplaires" },
                        new PieceEntity { SortOrder = 2, Name = "Quitus tranche 2", Detail = "2 exemplaires" }
                    },
                    Steps = new List<StepEntity>
                    {
                        new StepEntity { SortOrder = 0, Title = "Se connecter à son espace", Detail = "Site de l'université · compte étudiant" },
                        new StepEntity { SortOrder = 1, Title = "Télécharger les 3 quitus", Detail = "Médical · Tranche 1 · Tranche 2" },
                        new StepEntity { SortOrder = 2, Title = "Imprimer 2 exemplaires de chaque", Detail = "2 exemplaires de chaque quitus" },
                        new StepEntity { SortOrder = 3, Title = "Payer à la banque UBA", Detail = "Seule banque pour les frais de scolarité · hors application" },
                        new StepEntity { SortOrder = 4, Title = "Récupérer les reçus de paiement", Detail = "À conserver pour les autres démarches" }
                    }
                },
                new ProcedureEntity
                {
                    Id = "stage", SortOrder = 3, Category = "Scolarité", Title = "Dépôt de la lettre de mise en stage",
                    Delay = "Variable", Cost = "Gratuit", Office = "Étage 1", DoorNumber = "",
                    BadgeTop = "1er", BadgeMain = "étage", ServiceId = "sco",
                    FormSize = "3 Ko", FicheFile = "fiche-stage.pdf", LocateLabel = "Localiser la Scolarité",
                    Pieces = new List<PieceEntity>
                    {
                        new PieceEntity { SortOrder = 0, Name = "Quitus de l'année en cours", Detail = "Année académique en cours" },
                        new PieceEntity { SortOrder = 1, Name = "Lettre de mise en stage", Detail = "À déposer" }
                    },
                    Steps = new List<StepEntity>
                    {
                        new StepEntity { SortOrder = 0, Title = "Rassembler ses quitus", Detail = "Quitus de l'année en cours" },
                        new StepEntity { SortOrder = 1, Title = "Déposer la lettre de mise en stage", Detail = "Scolarité · 1er étage · repérer la salle grâce à l'intitulé sur la porte" }
                    }
                });
        }

        db.SaveChanges();
    }
}
