using System;
using System.Collections.Generic;
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
public class ApprovalController : Controller
{
    private readonly IClubApplicationService _clubApplicationService;
    private readonly IMembershipService _membershipService;
    private readonly IEventService _eventService;
    private readonly IEventApprovalService _eventApprovalService;
    private readonly IAnnouncementService _announcementService;
    private readonly IClubService _clubService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApprovalController(
        IClubApplicationService clubApplicationService,
        IMembershipService membershipService,
        IEventService eventService,
        IEventApprovalService eventApprovalService,
        IAnnouncementService announcementService,
        IClubService clubService,
        UserManager<ApplicationUser> userManager)
    {
        _clubApplicationService = clubApplicationService;
        _membershipService = membershipService;
        _eventService = eventService;
        _eventApprovalService = eventApprovalService;
        _announcementService = announcementService;
        _clubService = clubService;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index([FromQuery] ClubApprovalFilterViewModel filter)
    {
        filter ??= new();
        filter.PageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        filter.PageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var applications = await _clubApplicationService.GetAllWithDetailsAsync();

        var pending = new List<ClubApplicationApprovalItemViewModel>();
        var approved = new List<ClubApplicationApprovalItemViewModel>();
        var rejected = new List<ClubApplicationApprovalItemViewModel>();

        foreach (var application in applications)
        {
            var item = MapToViewModel(application);

            switch (application.Status)
            {
                case ApprovalStatus.Pending:
                    pending.Add(item);
                    break;
                case ApprovalStatus.Approved:
                    approved.Add(item);
                    break;
                case ApprovalStatus.Rejected:
                    rejected.Add(item);
                    break;
            }
        }

        var allItems = pending.Concat(approved).Concat(rejected).AsEnumerable();

        if (filter.Status.HasValue)
        {
            allItems = allItems.Where(x => x.Status == filter.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLowerInvariant();
            allItems = allItems.Where(x =>
                (!string.IsNullOrEmpty(x.ClubName) && x.ClubName.ToLowerInvariant().Contains(search)) ||
                (!string.IsNullOrEmpty(x.ApplicantName) && x.ApplicantName.ToLowerInvariant().Contains(search)) ||
                (!string.IsNullOrEmpty(x.ApplicantEmail) && x.ApplicantEmail.ToLowerInvariant().Contains(search)));
        }

        var paged = PagedList<ClubApplicationApprovalItemViewModel>.Create(
            allItems,
            filter.PageNumber,
            filter.PageSize);

        var viewModel = new ClubApprovalDashboardViewModel
        {
            PendingApplications = pending,
            ApprovedApplications = approved,
            RejectedApplications = rejected,
            PagedApplications = paged,
            Filter = filter,
            TotalCount = allItems.Count(),
            PendingCount = pending.Count,
            ApprovedCount = approved.Count,
            RejectedCount = rejected.Count
        };

        return View(viewModel);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Details(Guid id)
    {
        var application = await _clubApplicationService.GetByIdWithDetailsAsync(id);
        if (application == null)
            throw new NotFoundException("Topluluk başvurusu bulunamadı.");

        var viewModel = MapToViewModel(application);
        return View(viewModel);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,President")]
    public async Task<IActionResult> Member([FromQuery] MembershipApprovalFilterViewModel filter)
    {
        var isAdmin = User.IsInRole("Admin");
        var isPresident = User.IsInRole("President");

        filter ??= new();
        filter.PageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        filter.PageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var memberships = await _membershipService.GetAllMembershipsAsync();

        if (isPresident)
        {
            var userId = await RequireReviewerIdAsync();
            var presidentClubs = await _clubService.GetClubsByPresidentIdAsync(userId);
            var presidentClubIds = presidentClubs.Select(c => c.Id).ToHashSet();
            memberships = memberships.Where(m => presidentClubIds.Contains(m.ClubId));
        }

        var pending = new List<MembershipApprovalItemViewModel>();
        var approved = new List<MembershipApprovalItemViewModel>();
        var rejected = new List<MembershipApprovalItemViewModel>();

        foreach (var membership in memberships)
        {
            var item = MapMembershipToViewModel(membership);

            switch (membership.Status)
            {
                case MembershipStatus.Pending:
                    pending.Add(item);
                    break;
                case MembershipStatus.Approved:
                    approved.Add(item);
                    break;
                case MembershipStatus.Rejected:
                    rejected.Add(item);
                    break;
            }
        }

        var allItems = pending.Concat(approved).Concat(rejected).AsEnumerable();

        if (filter.Status.HasValue)
        {
            allItems = allItems.Where(x => x.Status == filter.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLowerInvariant();
            allItems = allItems.Where(x =>
                (!string.IsNullOrEmpty(x.ClubName) && x.ClubName.ToLowerInvariant().Contains(search)) ||
                (!string.IsNullOrEmpty(x.UserName) && x.UserName.ToLowerInvariant().Contains(search)) ||
                (!string.IsNullOrEmpty(x.UserEmail) && x.UserEmail.ToLowerInvariant().Contains(search)));
        }

        var paged = PagedList<MembershipApprovalItemViewModel>.Create(
            allItems,
            filter.PageNumber,
            filter.PageSize);

        var viewModel = new MembershipApprovalDashboardViewModel
        {
            PendingMemberships = pending,
            ApprovedMemberships = approved,
            RejectedMemberships = rejected,
            PagedMemberships = paged,
            Filter = filter,
            TotalCount = allItems.Count(),
            PendingCount = pending.Count,
            ApprovedCount = approved.Count,
            RejectedCount = rejected.Count
        };

        return View(viewModel);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,President")]
    public async Task<IActionResult> MemberDetails(Guid id)
    {
        var membership = await _membershipService.GetMembershipByIdAsync(id);
        if (membership == null)
            throw new NotFoundException("Üyelik başvurusu bulunamadı.");

        var viewModel = MapMembershipToViewModel(membership);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,President")]
    public async Task<IActionResult> ApproveMembership(Guid id, string? note)
    {
        var reviewerId = await RequireReviewerIdAsync();

        try
        {
            await _membershipService.ApproveMembershipAsync(id, reviewerId, note);
            TempData["SuccessMessage"] = "Üyelik başvurusu başarıyla onaylandı.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(MemberDetails), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,President")]
    public async Task<IActionResult> RejectMembership(Guid id, string? note)
    {
        var reviewerId = await RequireReviewerIdAsync();

        try
        {
            await _membershipService.RejectMembershipAsync(id, reviewerId, note);
            TempData["SuccessMessage"] = "Üyelik başvurusu reddedildi.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(MemberDetails), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid id, string? note)
    {
        var reviewerId = await RequireReviewerIdAsync();
        await _clubApplicationService.ApproveApplicationAsync(id, reviewerId, note);
        TempData["SuccessMessage"] = "Başvuru başarıyla onaylandı.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid id, string? note)
    {
        var reviewerId = await RequireReviewerIdAsync();
        await _clubApplicationService.RejectApplicationAsync(id, reviewerId, note);
        TempData["SuccessMessage"] = "Başvuru reddedildi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<Guid> RequireReviewerIdAsync()
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

    private static ClubApplicationApprovalItemViewModel MapToViewModel(ClubApplication application)
    {
        return new ClubApplicationApprovalItemViewModel
        {
            Id = application.Id,
            ClubName = application.ClubName,
            Mission = application.Mission,
            Vision = application.Vision,
            PlannedActivities = application.PlannedActivities,
            EstimatedMemberCount = application.EstimatedMemberCount,
            AdvisorName = application.AdvisorName,
            AdvisorEmail = application.AdvisorEmail,
            ResourceNeeds = application.ResourceNeeds,
            ApplicantName = application.ApplicantUser?.FullName ?? application.ApplicantUser?.Email ?? "Bilinmiyor",
            ApplicantEmail = application.ApplicantUser?.Email ?? "Bilinmiyor",
            CreatedAt = application.CreatedAt,
            Status = application.Status,
            ReviewerName = application.ReviewedBy?.FullName ?? application.ReviewedBy?.Email,
            ReviewedAt = application.ReviewedAt,
            CoordinatorNote = application.CoordinatorNote
        };
    }

    [HttpGet]
    [Authorize(Roles = "Admin,President")]
    public async Task<IActionResult> Event([FromQuery] EventApprovalFilterViewModel filter)
    {
        var isAdmin = User.IsInRole("Admin");
        var isPresident = User.IsInRole("President");

        filter ??= new();
        filter.PageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        filter.PageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var events = await _eventService.GetAllWithDetailsAsync();

        if (isPresident)
        {
            var userId = await RequireReviewerIdAsync();
            var presidentClubs = await _clubService.GetClubsByPresidentIdAsync(userId);
            var presidentClubIds = presidentClubs.Select(c => c.Id).ToHashSet();
            events = events.Where(e => presidentClubIds.Contains(e.ClubId));
        }

        var pending = new List<EventApprovalItemViewModel>();
        var approved = new List<EventApprovalItemViewModel>();
        var rejected = new List<EventApprovalItemViewModel>();

        foreach (var evt in events)
        {
            var item = MapEventToViewModel(evt);

            switch (evt.Status)
            {
                case ApprovalStatus.Pending:
                    pending.Add(item);
                    break;
                case ApprovalStatus.Approved:
                    approved.Add(item);
                    break;
                case ApprovalStatus.Rejected:
                    rejected.Add(item);
                    break;
            }
        }

        var allItems = pending.Concat(approved).Concat(rejected).AsEnumerable();

        if (filter.Status.HasValue)
        {
            allItems = allItems.Where(x => x.Status == filter.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLowerInvariant();
            allItems = allItems.Where(x =>
                (!string.IsNullOrEmpty(x.ClubName) && x.ClubName.ToLowerInvariant().Contains(search)) ||
                (!string.IsNullOrEmpty(x.Title) && x.Title.ToLowerInvariant().Contains(search)));
        }

        var paged = PagedList<EventApprovalItemViewModel>.Create(
            allItems,
            filter.PageNumber,
            filter.PageSize);

        var viewModel = new EventApprovalDashboardViewModel
        {
            PendingEvents = pending,
            ApprovedEvents = approved,
            RejectedEvents = rejected,
            PagedEvents = paged,
            Filter = filter,
            TotalCount = allItems.Count(),
            PendingCount = pending.Count,
            ApprovedCount = approved.Count,
            RejectedCount = rejected.Count
        };

        return View(viewModel);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,President")]
    public async Task<IActionResult> EventDetails(Guid id)
    {
        var evt = await _eventService.GetEventWithDetailsAsync(id);
        if (evt == null)
            throw new NotFoundException("Etkinlik bulunamadı.");

        var viewModel = MapEventToViewModel(evt);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,President")]
    public async Task<IActionResult> ApproveEvent(Guid id, string? note)
    {
        var reviewerId = await RequireReviewerIdAsync();

        try
        {
            var result = await _eventApprovalService.ApproveEventAsync(id, reviewerId, note);
            if (result)
            {
                TempData["SuccessMessage"] = "Etkinlik başarıyla onaylandı.";
            }
            else
            {
                TempData["ErrorMessage"] = "Etkinlik onaylanamadı.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(EventDetails), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,President")]
    public async Task<IActionResult> RejectEvent(Guid id, string? note)
    {
        var reviewerId = await RequireReviewerIdAsync();

        try
        {
            var result = await _eventApprovalService.RejectEventAsync(id, reviewerId, note);
            if (result)
            {
                TempData["SuccessMessage"] = "Etkinlik reddedildi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Etkinlik reddedilemedi.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(EventDetails), new { id });
    }

    private static MembershipApprovalItemViewModel MapMembershipToViewModel(ClubMembership membership)
    {
        return new MembershipApprovalItemViewModel
        {
            Id = membership.Id,
            ClubId = membership.ClubId,
            ClubName = membership.Club?.Name ?? "Bilinmiyor",
            UserId = membership.UserId,
            UserName = membership.User?.FullName ?? membership.User?.Email ?? "Bilinmiyor",
            UserEmail = membership.User?.Email ?? "Bilinmiyor",
            StudentNumber = membership.User?.StudentNumber,
            CreatedAt = membership.CreatedAt,
            Status = membership.Status,
            Role = membership.Role,
            ReviewerName = membership.ReviewedBy?.FullName ?? membership.ReviewedBy?.Email,
            ReviewedAt = membership.ReviewedAt,
            Note = membership.Note
        };
    }

    private static EventApprovalItemViewModel MapEventToViewModel(Event evt)
    {
        var latestApproval = evt.ApprovalHistory
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();

        return new EventApprovalItemViewModel
        {
            Id = evt.Id,
            Title = evt.Title,
            Description = evt.Description,
            EventDate = evt.EventDate,
            EstimatedBudget = evt.EstimatedBudget,
            ClubId = evt.ClubId,
            ClubName = evt.Club?.Name ?? "Bilinmiyor",
            CreatedByName = evt.CreatedById.HasValue ? null : null, // TODO: CreatedBy navigation property eklenebilir
            CreatedByEmail = null,
            CreatedAt = evt.CreatedAt,
            Status = evt.Status,
            ReviewerName = latestApproval?.Reviewer?.FullName ?? latestApproval?.Reviewer?.Email,
            ReviewedAt = latestApproval?.CreatedAt,
            Comment = latestApproval?.Comment
        };
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Announcement()
    {
        var announcements = await _announcementService.GetPendingAnnouncementsAsync();
        
        // Debug için
        var totalCount = announcements.Count();
        var viewModel = announcements.Select(a => new
        {
            Id = a.Id,
            Title = a.Title,
            Content = a.Content,
            ClubName = a.Club?.Name,
            Audience = a.Audience,
            CreatedAt = a.CreatedAt,
            Status = a.Status
        }).ToList();

        ViewBag.Announcements = viewModel;
        ViewBag.TotalCount = totalCount;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveAnnouncement(Guid id)
    {
        var reviewerId = await RequireReviewerIdAsync();

        try
        {
            var result = await _announcementService.ApproveAnnouncementAsync(id, reviewerId);
            if (result)
            {
                TempData["SuccessMessage"] = "Duyuru başarıyla onaylandı.";
            }
            else
            {
                TempData["ErrorMessage"] = "Duyuru onaylanamadı.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Announcement));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectAnnouncement(Guid id, string? note)
    {
        var reviewerId = await RequireReviewerIdAsync();

        try
        {
            var result = await _announcementService.RejectAnnouncementAsync(id, reviewerId, note);
            if (result)
            {
                TempData["SuccessMessage"] = "Duyuru reddedildi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Duyuru reddedilemedi.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Announcement));
    }
}

