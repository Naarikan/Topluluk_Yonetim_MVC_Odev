using System.ComponentModel.DataAnnotations;

namespace Topluluk_Yonetim.MVC.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "E-posta adresi zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    [RegularExpression(@"^[^@]+@ogr\.selcuk\.edu\.tr$", ErrorMessage = "E-posta adresi @ogr.selcuk.edu.tr ile bitmelidir")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Şifre zorunludur")]
    [StringLength(100, ErrorMessage = "Şifre en az {2} karakter olmalıdır", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Şifre tekrarı zorunludur")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre Tekrar")]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
    public string ConfirmPassword { get; set; } = null!;

    [Required(ErrorMessage = "Ad Soyad zorunludur")]
    [StringLength(100, ErrorMessage = "Ad Soyad en fazla {1} karakter olabilir")]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = null!;

    [StringLength(20, ErrorMessage = "Öğrenci numarası en fazla {1} karakter olabilir")]
    [Display(Name = "Öğrenci Numarası")]
    public string? StudentNumber { get; set; }
}

