using Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;
using Topluluk_Yonetim.MVC.Exceptions;
using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.Enums;
using Topluluk_Yonetim.MVC.Services.Interfaces;

namespace Topluluk_Yonetim.MVC.Services.Implementations;

public class EventApprovalService : IEventApprovalService
{
    private readonly IEventApprovalRepository _approvalRepository;
    private readonly IRepository<Event> _eventRepository;

    public EventApprovalService(
        IEventApprovalRepository approvalRepository,
        IRepository<Event> eventRepository)
    {
        _approvalRepository = approvalRepository;
        _eventRepository = eventRepository;
    }

    public async Task<IEnumerable<EventApproval>> GetApprovalsByEventIdAsync(Guid eventId)
    {
        return await _approvalRepository.GetByEventIdWithDetailsAsync(eventId);
    }

    public async Task<EventApproval?> GetLatestApprovalByEventIdAsync(Guid eventId)
    {
        return await _approvalRepository.GetLatestByEventIdAsync(eventId);
    }

    public async Task<EventApproval> CreateApprovalAsync(EventApproval approval)
    {
        await _approvalRepository.AddAsync(approval);
        await _approvalRepository.SaveChangesAsync();
        return approval;
    }

    public async Task<bool> ApproveEventAsync(Guid eventId, Guid reviewerId, string? note = null)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(eventId);
        if (eventEntity == null)
            throw new NotFoundException("Etkinlik bulunamadı.");

        if (eventEntity.Status != ApprovalStatus.Pending)
            throw new BusinessRuleException("Sadece beklemedeki etkinlikler onaylanabilir.");

        var approval = new EventApproval
        {
            EventId = eventId,
            ReviewerId = reviewerId,
            Status = ApprovalStatus.Approved,
            Comment = note
        };

        eventEntity.Status = ApprovalStatus.Approved;

        await _approvalRepository.AddAsync(approval);
        _eventRepository.Update(eventEntity);
        await _approvalRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RejectEventAsync(Guid eventId, Guid reviewerId, string? note = null)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(eventId);
        if (eventEntity == null)
            throw new NotFoundException("Etkinlik bulunamadı.");

        if (eventEntity.Status != ApprovalStatus.Pending)
            throw new BusinessRuleException("Sadece beklemedeki etkinlikler reddedilebilir.");

        var approval = new EventApproval
        {
            EventId = eventId,
            ReviewerId = reviewerId,
            Status = ApprovalStatus.Rejected,
            Comment = note
        };

        eventEntity.Status = ApprovalStatus.Rejected;

        await _approvalRepository.AddAsync(approval);
        _eventRepository.Update(eventEntity);
        await _approvalRepository.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<EventApproval>> GetApprovalsByReviewerIdAsync(Guid reviewerId)
    {
        return await _approvalRepository.FindAsync(ea => ea.ReviewerId == reviewerId);
    }
}

