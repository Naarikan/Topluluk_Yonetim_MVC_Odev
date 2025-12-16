using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.Enums;
using Topluluk_Yonetim.MVC.Models.ViewModels;
using Topluluk_Yonetim.MVC.Services.Interfaces;

namespace Topluluk_Yonetim.MVC.Controllers;

[Authorize] // Liste anonim olabilir, başvuru için login isteyeceğiz
public class ClubApplicationController : Controller
{
    private readonly IClubApplicationService _clubApplicationService;
    private readonly IClubService _clubService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ClubApplicationController(
        IClubApplicationService clubApplicationService,
        IClubService clubService,
        UserManager<ApplicationUser> userManager)
    {
        _clubApplicationService = clubApplicationService;
        _clubService = clubService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ClubListFilterViewModel filter)
    {
        filter ??= new();
        filter.PageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        filter.PageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var clubs = await _clubService.GetAllWithDetailsAsync();

        var mapped = clubs.Select(c => new ClubListViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            AdvisorName = c.AdvisorName,
            PresidentName = c.President?.FullName ?? c.President?.Email ?? "Bilinmiyor",
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            MemberCount = c.Memberships?.Count(m => m.Status == MembershipStatus.Approved) ?? 0
        }).AsEnumerable();

        if (filter.OnlyActive)
        {
            mapped = mapped.Where(c => c.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLowerInvariant();
            mapped = mapped.Where(c =>
                (!string.IsNullOrEmpty(c.Name) && c.Name.ToLowerInvariant().Contains(search)) ||
                (!string.IsNullOrEmpty(c.Description) && c.Description.ToLowerInvariant().Contains(search)) ||
                (!string.IsNullOrEmpty(c.PresidentName) && c.PresidentName.ToLowerInvariant().Contains(search)));
        }

        var paged = PagedList<ClubListViewModel>.Create(mapped, filter.PageNumber, filter.PageSize);

        var viewModel = new ClubListPageViewModel
        {
            Clubs = paged,
            Filter = filter,
            TotalCount = mapped.Count()
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }

        return View(new ClubApplicationViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClubApplicationViewModel model)
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var applicantUserId = await RequireUserIdAsync();

        var presidentClubs = await _clubService.GetClubsByPresidentIdAsync(applicantUserId);
        if (presidentClubs.Any(c => c.IsActive))
        {
            ModelState.AddModelError(string.Empty, "Zaten bir kulüpte başkansınız. Başka bir kulüp için başvuru yapamazsınız.");
            return View(model);
        }

        var application = new ClubApplication
        {
            ClubName = model.ClubName,
            Mission = model.Mission,
            Vision = model.Vision,
            PlannedActivities = model.PlannedActivities,
            EstimatedMemberCount = model.EstimatedMemberCount,
            AdvisorName = model.AdvisorName,
            AdvisorEmail = model.AdvisorEmail,
            ResourceNeeds = model.ResourceNeeds,
            Status = ApprovalStatus.Pending,
            ApplicantUserId = applicantUserId
        };

        await _clubApplicationService.CreateAsync(application);

        TempData["SuccessMessage"] = "Topluluk başvurunuz başarıyla gönderildi. Onay sürecinde size bilgi verilecektir.";
        return RedirectToAction(nameof(Index));
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
}

