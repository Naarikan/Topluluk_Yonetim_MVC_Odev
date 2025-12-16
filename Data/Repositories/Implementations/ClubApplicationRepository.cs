using Microsoft.EntityFrameworkCore;
using Topluluk_Yonetim.MVC.Data;
using Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;
using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Data.Repositories.Implementations;

public class ClubApplicationRepository : Repository<ClubApplication>, IClubApplicationRepository
{
    public ClubApplicationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ClubApplication>> GetAllWithDetailsAsync()
    {
        return await _context.ClubApplications
            .Include(ca => ca.ApplicantUser)
            .Include(ca => ca.ReviewedBy)
            .ToListAsync();
    }

    public async Task<ClubApplication?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.ClubApplications
            .Include(ca => ca.ApplicantUser)
            .Include(ca => ca.ReviewedBy)
            .FirstOrDefaultAsync(ca => ca.Id == id);
    }
}

