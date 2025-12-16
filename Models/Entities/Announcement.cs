using System;
using System.ComponentModel.DataAnnotations;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Models.Entities;

public class Announcement : AuditableEntity
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = null!;

    public AnnouncementAudience Audience { get; set; } = AnnouncementAudience.AllStudents;

    public Guid? ClubId { get; set; }

    public Club? Club { get; set; }

    public bool IsPinned { get; set; }

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public ICollection<AnnouncementRead> ReadBy { get; set; } = new List<AnnouncementRead>();
}

