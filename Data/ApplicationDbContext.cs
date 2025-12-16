using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<ClubApplication> ClubApplications => Set<ClubApplication>();
    public DbSet<ClubMembership> ClubMemberships => Set<ClubMembership>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventApproval> EventApprovals => Set<EventApproval>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<AnnouncementRead> AnnouncementReads => Set<AnnouncementRead>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(DateTime))
                    {
                        var dateTimeValue = (DateTime?)property.CurrentValue;
                        if (dateTimeValue.HasValue && dateTimeValue.Value.Kind != DateTimeKind.Utc)
                        {
                            property.CurrentValue = dateTimeValue.Value.Kind == DateTimeKind.Local
                                ? dateTimeValue.Value.ToUniversalTime()
                                : DateTime.SpecifyKind(dateTimeValue.Value, DateTimeKind.Utc);
                        }
                    }
                    else if (property.Metadata.ClrType == typeof(DateTime?))
                    {
                        var dateTimeValue = (DateTime?)property.CurrentValue;
                        if (dateTimeValue.HasValue && dateTimeValue.Value.Kind != DateTimeKind.Utc)
                        {
                            property.CurrentValue = dateTimeValue.Value.Kind == DateTimeKind.Local
                                ? dateTimeValue.Value.ToUniversalTime()
                                : DateTime.SpecifyKind(dateTimeValue.Value, DateTimeKind.Utc);
                        }
                    }
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Club>()
            .HasOne(c => c.President)
            .WithMany(u => u.ManagedClubs)
            .HasForeignKey(c => c.PresidentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ClubMembership>()
            .HasOne(cm => cm.Club)
            .WithMany(c => c.Memberships)
            .HasForeignKey(cm => cm.ClubId);

        builder.Entity<ClubMembership>()
            .HasOne(cm => cm.User)
            .WithMany(u => u.ClubMemberships)
            .HasForeignKey(cm => cm.UserId);

        builder.Entity<ClubMembership>()
            .HasIndex(cm => cm.UserId)
            .IsUnique()
            .HasFilter("\"Role\" IN (1, 2, 3)");

        builder.Entity<ClubApplication>()
            .HasOne(ca => ca.ApplicantUser)
            .WithMany()
            .HasForeignKey(ca => ca.ApplicantUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ClubApplication>()
            .HasOne(ca => ca.ReviewedBy)
            .WithMany()
            .HasForeignKey(ca => ca.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Event>()
            .HasOne(e => e.Club)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.ClubId);

        builder.Entity<EventApproval>()
            .HasOne(ea => ea.Event)
            .WithMany(e => e.ApprovalHistory)
            .HasForeignKey(ea => ea.EventId);

        builder.Entity<EventApproval>()
            .HasOne(ea => ea.Reviewer)
            .WithMany()
            .HasForeignKey(ea => ea.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Announcement>()
            .HasOne(a => a.Club)
            .WithMany(c => c.Announcements)
            .HasForeignKey(a => a.ClubId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<AnnouncementRead>()
            .HasOne(ar => ar.Announcement)
            .WithMany(a => a.ReadBy)
            .HasForeignKey(ar => ar.AnnouncementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AnnouncementRead>()
            .HasOne(ar => ar.User)
            .WithMany(u => u.ReadAnnouncements)
            .HasForeignKey(ar => ar.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AnnouncementRead>()
            .HasIndex(ar => new { ar.AnnouncementId, ar.UserId })
            .IsUnique();
    }
}

