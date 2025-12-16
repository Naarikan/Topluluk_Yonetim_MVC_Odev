using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;

public interface IEventApprovalRepository : IRepository<EventApproval>
{
    Task<IEnumerable<EventApproval>> GetByEventIdWithDetailsAsync(Guid eventId);
    Task<EventApproval?> GetLatestByEventIdAsync(Guid eventId);
}

