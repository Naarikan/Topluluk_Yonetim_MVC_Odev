using Microsoft.Extensions.Logging;
using Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;
using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.Enums;
using Topluluk_Yonetim.MVC.Services.Interfaces;

namespace Topluluk_Yonetim.MVC.Services.Implementations;

public class AnnouncementService : Service<Announcement>, IAnnouncementService
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IAnnouncementReadRepository _announcementReadRepository;
    private readonly ILogger<AnnouncementService> _logger;

    public AnnouncementService(
        IAnnouncementRepository announcementRepository,
        IAnnouncementReadRepository announcementReadRepository,
        ILogger<AnnouncementService> logger) : base(announcementRepository)
    {
        _announcementRepository = announcementRepository;
        _announcementReadRepository = announcementReadRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Announcement>> GetAllOrderedAsync()
    {
        return await _announcementRepository.GetAllOrderedAsync();
    }

    public async Task<IEnumerable<Announcement>> GetAnnouncementsByClubIdAsync(Guid? clubId)
    {
        if (clubId.HasValue)
            return await _announcementRepository.FindAsync(a => a.ClubId == clubId.Value);
        
        return await _announcementRepository.FindAsync(a => a.ClubId == null);
    }

    public async Task<IEnumerable<Announcement>> GetAnnouncementsByAudienceAsync(AnnouncementAudience audience)
    {
        return await _announcementRepository.FindAsync(a => a.Audience == audience);
    }

    public async Task<IEnumerable<Announcement>> GetPinnedAnnouncementsAsync()
    {
        return await _announcementRepository.FindAsync(a => a.IsPinned);
    }


    public async Task<bool> MarkAsReadAsync(Guid announcementId, Guid userId)
    {
        var existingRead = await _announcementReadRepository.FirstOrDefaultAsync(ar =>
            ar.AnnouncementId == announcementId && ar.UserId == userId);

        if (existingRead != null)
            return true; // Zaten okunmuş

        var announcementRead = new AnnouncementRead
        {
            AnnouncementId = announcementId,
            UserId = userId,
            ReadAt = DateTime.UtcNow
        };

        await _announcementReadRepository.AddAsync(announcementRead);
        var result = await _announcementReadRepository.SaveChangesAsync();

        if (result > 0)
        {
            _logger.LogInformation("Announcement marked as read | AnnouncementId={AnnouncementId} | UserId={UserId}",
                announcementId, userId);
        }

        return result > 0;
    }

    public async Task<bool> IsAnnouncementReadByUserAsync(Guid announcementId, Guid userId)
    {
        return await _announcementReadRepository.AnyAsync(ar =>
            ar.AnnouncementId == announcementId && ar.UserId == userId);
    }

    public async Task<IEnumerable<Announcement>> GetUnreadAnnouncementsForUserAsync(Guid userId)
    {
        var readIds = await _announcementRepository.GetReadAnnouncementIdsByUserIdAsync(userId);
        return await _announcementRepository.GetUnreadAnnouncementsAsync(readIds);
    }

    public async Task<IEnumerable<Announcement>> GetPendingAnnouncementsAsync()
    {
        return await _announcementRepository.GetPendingAnnouncementsWithDetailsAsync();
    }

    public async Task<bool> ApproveAnnouncementAsync(Guid announcementId, Guid reviewerId)
    {
        var announcement = await _announcementRepository.GetByIdAsync(announcementId);
        if (announcement == null)
            throw new Exceptions.NotFoundException("Duyuru bulunamadı.");

        if (announcement.Status != ApprovalStatus.Pending)
            throw new Exceptions.BusinessRuleException("Sadece beklemedeki duyurular onaylanabilir.");

        announcement.Status = ApprovalStatus.Approved;
        announcement.UpdatedById = reviewerId;
        announcement.UpdatedAt = DateTime.UtcNow;

        _announcementRepository.Update(announcement);
        var result = await _announcementRepository.SaveChangesAsync();

        if (result > 0)
        {
            _logger.LogInformation("Announcement approved | AnnouncementId={AnnouncementId} | ReviewerId={ReviewerId}",
                announcementId, reviewerId);
        }

        return result > 0;
    }

    public async Task<bool> RejectAnnouncementAsync(Guid announcementId, Guid reviewerId, string? note = null)
    {
        var announcement = await _announcementRepository.GetByIdAsync(announcementId);
        if (announcement == null)
            throw new Exceptions.NotFoundException("Duyuru bulunamadı.");

        if (announcement.Status != ApprovalStatus.Pending)
            throw new Exceptions.BusinessRuleException("Sadece beklemedeki duyurular reddedilebilir.");

        announcement.Status = ApprovalStatus.Rejected;
        announcement.UpdatedById = reviewerId;
        announcement.UpdatedAt = DateTime.UtcNow;

        _announcementRepository.Update(announcement);
        var result = await _announcementRepository.SaveChangesAsync();

        if (result > 0)
        {
            _logger.LogInformation("Announcement rejected | AnnouncementId={AnnouncementId} | ReviewerId={ReviewerId} | Note={Note}",
                announcementId, reviewerId, note);
        }

        return result > 0;
    }

    public override async Task<bool> DeleteAsync(Guid id)
    {
        var announcement = await _announcementRepository.GetByIdAsync(id);
        if (announcement == null)
            return await base.DeleteAsync(id); // base zaten NotFoundException fırlatacak

        var result = await base.DeleteAsync(id);

        if (result)
        {
            _logger.LogInformation("Announcement deleted | AnnouncementId={AnnouncementId} | Title={Title} | ClubId={ClubId}",
                announcement.Id, announcement.Title, announcement.ClubId);
        }

        return result;
    }
}

