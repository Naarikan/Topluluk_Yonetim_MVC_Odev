using Topluluk_Yonetim.MVC.Data;
using Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;
using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Data.Repositories.Implementations;

public class AnnouncementReadRepository : Repository<AnnouncementRead>, IAnnouncementReadRepository
{
    public AnnouncementReadRepository(ApplicationDbContext context) : base(context)
    {
    }
}

