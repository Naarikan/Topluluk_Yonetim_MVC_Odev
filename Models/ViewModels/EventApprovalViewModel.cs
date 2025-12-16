using System;
using System.Collections.Generic;
using System.Linq;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Models.ViewModels;

public class EventApprovalItemViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public decimal EstimatedBudget { get; set; }
    public Guid ClubId { get; set; }
    public string ClubName { get; set; } = "Bilinmiyor";
    public string? CreatedByName { get; set; }
    public string? CreatedByEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public ApprovalStatus Status { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? Comment { get; set; }
}

public class EventApprovalDashboardViewModel
{
    public IEnumerable<EventApprovalItemViewModel> PendingEvents { get; set; }
        = Enumerable.Empty<EventApprovalItemViewModel>();

    public IEnumerable<EventApprovalItemViewModel> ApprovedEvents { get; set; }
        = Enumerable.Empty<EventApprovalItemViewModel>();

    public IEnumerable<EventApprovalItemViewModel> RejectedEvents { get; set; }
        = Enumerable.Empty<EventApprovalItemViewModel>();

    public PagedList<EventApprovalItemViewModel>? PagedEvents { get; set; }

    public EventApprovalFilterViewModel Filter { get; set; } = new();

    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
}

