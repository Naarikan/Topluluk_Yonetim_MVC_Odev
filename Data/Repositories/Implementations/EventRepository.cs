using Microsoft.EntityFrameworkCore;
using Topluluk_Yonetim.MVC.Data;
using Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;
using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Data.Repositories.Implementations;

public class EventRepository : Repository<Event>, IEventRepository
{
    public EventRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Event>> GetAllWithDetailsAsync()
    {
        return await _context.Events
            .Include(e => e.Club)
            .Include(e => e.ApprovalHistory)
            .ThenInclude(ea => ea.Reviewer)
            .ToListAsync();
    }

    public async Task<Event?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Events
            .Include(e => e.Club)
            .Include(e => e.ApprovalHistory)
            .ThenInclude(ea => ea.Reviewer)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}

