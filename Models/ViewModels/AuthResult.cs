using Topluluk_Yonetim.MVC.Models.Entities;

namespace Topluluk_Yonetim.MVC.Models.ViewModels;

public class AuthResult
{
    public bool Succeeded { get; set; }
    public ApplicationUser? User { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Errors { get; set; } = new();
}

