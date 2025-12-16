using System;
using System.Collections.Generic;
using System.Linq;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Models.ViewModels;

public class MembershipApprovalItemViewModel
{
    public Guid Id { get; set; }
    public Guid ClubId { get; set; }
    public string ClubName { get; set; } = null!;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = "Bilinmiyor";
    public string UserEmail { get; set; } = "Bilinmiyor";
    public string? StudentNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public MembershipStatus Status { get; set; }
    public ClubRole Role { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? Note { get; set; }
}

public class MembershipApprovalDashboardViewModel
{
    public IEnumerable<MembershipApprovalItemViewModel> PendingMemberships { get; set; }
        = Enumerable.Empty<MembershipApprovalItemViewModel>();

    public IEnumerable<MembershipApprovalItemViewModel> ApprovedMemberships { get; set; }
        = Enumerable.Empty<MembershipApprovalItemViewModel>();

    public IEnumerable<MembershipApprovalItemViewModel> RejectedMemberships { get; set; }
        = Enumerable.Empty<MembershipApprovalItemViewModel>();

    public PagedList<MembershipApprovalItemViewModel>? PagedMemberships { get; set; }

    public MembershipApprovalFilterViewModel Filter { get; set; } = new();

    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
}

