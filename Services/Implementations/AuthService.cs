using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Topluluk_Yonetim.MVC.Exceptions;
using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Models.ViewModels;
using Topluluk_Yonetim.MVC.Services.Interfaces;

namespace Topluluk_Yonetim.MVC.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthResult> LoginAsync(string email, string password, bool rememberMe)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new ValidationException("E-posta ve şifre boş olamaz.");

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new UnauthorizedException("E-posta veya şifre hatalı.");

        if (!user.IsActive)
            throw new UnauthorizedException("Hesabınız pasif durumda.");

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!, password, rememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                throw new UnauthorizedException("Hesabınız kilitlenmiş. Lütfen daha sonra tekrar deneyin.");
            if (result.IsNotAllowed)
                throw new UnauthorizedException("Giriş yapmanıza izin verilmiyor.");
            
            throw new UnauthorizedException("E-posta veya şifre hatalı.");
        }

        return new AuthResult { Succeeded = true, User = user };
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<AuthResult> RegisterAsync(RegisterViewModel model)
    {
        if (model == null)
            throw new ValidationException("Kayıt bilgileri boş olamaz.");

        // Email validation: Sadece @ogr.selcuk.edu.tr uzantılı email'ler kabul edilir
        if (!model.Email.EndsWith("@ogr.selcuk.edu.tr", StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("Sadece Selçuk Üniversitesi öğrenci e-posta adresi (@ogr.selcuk.edu.tr) ile kayıt olabilirsiniz.");

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
            throw new BusinessRuleException("Bu e-posta adresi zaten kullanılıyor.");

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            StudentNumber = model.StudentNumber,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ValidationException($"Kayıt işlemi başarısız: {errors}");
        }

        if (await _roleManager.RoleExistsAsync("Member"))
        {
            await _userManager.AddToRoleAsync(user, "Member");
        }

        return new AuthResult { Succeeded = true, User = user };
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        return await _userManager.GetUserAsync(user);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return null;

        return await _userManager.FindByIdAsync(userId.ToString());
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<bool> AssignRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new NotFoundException("Kullanıcı bulunamadı.");

        if (!await _roleManager.RoleExistsAsync(roleName))
            throw new NotFoundException($"Rol '{roleName}' bulunamadı.");

        if (await _userManager.IsInRoleAsync(user, roleName))
            return true; // Zaten bu rolde

        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }

    public async Task<bool> RemoveRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new NotFoundException("Kullanıcı bulunamadı.");

        if (!await _userManager.IsInRoleAsync(user, roleName))
            return true; // Zaten bu rolde değil

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        return result.Succeeded;
    }

    public async Task<IList<string>> GetUserRolesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new List<string>();

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> IsUserInRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;

        return await _userManager.IsInRoleAsync(user, roleName);
    }

    public async Task<bool> IsCoordinatorAsync(Guid userId)
    {
        return await IsUserInRoleAsync(userId, "Coordinator");
    }

    public async Task<bool> IsAdminAsync(Guid userId)
    {
        return await IsUserInRoleAsync(userId, "Admin");
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new NotFoundException("Kullanıcı bulunamadı.");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ValidationException($"Şifre değiştirme başarısız: {errors}");
        }

        return true;
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, ProfileViewModel model)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new NotFoundException("Kullanıcı bulunamadı.");

        user.FullName = model.FullName;
        user.StudentNumber = model.StudentNumber;

        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ValidationException($"Profil güncelleme başarısız: {errors}");
        }

        return true;
    }

    public async Task<bool> CreateRoleAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ValidationException("Rol adı boş olamaz.");

        if (await _roleManager.RoleExistsAsync(roleName))
            return true; // Zaten var

        var role = new IdentityRole<Guid> { Name = roleName };
        var result = await _roleManager.CreateAsync(role);
        return result.Succeeded;
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await _roleManager.RoleExistsAsync(roleName);
    }

    public async Task<IEnumerable<string>> GetAllRolesAsync()
    {
        return _roleManager.Roles.Select(r => r.Name!).ToList();
    }
}

