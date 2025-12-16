using System;
using System.ComponentModel.DataAnnotations;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Models.ViewModels;

public class AnnouncementListViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public AnnouncementAudience Audience { get; set; }
    public Guid? ClubId { get; set; }
    public string? ClubName { get; set; }
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByName { get; set; }
    public bool IsRead { get; set; }
}

public class AnnouncementCreateViewModel
{
    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir.")]
    [Display(Name = "Başlık")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "İçerik zorunludur.")]
    [StringLength(2000, ErrorMessage = "İçerik en fazla 2000 karakter olabilir.")]
    [Display(Name = "İçerik")]
    public string Content { get; set; } = null!;

    [Display(Name = "Hedef Kitle")]
    public AnnouncementAudience Audience { get; set; } = AnnouncementAudience.AllStudents;

    [Display(Name = "Topluluk (Opsiyonel)")]
    public Guid? ClubId { get; set; }

    [Display(Name = "Sabitle")]
    public bool IsPinned { get; set; }
}

public class AnnouncementDetailsViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public AnnouncementAudience Audience { get; set; }
    public string AudienceText { get; set; } = null!;
    public Guid? ClubId { get; set; }
    public string? ClubName { get; set; }
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByName { get; set; }
    public bool IsRead { get; set; }
    public int ReadCount { get; set; }
}

public class AnnouncementIndexViewModel
{
    public PagedList<AnnouncementListViewModel> Announcements { get; set; } = new(new List<AnnouncementListViewModel>(), 0, 1, 10);
    public AnnouncementFilterViewModel Filter { get; set; } = new();
    public int TotalCount { get; set; }
}

public class AnnouncementEditViewModel : AnnouncementCreateViewModel
{
    public Guid Id { get; set; }
}

