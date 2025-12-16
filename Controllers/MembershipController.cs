using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.Enums;
using Topluluk_Yonetim.MVC.Models.ViewModels;
using Topluluk_Yonetim.MVC.Services.Interfaces;
using Topluluk_Yonetim.MVC.Exceptions;

namespace Topluluk_Yonetim.MVC.Controllers;

[Authorize] // Üyelik başvurusu için login gerekli
public class MembershipController : Controller
{
    private readonly IMembershipService _membershipService;
    private readonly IClubService _clubService;
    private readonly UserManager<ApplicationUser> _userManager;

    public MembershipController(
        IMembershipService membershipService,
        IClubService clubService,
        UserManager<ApplicationUser> userManager)
    {
        _membershipService = membershipService;
        _clubService = clubService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Apply()
    {
        var userId = await RequireUserIdAsync();
        var allClubs = await _clubService.GetAllWithDetailsAsync();
        var activeClubs = allClubs.Where(c => c.IsActive).ToList();

        var userMemberships = userId != Guid.Empty
            ? await _membershipService.GetMembershipsByUserIdAsync(userId)
            : Enumerable.Empty<ClubMembership>();

        var viewModel = new MembershipApplyViewModel
        {
            ActiveClubs = activeClubs.Select(club =>
            {
                var membership = userMemberships.FirstOrDefault(m => m.ClubId == club.Id);
                var canApply = membership == null || membership.Status == MembershipStatus.Cancelled;
                var canCancel = membership != null && membership.Status == MembershipStatus.Pending;
                string? statusMessage = null;

                if (membership != null)
                {
                    statusMessage = membership.Status switch
                    {
                        MembershipStatus.Pending   => "Bekleyen başvurunuz var.",
                        MembershipStatus.Approved  => "Bu topluluğa zaten üyesiniz.",
                        MembershipStatus.Rejected  => $"Başvurunuz reddedildi. {(string.IsNullOrWhiteSpace(membership.Note) ? string.Empty : $"Gerekçe: {membership.Note}")}",
                        MembershipStatus.Cancelled => "Başvurunuz iptal edildi. Tekrar başvurabilirsiniz.",
                        _ => null
                    };
                }

                return new ClubItemViewModel
                {
                    Id = club.Id,
                    Name = club.Name,
                    Description = club.Description,
                    AdvisorName = club.AdvisorName,
                    PresidentName = club.President?.FullName ?? club.President?.Email ?? "Bilinmiyor",
                    CanApply = canApply,
                    StatusMessage = statusMessage,
                    CanCancel = canCancel
                };
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(Guid clubId)
    {
        if (clubId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Geçersiz topluluk seçimi.";
            return RedirectToAction(nameof(Apply));
        }

        var userId = await GetUserIdAsync();
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Kullanıcı oturumu bulunamadı.");
        }

        try
        {
            await _membershipService.RequestMembershipAsync(clubId, userId);
            TempData["SuccessMessage"] = "Üyelik başvurunuz başarıyla gönderildi. Onay bekleniyor.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Apply));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid clubId)
    {
        if (clubId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Geçersiz topluluk seçimi.";
            return RedirectToAction(nameof(Apply));
        }

        var userId = await RequireUserIdAsync();

        try
        {
            var cancelled = await _membershipService.CancelMembershipRequestAsync(clubId, userId);
            if (cancelled)
            {
                TempData["SuccessMessage"] = "Üyelik başvurunuz başarıyla iptal edildi.";
            }
            else
            {
                TempData["ErrorMessage"] = "İptal edilecek bekleyen bir başvuru bulunamadı.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Apply));
    }

    private async Task<Guid> GetUserIdAsync()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                return user.Id;
            }
        }

        return Guid.Empty;
    }

    private async Task<Guid> RequireUserIdAsync()
    {
        var userId = await GetUserIdAsync();
        if (userId != Guid.Empty)
        {
            return userId;
        }

        throw new UnauthorizedAccessException("Kullanıcı oturumu bulunamadı.");
    }
}

