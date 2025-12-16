using Microsoft.EntityFrameworkCore;
using Topluluk_Yonetim.MVC.Data;
using Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;
using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Data.Repositories.Implementations;

public class AnnouncementRepository : Repository<Announcement>, IAnnouncementRepository
{
    public AnnouncementRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Announcement>> GetAllOrderedAsync()
    {
        return await _context.Announcements
            .Include(a => a.Club)
            .Where(a => a.Status == ApprovalStatus.Approved)
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Announcement?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Announcements
            .Include(a => a.Club)
            .Include(a => a.ReadBy)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Guid>> GetReadAnnouncementIdsByUserIdAsync(Guid userId)
    {
        return await _context.AnnouncementReads
            .Where(ar => ar.UserId == userId)
            .Select(ar => ar.AnnouncementId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Announcement>> GetUnreadAnnouncementsAsync(IEnumerable<Guid> readIds)
    {
        return await _context.Announcements
            .Include(a => a.Club)
            .Where(a => !readIds.Contains(a.Id))
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Announcement>> GetPendingAnnouncementsWithDetailsAsync()
    {
        return await _context.Announcements
            .Include(a => a.Club)
            .Where(a => a.Status == ApprovalStatus.Pending && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}

