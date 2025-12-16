using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.ViewModels;

namespace Topluluk_Yonetim.MVC.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password, bool rememberMe);
    Task LogoutAsync();
    Task<AuthResult> RegisterAsync(RegisterViewModel model);
    
    Task<ApplicationUser?> GetCurrentUserAsync();
    Task<ApplicationUser?> GetUserByIdAsync(Guid userId);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
    
    Task<bool> AssignRoleAsync(Guid userId, string roleName);
    Task<bool> RemoveRoleAsync(Guid userId, string roleName);
    Task<IList<string>> GetUserRolesAsync(Guid userId);
    Task<bool> IsUserInRoleAsync(Guid userId, string roleName);
    Task<bool> IsCoordinatorAsync(Guid userId);
    Task<bool> IsAdminAsync(Guid userId);
    
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<bool> UpdateProfileAsync(Guid userId, ProfileViewModel model);
    
    Task<bool> CreateRoleAsync(string roleName);
    Task<bool> RoleExistsAsync(string roleName);
    Task<IEnumerable<string>> GetAllRolesAsync();
}

