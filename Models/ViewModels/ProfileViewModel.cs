using System.ComponentModel.DataAnnotations;

namespace Topluluk_Yonetim.MVC.Models.ViewModels;

public class ProfileViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Ad Soyad zorunludur")]
    [StringLength(100, ErrorMessage = "Ad Soyad en fazla {1} karakter olabilir")]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = null!;

    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = null!;

    [StringLength(20, ErrorMessage = "Öğrenci numarası en fazla {1} karakter olabilir")]
    [Display(Name = "Öğrenci Numarası")]
    public string? StudentNumber { get; set; }

    public List<string> Roles { get; set; } = new();
}

