using System;
using System.ComponentModel.DataAnnotations;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Models.Entities;

public class ClubMembership : AuditableEntity
{
    public Guid ClubId { get; set; }

    public Club Club { get; set; } = null!;

    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public MembershipStatus Status { get; set; } = MembershipStatus.Pending;

    public ClubRole Role { get; set; } = ClubRole.Member;

    public Guid? ReviewedByUserId { get; set; }

    public ApplicationUser? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }
}

