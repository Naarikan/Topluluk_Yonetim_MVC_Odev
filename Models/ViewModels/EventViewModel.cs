using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Models.ViewModels;

public class EventListViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public decimal EstimatedBudget { get; set; }
    public ApprovalStatus Status { get; set; }
    public string ClubName { get; set; } = null!;
    public Guid ClubId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EventCreateViewModel
{
    public Guid ClubId { get; set; }

    [Required(ErrorMessage = "Etkinlik başlığı zorunludur.")]
    [StringLength(150, ErrorMessage = "Başlık en fazla 150 karakter olabilir.")]
    [Display(Name = "Etkinlik Başlığı")]
    public string Title { get; set; } = null!;

    [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Etkinlik tarihi zorunludur.")]
    [Display(Name = "Etkinlik Tarihi")]
    [DataType(DataType.DateTime)]
    public DateTime EventDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Tahmini bütçe zorunludur.")]
    [Range(0, double.MaxValue, ErrorMessage = "Bütçe 0'dan büyük olmalıdır.")]
    [Display(Name = "Tahmini Bütçe")]
    [DataType(DataType.Currency)]
    public decimal EstimatedBudget { get; set; }
}

public class ClubOptionViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

public class EventIndexViewModel
{
    public PagedList<EventListViewModel> Events { get; set; } = new(new List<EventListViewModel>(), 0, 1, 10);
    public EventListFilterViewModel Filter { get; set; } = new();
    public int TotalCount { get; set; }
}

