using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;

public interface IEventRepository : IRepository<Event>
{
    Task<IEnumerable<Event>> GetAllWithDetailsAsync();
    Task<Event?> GetByIdWithDetailsAsync(Guid id);
}

