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
public class AnnouncementController : Controller
{
    private readonly IAnnouncementService _announcementService;
    private readonly IClubService _clubService;
    private readonly IMembershipService _membershipService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AnnouncementController(
        IAnnouncementService announcementService,
        IClubService clubService,
        IMembershipService membershipService,
        UserManager<ApplicationUser> userManager)
    {
        _announcementService = announcementService;
        _clubService = clubService;
        _membershipService = membershipService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] AnnouncementFilterViewModel filter)
    {
        filter ??= new();
        filter.PageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        filter.PageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var announcements = await _announcementService.GetAllOrderedAsync();
        var userId = await RequireUserIdAsync();
        var (isAdmin, isPresident) = GetRoles();

        var viewModelList = new List<AnnouncementListViewModel>();
        
        HashSet<Guid>? presidentClubIds = null;

        foreach (var announcement in announcements)
        {
            if (announcement.Audience == AnnouncementAudience.SpecificClubMembers && announcement.ClubId.HasValue)
            {
                var clubId = announcement.ClubId.Value;

                var allowed = false;

                if (isAdmin)
                {
                    allowed = true;
                }
                else
                {
                    if (isPresident)
                    {
                        presidentClubIds ??= (await _clubService
                                .GetClubsByPresidentIdAsync(userId))
                            .Where(c => c.IsActive)
                            .Select(c => c.Id)
                            .ToHashSet();

                        if (presidentClubIds.Contains(clubId))
                        {
                            allowed = true;
                        }
                    }

                    if (!allowed)
                    {
                        var isMember = await _membershipService.IsUserMemberOfClubAsync(userId, clubId);
                        if (isMember)
                        {
                            allowed = true;
                        }
                    }
                }

                if (!allowed)
                {
                    continue;
                }
            }

            var isRead = await _announcementService.IsAnnouncementReadByUserAsync(announcement.Id, userId);
            
            viewModelList.Add(new AnnouncementListViewModel
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Content = announcement.Content.Length > 200 ? announcement.Content.Substring(0, 200) + "..." : announcement.Content,
                Audience = announcement.Audience,
                ClubId = announcement.ClubId,
                ClubName = announcement.Club?.Name,
                IsPinned = announcement.IsPinned,
                CreatedAt = announcement.CreatedAt,
                CreatedByName = null, 
                IsRead = isRead
            });
        }

        var query = viewModelList.AsEnumerable();

        if (filter.Audience.HasValue)
        {
            query = query.Where(x => x.Audience == filter.Audience.Value);
        }

        if (filter.ClubId.HasValue && filter.ClubId.Value != Guid.Empty)
        {
            query = query.Where(x => x.ClubId == filter.ClubId.Value);
        }

        if (filter.IsPinned.HasValue)
        {
            query = query.Where(x => x.IsPinned == filter.IsPinned.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLowerInvariant();
            query = query.Where(x =>
                (!string.IsNullOrEmpty(x.Title) && x.Title.ToLowerInvariant().Contains(search)) ||
                (!string.IsNullOrEmpty(x.Content) && x.Content.ToLowerInvariant().Contains(search)) ||
                (!string.IsNullOrEmpty(x.ClubName) && x.ClubName.ToLowerInvariant().Contains(search)));
        }

        var paged = PagedList<AnnouncementListViewModel>.Create(query, filter.PageNumber, filter.PageSize);

        var viewModel = new AnnouncementIndexViewModel
        {
            Announcements = paged,
            Filter = filter,
            TotalCount = query.Count()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var (isAdmin, isPresident) = GetRoles();
        if (!isAdmin && !isPresident)
        {
            TempData["ErrorMessage"] = "Duyuru oluşturma yetkiniz bulunmuyor.";
            return RedirectToAction(nameof(Index));
        }

        var clubs = await _clubService.GetAllWithDetailsAsync();

        var allowedAudiences = isAdmin
            ? new List<AnnouncementAudience>
            {
                AnnouncementAudience.AllStudents,
                AnnouncementAudience.Presidents,
                AnnouncementAudience.SpecificClubMembers
            }
            : new List<AnnouncementAudience>
            {
                AnnouncementAudience.AllStudents,
                AnnouncementAudience.ClubMembers
            };

        ViewBag.AllowedAudiences = allowedAudiences;

        var viewModel = new AnnouncementCreateViewModel
        {
            Audience = allowedAudiences.First()
        };

        if (isAdmin)
        {
            ViewBag.Clubs = clubs.Where(c => c.IsActive).ToList();
        }
        else
        {
            var userId = await RequireUserIdAsync();
            var presidentClubs = (await _clubService.GetClubsByPresidentIdAsync(userId)).Where(c => c.IsActive).ToList();
            if (!presidentClubs.Any())
            {
                TempData["ErrorMessage"] = "Başkan olarak bağlı olduğunuz aktif bir kulüp bulunamadı.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Clubs = presidentClubs;
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AnnouncementCreateViewModel model)
    {
        var (isAdmin, isPresident) = GetRoles();
        if (!isAdmin && !isPresident)
        {
            TempData["ErrorMessage"] = "Duyuru oluşturma yetkiniz bulunmuyor.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            var clubs = await _clubService.GetAllWithDetailsAsync();
            ViewBag.Clubs = clubs.Where(c => c.IsActive).ToList();

            var (isAdminForView, isPresidentForView) = GetRoles();
            ViewBag.AllowedAudiences = isAdminForView
                ? new List<AnnouncementAudience>
                {
                    AnnouncementAudience.AllStudents,
                    AnnouncementAudience.Presidents,
                    AnnouncementAudience.SpecificClubMembers
                }
                : new List<AnnouncementAudience>
                {
                    AnnouncementAudience.AllStudents,
                    AnnouncementAudience.ClubMembers
                };

            return View(model);
        }

        try
        {
            var userId = await RequireUserIdAsync();
            var clubs = await _clubService.GetAllWithDetailsAsync();

            Guid? targetClubId = model.ClubId == Guid.Empty ? null : model.ClubId;

            if (isPresident)
            {
                var presidentClubs = (await _clubService.GetClubsByPresidentIdAsync(userId)).Where(c => c.IsActive).Select(c => c.Id).ToHashSet();

                if (model.Audience == AnnouncementAudience.Presidents || model.Audience == AnnouncementAudience.SpecificClubMembers)
                {
                    TempData["ErrorMessage"] = "Başkanlar yalnızca tüm öğrenciler veya kendi kulüp üyeleri için duyuru yapabilir.";
                    ViewBag.Clubs = clubs.Where(c => presidentClubs.Contains(c.Id)).ToList();
                    ViewBag.AllowedAudiences = new List<AnnouncementAudience>
                    {
                        AnnouncementAudience.AllStudents,
                        AnnouncementAudience.ClubMembers
                    };
                    return View(model);
                }

                if (model.Audience == AnnouncementAudience.ClubMembers)
                {
                    if (!targetClubId.HasValue || !presidentClubs.Contains(targetClubId.Value))
                    {
                        TempData["ErrorMessage"] = "Yalnızca başkanı olduğunuz kulüp için duyuru oluşturabilirsiniz.";
                        ViewBag.Clubs = clubs.Where(c => presidentClubs.Contains(c.Id)).ToList();
                        ViewBag.AllowedAudiences = new List<AnnouncementAudience>
                        {
                            AnnouncementAudience.AllStudents,
                            AnnouncementAudience.ClubMembers
                        };
                        return View(model);
                    }
                }
                else
                {
                    targetClubId = null;
                }
            }
            
            var announcement = new Announcement
            {
                Title = model.Title,
                Content = model.Content,
                Audience = model.Audience,
                ClubId = targetClubId,
                IsPinned = model.IsPinned,
                // Admin direkt onaylı, başkanlar beklemede
                Status = isAdmin ? ApprovalStatus.Approved : ApprovalStatus.Pending,
                CreatedById = userId
            };

            await _announcementService.CreateAsync(announcement);
            
            if (isAdmin)
            {
                TempData["SuccessMessage"] = "Duyuru başarıyla oluşturuldu ve yayınlandı.";
            }
            else
            {
                TempData["SuccessMessage"] = "Duyuru başarıyla oluşturuldu. Admin onayından sonra yayınlanacaktır.";
            }
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            var clubs = await _clubService.GetAllWithDetailsAsync();
            ViewBag.Clubs = clubs.Where(c => c.IsActive).ToList();
            return View(model);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,President")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var announcement = await _announcementService.GetByIdAsync(id);
        if (announcement == null)
            throw new NotFoundException("Duyuru bulunamadı.");

        var (isAdmin, isPresident) = GetRoles();
        var clubs = await _clubService.GetAllWithDetailsAsync();

        if (isPresident)
        {
            var userId = await RequireUserIdAsync();

            if (announcement.CreatedById != userId)
                return Forbid();

            var presidentClubIds = (await _clubService.GetClubsByPresidentIdAsync(userId))
                .Where(c => c.IsActive)
                .Select(c => c.Id)
                .ToHashSet();

            if (!announcement.ClubId.HasValue || !presidentClubIds.Contains(announcement.ClubId.Value))
                return Forbid();

            ViewBag.Clubs = clubs.Where(c => presidentClubIds.Contains(c.Id)).ToList();

            ViewBag.AllowedAudiences = new List<AnnouncementAudience>
            {
                AnnouncementAudience.AllStudents,
                AnnouncementAudience.ClubMembers
            };
        }
        else
        {
            ViewBag.Clubs = clubs.Where(c => c.IsActive).ToList();
            ViewBag.AllowedAudiences = new List<AnnouncementAudience>
            {
                AnnouncementAudience.AllStudents,
                AnnouncementAudience.Presidents,
                AnnouncementAudience.SpecificClubMembers
            };
        }

        var vm = new AnnouncementEditViewModel
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Content = announcement.Content,
            Audience = announcement.Audience,
            ClubId = announcement.ClubId,
            IsPinned = announcement.IsPinned
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,President")]
    public async Task<IActionResult> Edit(AnnouncementEditViewModel model)
    {
        var (isAdmin, isPresident) = GetRoles();
        if (!isAdmin && !isPresident)
        {
            TempData["ErrorMessage"] = "Duyuru düzenleme yetkiniz bulunmuyor.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            var clubs = await _clubService.GetAllWithDetailsAsync();
            if (isPresident)
            {
                var userId = await RequireUserIdAsync();
                var presidentClubIds = (await _clubService.GetClubsByPresidentIdAsync(userId))
                    .Where(c => c.IsActive)
                    .Select(c => c.Id)
                    .ToHashSet();
                ViewBag.Clubs = clubs.Where(c => presidentClubIds.Contains(c.Id)).ToList();
                ViewBag.AllowedAudiences = new List<AnnouncementAudience>
                {
                    AnnouncementAudience.AllStudents,
                    AnnouncementAudience.ClubMembers
                };
            }
            else
            {
                ViewBag.Clubs = clubs.Where(c => c.IsActive).ToList();
                ViewBag.AllowedAudiences = new List<AnnouncementAudience>
                {
                    AnnouncementAudience.AllStudents,
                    AnnouncementAudience.Presidents,
                    AnnouncementAudience.SpecificClubMembers
                };
            }
            return View(model);
        }

        var announcement = await _announcementService.GetByIdAsync(model.Id);
        if (announcement == null)
            throw new NotFoundException("Duyuru bulunamadı.");

        var allClubs = await _clubService.GetAllWithDetailsAsync();

        if (isPresident)
        {
            var userId = await RequireUserIdAsync();

            if (announcement.CreatedById != userId)
            {
                TempData["ErrorMessage"] = "Sadece oluşturduğunuz duyuruları düzenleyebilirsiniz.";
                return RedirectToAction(nameof(Index));
            }

            var presidentClubIds = (await _clubService.GetClubsByPresidentIdAsync(userId))
                .Where(c => c.IsActive)
                .Select(c => c.Id)
                .ToHashSet();

            if (model.Audience == AnnouncementAudience.Presidents || model.Audience == AnnouncementAudience.SpecificClubMembers)
            {
                TempData["ErrorMessage"] = "Başkanlar yalnızca tüm öğrenciler veya kendi kulüp üyeleri için duyuru yapabilir.";
                ViewBag.Clubs = allClubs.Where(c => presidentClubIds.Contains(c.Id)).ToList();
                ViewBag.AllowedAudiences = new List<AnnouncementAudience>
                {
                    AnnouncementAudience.AllStudents,
                    AnnouncementAudience.ClubMembers
                };
                return View(model);
            }

            if (model.Audience == AnnouncementAudience.ClubMembers)
            {
                if (!model.ClubId.HasValue || !presidentClubIds.Contains(model.ClubId.Value))
                {
                    TempData["ErrorMessage"] = "Yalnızca başkanı olduğunuz kulüp için duyuru düzenleyebilirsiniz.";
                    ViewBag.Clubs = allClubs.Where(c => presidentClubIds.Contains(c.Id)).ToList();
                    ViewBag.AllowedAudiences = new List<AnnouncementAudience>
                    {
                        AnnouncementAudience.AllStudents,
                        AnnouncementAudience.ClubMembers
                    };
                    return View(model);
                }
            }
            else
            {
                model.ClubId = null;
            }
        }

        announcement.Title = model.Title;
        announcement.Content = model.Content;
        announcement.Audience = model.Audience;
        announcement.ClubId = model.ClubId == Guid.Empty ? null : model.ClubId;
        announcement.IsPinned = model.IsPinned;

        await _announcementService.UpdateAsync(announcement);
        TempData["SuccessMessage"] = "Duyuru güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var announcement = await _announcementService.GetByIdAsync(id);
        if (announcement == null)
            throw new NotFoundException("Duyuru bulunamadı.");

        var userId = await RequireUserIdAsync();
        
        await _announcementService.MarkAsReadAsync(id, userId);

        var audienceText = announcement.Audience switch
        {
            AnnouncementAudience.AllStudents => "Tüm Öğrenciler",
            AnnouncementAudience.Presidents => "Başkanlar",
            AnnouncementAudience.ClubMembers => "Kulüp Üyeleri",
            AnnouncementAudience.SpecificClubMembers => "Belirli Kulüp Üyeleri",
            _ => "Bilinmiyor"
        };

        var viewModel = new AnnouncementDetailsViewModel
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Content = announcement.Content,
            Audience = announcement.Audience,
            AudienceText = audienceText,
            ClubId = announcement.ClubId,
            ClubName = announcement.Club?.Name,
            IsPinned = announcement.IsPinned,
            CreatedAt = announcement.CreatedAt,
            CreatedByName = null, // TODO: CreatedBy navigation property eklenebilir
            IsRead = await _announcementService.IsAnnouncementReadByUserAsync(id, userId),
            ReadCount = announcement.ReadBy?.Count ?? 0
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _announcementService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Duyuru başarıyla silindi.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

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

    private (bool isAdmin, bool isPresident) GetRoles()
    {
        var isAdmin = User?.IsInRole("Admin") == true;
        var isPresident = User?.IsInRole("President") == true;
        return (isAdmin, isPresident);
    }
}

