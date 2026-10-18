using EnsPdMap.Api.Models;

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
        if (!db.Services.Any())
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
                    Id = "cai", SortOrder = 2, Code = "CAI", Name = "Caisse & Quitus", ShortName = "Caisse",
                    SheetTitle = "Caisse & Quitus · Porte 012", Description = "Frais, reçus, quitus UBA",
                    Category = "Caisse", Block = "Bloc administratif", Floor = 0,
                    ListLoc = "Bloc administratif · Porte 012", Place = "Bloc administratif · RDC",
                    Landmark = "Repère : à droite de l'entrée principale",
                    Status = "Ouvert", Crowd = "Forte", ClosesAt = "14:00",
                    Manager = "Responsable de la caisse",
                    Phone = "+237 6 00 00 00 02", Email = "caisse@univ-douala.cm",
                    Queue = "File : 14 personnes · environ 35 min", Distance = 90, Minutes = 2,
                    Hours = Hours("Lundi – Jeudi", "08:00 – 14:00", "Vendredi", "08:00 – 12:00", "Samedi – Dimanche", "Fermé")
                },
                new ServiceEntity
                {
                    Id = "med", SortOrder = 3, Code = "MÉD", Name = "Centre médical CMSU", ShortName = "CMSU",
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

        if (!db.Procedures.Any())
        {
            db.Procedures.AddRange(
                new ProcedureEntity
                {
                    Id = "attestation", SortOrder = 0, Category = "Scolarité", Title = "Retrait d'attestation de scolarité",
                    Delay = "48 h", Cost = "2 500 F", Office = "P. 104", DoorNumber = "104", ServiceId = "sco",
                    FormSize = "184 Ko", LocateLabel = "Localiser la Scolarité",
                    Pieces = new List<PieceEntity>
                    {
                        new PieceEntity { SortOrder = 0, Name = "Photocopie de la CNI" },
                        new PieceEntity { SortOrder = 1, Name = "Quitus de paiement UBA", Detail = "Année académique en cours" },
                        new PieceEntity { SortOrder = 2, Name = "Deux timbres fiscaux", Detail = "1 000 FCFA chacun" },
                        new PieceEntity { SortOrder = 3, Name = "Copie du dernier relevé de notes" }
                    },
                    Steps = new List<StepEntity>
                    {
                        new StepEntity { SortOrder = 0, Title = "Télécharger et remplir le formulaire", Detail = "PDF compact · 184 Ko" },
                        new StepEntity { SortOrder = 1, Title = "Valider le quitus", Detail = "Caisse · Bloc administratif · RDC" },
                        new StepEntity { SortOrder = 2, Title = "Déposer le dossier", Detail = "Scolarité · Bloc A · Porte 104" },
                        new StepEntity { SortOrder = 3, Title = "Retirer l'attestation sous 48 h", Detail = "Scolarité · Porte 104" }
                    }
                },
                new ProcedureEntity
                {
                    Id = "releve", SortOrder = 1, Category = "Scolarité", Title = "Demande de relevé de notes",
                    Delay = "72 h", Cost = "1 000 F", Office = "P. 104", DoorNumber = "104", ServiceId = "sco",
                    FormSize = "156 Ko", LocateLabel = "Localiser la Scolarité",
                    Pieces = new List<PieceEntity>
                    {
                        new PieceEntity { SortOrder = 0, Name = "Photocopie de la CNI" },
                        new PieceEntity { SortOrder = 1, Name = "Quitus de paiement UBA", Detail = "Année académique en cours" },
                        new PieceEntity { SortOrder = 2, Name = "Demande manuscrite" }
                    },
                    Steps = new List<StepEntity>
                    {
                        new StepEntity { SortOrder = 0, Title = "Télécharger et remplir le formulaire", Detail = "PDF compact · 156 Ko" },
                        new StepEntity { SortOrder = 1, Title = "Déposer le dossier", Detail = "Scolarité · Bloc A · Porte 104" },
                        new StepEntity { SortOrder = 2, Title = "Retirer le relevé sous 72 h", Detail = "Scolarité · Porte 104" }
                    }
                },
                new ProcedureEntity
                {
                    Id = "quitus", SortOrder = 2, Category = "Caisse", Title = "Obtention du quitus UBA",
                    Delay = "24 h", Cost = "Gratuit", Office = "P. 012", DoorNumber = "012", ServiceId = "cai",
                    FormSize = "92 Ko", LocateLabel = "Localiser la Caisse",
                    Pieces = new List<PieceEntity>
                    {
                        new PieceEntity { SortOrder = 0, Name = "Reçu de paiement des frais" },
                        new PieceEntity { SortOrder = 1, Name = "Photocopie de la CNI" }
                    },
                    Steps = new List<StepEntity>
                    {
                        new StepEntity { SortOrder = 0, Title = "Présenter le reçu à la Caisse", Detail = "Caisse · Bloc administratif · Porte 012" },
                        new StepEntity { SortOrder = 1, Title = "Retirer le quitus sous 24 h", Detail = "Caisse · Porte 012" }
                    }
                });
        }

        db.SaveChanges();
    }
}
