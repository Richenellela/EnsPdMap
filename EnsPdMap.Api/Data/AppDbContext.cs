using EnsPdMap.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EnsPdMap.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<StudentProgress> Progress => Set<StudentProgress>();
    public DbSet<CrowdReport> CrowdReports => Set<CrowdReport>();
    public DbSet<CampusCalibration> Calibrations => Set<CampusCalibration>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Student>().HasIndex(s => s.Matricule).IsUnique();
        b.Entity<Student>().HasIndex(s => s.Email).IsUnique();
        b.Entity<StudentProgress>().HasIndex(p => new { p.StudentId, p.ProcedureId }).IsUnique();
        b.Entity<CrowdReport>().HasIndex(r => new { r.ServiceId, r.CreatedAt });
    }
}
