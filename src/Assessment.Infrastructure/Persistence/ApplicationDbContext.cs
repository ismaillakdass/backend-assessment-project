using System.Reflection;
using Assessment.Domain.Entities;
using Assessment.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Assessment.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Donor> Donors => Set<Donor>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<Volunteer> Volunteers => Set<Volunteer>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventVolunteer> EventVolunteers => Set<EventVolunteer>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
