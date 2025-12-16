using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Topluluk_Yonetim.MVC.Models;

namespace Topluluk_Yonetim.MVC.Controllers
{
    [AllowAnonymous] // Ana sayfa herkese açık
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int code = 500, string? message = null)
        {
            var defaultMessage = code switch
            {
                400 => "İsteğinizde bir sorun var. Lütfen girdilerinizi kontrol edin.",
                401 => "Bu işlemi yapmak için yetkiniz bulunmuyor.",
                404 => "Ulaşmaya çalıştığınız sayfa ya da kayıt bulunamadı.",
                422 => "Gönderdiğiniz bilgiler doğrulanamadı. Lütfen tekrar deneyin.",
                _ => "Bir şeyler ters gitti. Lütfen daha sonra tekrar deneyin."
            };

            var model = new ErrorViewModel
            {
                StatusCode = code,
                Message = string.IsNullOrWhiteSpace(message) ? defaultMessage : message,
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            return View(model);
        }
    }
}
