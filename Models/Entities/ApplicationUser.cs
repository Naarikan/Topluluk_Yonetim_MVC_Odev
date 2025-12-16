using Microsoft.AspNetCore.Identity;

namespace Topluluk_Yonetim.MVC.Models.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FullName { get; set; }

    public string? StudentNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ClubMembership> ClubMemberships { get; set; } = new List<ClubMembership>();

    public ICollection<Club> ManagedClubs { get; set; } = new List<Club>();

    public ICollection<AnnouncementRead> ReadAnnouncements { get; set; } = new List<AnnouncementRead>();
}

