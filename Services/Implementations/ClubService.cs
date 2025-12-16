using Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;
using Topluluk_Yonetim.MVC.Exceptions;
using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Services.Interfaces;

namespace Topluluk_Yonetim.MVC.Services.Implementations;

public class ClubService : Service<Club>, IClubService
{
    private readonly IClubRepository _clubRepository;

    public ClubService(IClubRepository clubRepository) : base(clubRepository)
    {
        _clubRepository = clubRepository;
    }

    public async Task<Club?> GetClubWithDetailsAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ValidationException("Geçersiz topluluk kimliği.");

        return await _clubRepository.GetClubWithDetailsAsync(id);
    }

    public async Task<bool> IsClubActiveAsync(Guid id)
    {
        var club = await GetByIdAsync(id);
        return club?.IsActive ?? false;
    }

    public async Task<IEnumerable<Club>> GetClubsByPresidentIdAsync(Guid presidentId)
    {
        if (presidentId == Guid.Empty)
            throw new ValidationException("Geçersiz başkan kimliği.");

        return await _clubRepository.GetClubsByPresidentIdAsync(presidentId);
    }

    public async Task<IEnumerable<Club>> GetAllWithDetailsAsync()
    {
        return await _clubRepository.GetAllWithDetailsAsync();
    }
}

