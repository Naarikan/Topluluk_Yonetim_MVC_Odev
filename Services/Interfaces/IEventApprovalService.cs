using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Services.Interfaces;

public interface IEventApprovalService
{
    Task<IEnumerable<EventApproval>> GetApprovalsByEventIdAsync(Guid eventId);
    Task<EventApproval?> GetLatestApprovalByEventIdAsync(Guid eventId);
    Task<EventApproval> CreateApprovalAsync(EventApproval approval);
    Task<bool> ApproveEventAsync(Guid eventId, Guid reviewerId, string? note = null);
    Task<bool> RejectEventAsync(Guid eventId, Guid reviewerId, string? note = null);
    Task<IEnumerable<EventApproval>> GetApprovalsByReviewerIdAsync(Guid reviewerId);
}

