using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Services.Interfaces;

public interface IClubApplicationService : IService<ClubApplication>
{
    Task<IEnumerable<ClubApplication>> GetAllWithDetailsAsync();
    Task<ClubApplication?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<ClubApplication>> GetPendingApplicationsAsync();
    Task<bool> ApproveApplicationAsync(Guid applicationId, Guid reviewerId, string? note = null);
    Task<bool> RejectApplicationAsync(Guid applicationId, Guid reviewerId, string? note = null);
    Task<IEnumerable<ClubApplication>> GetApplicationsByUserIdAsync(Guid userId);
}

