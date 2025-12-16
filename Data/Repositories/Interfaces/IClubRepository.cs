using Microsoft.EntityFrameworkCore;
using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;

public interface IClubRepository : IRepository<Club>
{
    Task<Club?> GetClubWithDetailsAsync(Guid id);
    Task<IEnumerable<Club>> GetClubsByPresidentIdAsync(Guid presidentId);
    Task<IEnumerable<Club>> GetAllWithDetailsAsync();
}

