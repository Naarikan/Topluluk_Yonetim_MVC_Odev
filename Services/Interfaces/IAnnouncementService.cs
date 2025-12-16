using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Services.Interfaces;

public interface IAnnouncementService : IService<Announcement>
{
    Task<IEnumerable<Announcement>> GetAllOrderedAsync();
    Task<IEnumerable<Announcement>> GetAnnouncementsByClubIdAsync(Guid? clubId);
    Task<IEnumerable<Announcement>> GetAnnouncementsByAudienceAsync(AnnouncementAudience audience);
    Task<IEnumerable<Announcement>> GetPinnedAnnouncementsAsync();
    Task<bool> MarkAsReadAsync(Guid announcementId, Guid userId);
    Task<bool> IsAnnouncementReadByUserAsync(Guid announcementId, Guid userId);
    Task<IEnumerable<Announcement>> GetUnreadAnnouncementsForUserAsync(Guid userId);
    Task<IEnumerable<Announcement>> GetPendingAnnouncementsAsync();
    Task<bool> ApproveAnnouncementAsync(Guid announcementId, Guid reviewerId);
    Task<bool> RejectAnnouncementAsync(Guid announcementId, Guid reviewerId, string? note = null);
}

