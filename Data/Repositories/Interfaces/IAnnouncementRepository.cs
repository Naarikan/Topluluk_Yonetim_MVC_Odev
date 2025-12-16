using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;

public interface IAnnouncementRepository : IRepository<Announcement>
{
    Task<IEnumerable<Announcement>> GetAllOrderedAsync();
    Task<Announcement?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Guid>> GetReadAnnouncementIdsByUserIdAsync(Guid userId);
    Task<IEnumerable<Announcement>> GetUnreadAnnouncementsAsync(IEnumerable<Guid> readIds);
    Task<IEnumerable<Announcement>> GetPendingAnnouncementsWithDetailsAsync();
}

