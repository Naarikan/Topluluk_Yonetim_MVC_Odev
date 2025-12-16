using System;

namespace Topluluk_Yonetim.MVC.Models.Entities;

public class AnnouncementRead : AuditableEntity
{
    public Guid AnnouncementId { get; set; }

    public Announcement Announcement { get; set; } = null!;

    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
}

