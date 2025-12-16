using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Topluluk_Yonetim.MVC.Models.ViewModels;
using Topluluk_Yonetim.MVC.Services.Interfaces;

namespace Topluluk_Yonetim.MVC.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var result = await _authService.LoginAsync(
                model.Email, model.Password, model.RememberMe);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);
                
                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _authService.RegisterAsync(model);
            TempData["SuccessMessage"] = "Kayıt işlemi başarıyla tamamlandı. Giriş yapabilirsiniz.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user == null)
            return RedirectToAction(nameof(Login));

        var roles = await _authService.GetUserRolesAsync(user.Id);
        
        var viewModel = new ProfileViewModel
        {
            Id = user.Id,
            FullName = user.FullName ?? "",
            Email = user.Email ?? "",
            StudentNumber = user.StudentNumber,
            Roles = roles.ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction(nameof(Login));

            await _authService.UpdateProfileAsync(user.Id, model);
            TempData["SuccessMessage"] = "Profil başarıyla güncellendi.";
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var user = await _authService.GetCurrentUserAsync();
            if (user != null)
            {
                var roles = await _authService.GetUserRolesAsync(user.Id);
                model.Roles = roles.ToList();
            }
            return View(model);
        }
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction(nameof(Login));

            await _authService.ChangePasswordAsync(
                user.Id, model.CurrentPassword, model.NewPassword);
            
            TempData["SuccessMessage"] = "Şifre başarıyla değiştirildi.";
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}

