using Microsoft.EntityFrameworkCore;
using Topluluk_Yonetim.MVC.Data;
using Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;
using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Data.Repositories.Implementations;

public class MembershipRepository : Repository<ClubMembership>, IMembershipRepository
{
    public MembershipRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ClubMembership>> GetAllWithDetailsAsync()
    {
        return await _context.ClubMemberships
            .Include(cm => cm.Club)
            .Include(cm => cm.User)
            .Include(cm => cm.ReviewedBy)
            .ToListAsync();
    }

    public async Task<ClubMembership?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.ClubMemberships
            .Include(cm => cm.Club)
            .Include(cm => cm.User)
            .Include(cm => cm.ReviewedBy)
            .FirstOrDefaultAsync(cm => cm.Id == id);
    }

    public async Task<IEnumerable<ClubMembership>> GetByUserIdWithDetailsAsync(Guid userId)
    {
        return await _context.ClubMemberships
            .Include(cm => cm.Club)
            .Where(cm => cm.UserId == userId)
            .ToListAsync();
    }
}

