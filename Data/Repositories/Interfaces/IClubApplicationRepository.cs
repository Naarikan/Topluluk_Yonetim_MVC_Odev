using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;

public interface IClubApplicationRepository : IRepository<ClubApplication>
{
    Task<IEnumerable<ClubApplication>> GetAllWithDetailsAsync();
    Task<ClubApplication?> GetByIdWithDetailsAsync(Guid id);
}

