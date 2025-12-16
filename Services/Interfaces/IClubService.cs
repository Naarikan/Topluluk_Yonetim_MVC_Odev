using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Services.Interfaces;

public interface IClubService : IService<Club>
{
    Task<Club?> GetClubWithDetailsAsync(Guid id);
    Task<bool> IsClubActiveAsync(Guid id);
    Task<IEnumerable<Club>> GetClubsByPresidentIdAsync(Guid presidentId);
    Task<IEnumerable<Club>> GetAllWithDetailsAsync();
}

