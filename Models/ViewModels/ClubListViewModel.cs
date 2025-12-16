using System;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Models.ViewModels;

public class ClubListViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? AdvisorName { get; set; }
    public string PresidentName { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
}

public class ClubListPageViewModel
{
    public PagedList<ClubListViewModel> Clubs { get; set; } = new(new List<ClubListViewModel>(), 0, 1, 10);
    public ClubListFilterViewModel Filter { get; set; } = new();
    public int TotalCount { get; set; }
}

