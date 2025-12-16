using System;
using System.ComponentModel.DataAnnotations;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Models.Entities;

public class ClubApplication : AuditableEntity
{
    [Required]
    [StringLength(150)]
    public string ClubName { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string Mission { get; set; } = null!;

    [StringLength(500)]
    public string? Vision { get; set; }

    [StringLength(1000)]
    public string? PlannedActivities { get; set; }

    [Range(5, 500)]
    public int EstimatedMemberCount { get; set; }

    [StringLength(200)]
    public string? AdvisorName { get; set; }

    [EmailAddress]
    [StringLength(200)]
    public string? AdvisorEmail { get; set; }

    [StringLength(1000)]
    public string? ResourceNeeds { get; set; }

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public Guid ApplicantUserId { get; set; }

    public ApplicationUser ApplicantUser { get; set; } = null!;

    public Guid? ReviewedByUserId { get; set; }

    public ApplicationUser? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    [StringLength(750)]
    public string? CoordinatorNote { get; set; }
}
