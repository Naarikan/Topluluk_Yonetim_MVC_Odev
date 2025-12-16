using System;

namespace Topluluk_Yonetim.MVC.Models.ViewModels;

public class PagingFilterViewModel
{
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ClubApprovalFilterViewModel : PagingFilterViewModel
{
    public Enums.ApprovalStatus? Status { get; set; }
}

public class MembershipApprovalFilterViewModel : PagingFilterViewModel
{
    public Enums.MembershipStatus? Status { get; set; }
}

public class EventApprovalFilterViewModel : PagingFilterViewModel
{
    public Enums.ApprovalStatus? Status { get; set; }
}

public class AnnouncementFilterViewModel : PagingFilterViewModel
{
    public Enums.AnnouncementAudience? Audience { get; set; }
    public Guid? ClubId { get; set; }
    public bool? IsPinned { get; set; }
}

public class EventListFilterViewModel : PagingFilterViewModel
{
    public Enums.ApprovalStatus? Status { get; set; }
    public Guid? ClubId { get; set; }
}

public class ClubListFilterViewModel : PagingFilterViewModel
{
    public bool OnlyActive { get; set; } = true;
}

