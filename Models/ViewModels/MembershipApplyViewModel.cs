using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Models.ViewModels;

public class MembershipApplyViewModel
{
    public List<ClubItemViewModel> ActiveClubs { get; set; } = new();
    public Guid? SelectedClubId { get; set; }
}

public class ClubItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? AdvisorName { get; set; }
    public string PresidentName { get; set; } = null!;
    public bool CanApply { get; set; } = true;
    public string? StatusMessage { get; set; }
    public bool CanCancel { get; set; }
}

