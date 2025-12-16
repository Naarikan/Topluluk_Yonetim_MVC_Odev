using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.Enums;
using Topluluk_Yonetim.MVC.Models.ViewModels;
using Topluluk_Yonetim.MVC.Services.Interfaces;
using Topluluk_Yonetim.MVC.Exceptions;

namespace Topluluk_Yonetim.MVC.Controllers;

[Authorize]
public class EventController : Controller
{
    private readonly IEventService _eventService;
    private readonly IClubService _clubService;
    private readonly IMembershipService _membershipService;
    private readonly UserManager<ApplicationUser> _userManager;

    public EventController(
        IEventService eventService,
        IClubService clubService,
        IMembershipService membershipService,
        UserManager<ApplicationUser> userManager)
    {
        _eventService = eventService;
        _clubService = clubService;
        _membershipService = membershipService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] EventListFilterViewModel filter)
    {
        filter ??= new();
        filter.PageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        filter.PageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var events = await _eventService.GetAllWithDetailsAsync();
        
        var eventList = events.Select(e => new EventListViewModel
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            EventDate = e.EventDate,
            EstimatedBudget = e.EstimatedBudget,
            Status = e.Status,
            ClubName = e.Club?.Name ?? "Bilinmiyor",
            ClubId = e.ClubId,
            CreatedAt = e.CreatedAt
        }).OrderBy(e => e.EventDate).ToList();

        var query = eventList.AsEnumerable();

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin)
        {
            query = query.Where(x => x.Status == ApprovalStatus.Approved);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.ClubId.HasValue && filter.ClubId.Value != Guid.Empty)
        {
            query = query.Where(x => x.ClubId == filter.ClubId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLowerInvariant();
            query = query.Where(x =>
                (!string.IsNullOrEmpty(x.Title) && x.Title.ToLowerInvariant().Contains(search)) ||
                (!string.IsNullOrEmpty(x.Description) && x.Description.ToLowerInvariant().Contains(search)) ||
                (!string.IsNullOrEmpty(x.ClubName) && x.ClubName.ToLowerInvariant().Contains(search)));
        }

        var paged = PagedList<EventListViewModel>.Create(query, filter.PageNumber, filter.PageSize);

        var viewModel = new EventIndexViewModel
        {
            Events = paged,
            Filter = filter,
            TotalCount = query.Count()
        };

        return View(viewModel);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,President")]
    public async Task<IActionResult> Create()
    {
        var clubId = await GetUserClubIdAsync();
        if (clubId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Etkinlik başvurusu yapabilmek için aktif bir topluluğa üye olmanız gerekmektedir.";
            return RedirectToAction(nameof(Index));
        }

        var defaultDate = DateTime.Now.AddDays(7);
        defaultDate = new DateTime(defaultDate.Year, defaultDate.Month, defaultDate.Day, defaultDate.Hour, defaultDate.Minute, 0, DateTimeKind.Local);

        var viewModel = new EventCreateViewModel
        {
            ClubId = clubId,
            EventDate = defaultDate
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,President")]
    public async Task<IActionResult> Create(EventCreateViewModel model)
    {
        var clubId = await GetUserClubIdAsync();
        if (clubId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Etkinlik başvurusu yapabilmek için aktif bir topluluğa üye olmanız gerekmektedir.";
            return RedirectToAction(nameof(Index));
        }

        model.ClubId = clubId;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var userId = await RequireUserIdAsync();
            
            var localEventDate = new DateTime(
                model.EventDate.Year, 
                model.EventDate.Month, 
                model.EventDate.Day, 
                model.EventDate.Hour, 
                model.EventDate.Minute, 
                0,
                DateTimeKind.Local);
            
            var utcEventDate = localEventDate.ToUniversalTime();
            
            var eventEntity = new Event
            {
                ClubId = model.ClubId,
                Title = model.Title,
                Description = model.Description,
                EventDate = utcEventDate, // UTC olarak kaydet
                EstimatedBudget = model.EstimatedBudget,
                Status = ApprovalStatus.Pending,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow // Açıkça UTC olarak set et
            };

            await _eventService.CreateAsync(eventEntity);
            TempData["SuccessMessage"] = "Etkinlik başvurunuz başarıyla gönderildi. Onay bekleniyor.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return View(model);
        }
    }

    private async Task<Guid> RequireUserIdAsync()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                return user.Id;
            }
        }

        throw new UnauthorizedAccessException("Kullanıcı oturumu bulunamadı.");
    }

    private async Task<Guid> GetUserClubIdAsync()
    {
        var userId = await RequireUserIdAsync();
        
        var memberships = await _membershipService.GetMembershipsByUserIdAsync(userId);
        var approvedMemberships = memberships
            .Where(m => m.Status == MembershipStatus.Approved && m.Club?.IsActive == true)
            .ToList();

        if (approvedMemberships.Any())
        {
            return approvedMemberships.First().ClubId;
        }

        var clubs = await _clubService.GetAllWithDetailsAsync();
        var firstActiveClub = clubs.FirstOrDefault(c => c.IsActive);
        
        return firstActiveClub?.Id ?? Guid.Empty;
    }
}

