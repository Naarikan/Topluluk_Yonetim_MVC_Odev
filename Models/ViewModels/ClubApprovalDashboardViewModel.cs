using System;
using System.Collections.Generic;
using System.Linq;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Models.ViewModels;

public class ClubApplicationApprovalItemViewModel
{
    public Guid Id { get; set; }
    public string ClubName { get; set; } = null!;
    public string Mission { get; set; } = null!;
    public string? Vision { get; set; }
    public string? PlannedActivities { get; set; }
    public int EstimatedMemberCount { get; set; }
    public string? AdvisorName { get; set; }
    public string? AdvisorEmail { get; set; }
    public string? ResourceNeeds { get; set; }
    public string ApplicantName { get; set; } = "Bilinmiyor";
    public string ApplicantEmail { get; set; } = "Bilinmiyor";
    public DateTime CreatedAt { get; set; }
    public ApprovalStatus Status { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? CoordinatorNote { get; set; }
}

public class ClubApprovalDashboardViewModel
{
    public IEnumerable<ClubApplicationApprovalItemViewModel> PendingApplications { get; set; }
        = Enumerable.Empty<ClubApplicationApprovalItemViewModel>();

    public IEnumerable<ClubApplicationApprovalItemViewModel> ApprovedApplications { get; set; }
        = Enumerable.Empty<ClubApplicationApprovalItemViewModel>();

    public IEnumerable<ClubApplicationApprovalItemViewModel> RejectedApplications { get; set; }
        = Enumerable.Empty<ClubApplicationApprovalItemViewModel>();

    public PagedList<ClubApplicationApprovalItemViewModel>? PagedApplications { get; set; }

    public ClubApprovalFilterViewModel Filter { get; set; } = new();

    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
}

