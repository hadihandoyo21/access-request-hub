using AccessRequestHub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AccessRequestHub.Api.Data;

public class AppDbContext : DbContext
{
    public DbSet<AccessRequest> AccessRequests => Set<AccessRequest>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AppUser> Users => Set<AppUser>(); 

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AccessRequest>()
    .HasIndex(r => new { r.Requester, r.ClientRequestId })
    .IsUnique();

        modelBuilder.Entity<AuditLog>()
            .HasOne<AccessRequest>()
            .WithMany()
            .HasForeignKey(a => a.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AppUser>().HasData(
            new AppUser { Id = 1, Email = "alice@example.local", Role = "Requester", ManagedApp = null },
            new AppUser { Id = 2, Email = "bob@example.local", Role = "Manager", ManagedApp = null },
            new AppUser { Id = 3, Email = "carol@example.local", Role = "SystemOwner", ManagedApp = "CRM" },
            new AppUser { Id = 4, Email = "dana@example.local", Role = "SystemOwner", ManagedApp = "Finance Portal" },
            new AppUser { Id = 5, Email = "erin@example.local", Role = "Admin/Auditor", ManagedApp = null }
        );
    }
}