using System.ComponentModel.DataAnnotations;
using Topluluk_Yonetim.MVC.Models.Enums;

namespace Topluluk_Yonetim.MVC.Models.ViewModels;

public class ClubApplicationViewModel
{
    [Required(ErrorMessage = "Topluluk adı zorunludur.")]
    [StringLength(150, ErrorMessage = "Topluluk adı en fazla 150 karakter olabilir.")]
    [Display(Name = "Topluluk Adı")]
    public string ClubName { get; set; } = null!;

    [Required(ErrorMessage = "Misyon zorunludur.")]
    [StringLength(500, ErrorMessage = "Misyon en fazla 500 karakter olabilir.")]
    [Display(Name = "Misyon")]
    public string Mission { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Vizyon en fazla 500 karakter olabilir.")]
    [Display(Name = "Vizyon")]
    public string? Vision { get; set; }

    [StringLength(1000, ErrorMessage = "Planlanan aktiviteler en fazla 1000 karakter olabilir.")]
    [Display(Name = "Planlanan Aktiviteler")]
    public string? PlannedActivities { get; set; }

    [Required(ErrorMessage = "Tahmini üye sayısı zorunludur.")]
    [Range(5, 500, ErrorMessage = "Tahmini üye sayısı 5 ile 500 arasında olmalıdır.")]
    [Display(Name = "Tahmini Üye Sayısı")]
    public int EstimatedMemberCount { get; set; }

    [StringLength(200, ErrorMessage = "Danışman adı en fazla 200 karakter olabilir.")]
    [Display(Name = "Danışman Adı")]
    public string? AdvisorName { get; set; }

    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200, ErrorMessage = "E-posta adresi en fazla 200 karakter olabilir.")]
    [Display(Name = "Danışman E-posta")]
    public string? AdvisorEmail { get; set; }

    [StringLength(1000, ErrorMessage = "Kaynak ihtiyaçları en fazla 1000 karakter olabilir.")]
    [Display(Name = "Kaynak İhtiyaçları")]
    public string? ResourceNeeds { get; set; }
}

