using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Services.Interfaces;

public interface IEventService : IService<Event>
{
    Task<IEnumerable<Event>> GetAllWithDetailsAsync();
    Task<Event?> GetEventWithDetailsAsync(Guid id);
    Task<IEnumerable<Event>> GetEventsByClubIdAsync(Guid clubId);
    Task<IEnumerable<Event>> GetEventsByStatusAsync(ApprovalStatus status);
    Task<IEnumerable<Event>> GetUpcomingEventsAsync();
}

