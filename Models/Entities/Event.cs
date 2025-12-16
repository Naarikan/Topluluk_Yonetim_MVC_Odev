using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Models.Entities;

public class Event : AuditableEntity
{
    public Guid ClubId { get; set; }

    public Club Club { get; set; } = null!;

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime EventDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal EstimatedBudget { get; set; }

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public ICollection<EventApproval> ApprovalHistory { get; set; } = new List<EventApproval>();
}

