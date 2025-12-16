using Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;
using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.Enums;
using Topluluk_Yonetim.MVC.Services.Interfaces;

namespace Topluluk_Yonetim.MVC.Services.Implementations;

public class EventService : Service<Event>, IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository) : base(eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<IEnumerable<Event>> GetAllWithDetailsAsync()
    {
        return await _eventRepository.GetAllWithDetailsAsync();
    }

    public async Task<Event?> GetEventWithDetailsAsync(Guid id)
    {
        return await _eventRepository.GetByIdWithDetailsAsync(id);
    }

    public async Task<IEnumerable<Event>> GetEventsByClubIdAsync(Guid clubId)
    {
        return await _eventRepository.FindAsync(e => e.ClubId == clubId);
    }

    public async Task<IEnumerable<Event>> GetEventsByStatusAsync(ApprovalStatus status)
    {
        return await _eventRepository.FindAsync(e => e.Status == status);
    }

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
    {
        return await _eventRepository.FindAsync(e => e.EventDate >= DateTime.UtcNow);
    }

}

