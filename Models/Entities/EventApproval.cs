using System;
using System.ComponentModel.DataAnnotations;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Models.Entities;

public class EventApproval : AuditableEntity
{
    public Guid EventId { get; set; }

    public Event Event { get; set; } = null!;

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public Guid ReviewerId { get; set; }

    public ApplicationUser Reviewer { get; set; } = null!;

    [StringLength(500)]
    public string? Comment { get; set; }
}

