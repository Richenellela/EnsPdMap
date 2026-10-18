using EnsPdMap.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EnsPdMap.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<ServiceEntity> Services => Set<ServiceEntity>();
    public DbSet<HourEntity> Hours => Set<HourEntity>();
    public DbSet<ProcedureEntity> Procedures => Set<ProcedureEntity>();
    public DbSet<PieceEntity> Pieces => Set<PieceEntity>();
    public DbSet<StepEntity> Steps => Set<StepEntity>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Student>().HasIndex(s => s.Matricule).IsUnique();
        b.Entity<Student>().HasIndex(s => s.Email).IsUnique();

        b.Entity<ServiceEntity>().HasKey(s => s.Id);
        b.Entity<ServiceEntity>().HasMany(s => s.Hours).WithOne().HasForeignKey(h => h.ServiceId);

        b.Entity<ProcedureEntity>().HasKey(p => p.Id);
        b.Entity<ProcedureEntity>().HasMany(p => p.Pieces).WithOne().HasForeignKey(x => x.ProcedureId);
        b.Entity<ProcedureEntity>().HasMany(p => p.Steps).WithOne().HasForeignKey(x => x.ProcedureId);
    }
}
