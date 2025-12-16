using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;

public interface IMembershipRepository : IRepository<ClubMembership>
{
    Task<IEnumerable<ClubMembership>> GetAllWithDetailsAsync();
    Task<ClubMembership?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<ClubMembership>> GetByUserIdWithDetailsAsync(Guid userId);
}

