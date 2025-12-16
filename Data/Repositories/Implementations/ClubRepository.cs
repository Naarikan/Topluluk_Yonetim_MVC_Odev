using Microsoft.EntityFrameworkCore;
using Topluluk_Yonetim.MVC.Data;
using Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;
using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Data.Repositories.Implementations;

public class ClubRepository : Repository<Club>, IClubRepository
{
    public ClubRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Club?> GetClubWithDetailsAsync(Guid id)
    {
        return await _context.Clubs
            .Include(c => c.President)
            .Include(c => c.Memberships)
            .ThenInclude(m => m.User)
            .Include(c => c.Events)
            .Include(c => c.Announcements)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Club>> GetClubsByPresidentIdAsync(Guid presidentId)
    {
        return await FindAsync(c => c.PresidentId == presidentId);
    }

    public async Task<IEnumerable<Club>> GetAllWithDetailsAsync()
    {
        return await _context.Clubs
            .Include(c => c.President)
            .Include(c => c.Memberships)
            .ToListAsync();
    }
}

