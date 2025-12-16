using Microsoft.EntityFrameworkCore;
using Topluluk_Yonetim.MVC.Data;
using Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;
using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Data.Repositories.Implementations;

public class EventApprovalRepository : Repository<EventApproval>, IEventApprovalRepository
{
    public EventApprovalRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<EventApproval>> GetByEventIdWithDetailsAsync(Guid eventId)
    {
        return await _context.EventApprovals
            .Include(ea => ea.Reviewer)
            .Where(ea => ea.EventId == eventId)
            .OrderByDescending(ea => ea.CreatedAt)
            .ToListAsync();
    }

    public async Task<EventApproval?> GetLatestByEventIdAsync(Guid eventId)
    {
        return await _context.EventApprovals
            .Include(ea => ea.Reviewer)
            .Where(ea => ea.EventId == eventId)
            .OrderByDescending(ea => ea.CreatedAt)
            .FirstOrDefaultAsync();
    }
}

