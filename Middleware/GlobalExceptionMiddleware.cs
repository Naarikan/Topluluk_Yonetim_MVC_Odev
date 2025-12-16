using Microsoft.AspNetCore.Mvc;
using System.Net;
using Topluluk_Yonetim.MVC.Exceptions;

namespace Topluluk_Yonetim.MVC.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionMiddleware(RequestDelegate next, IHostEnvironment environment)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = GetStatusCodeAndMessage(exception);

            if (IsApiRequest(context))
            {
                var problem = new ProblemDetails
                {
                    Status = statusCode,
                    Title = GetTitle(statusCode),
                    Detail = ShouldExposeDetails(statusCode) ? message : "Beklenmeyen bir hata oluştu.",
                    Instance = context.Request.Path
                };

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(problem);
            }
            else
            {
                var encodedMessage = ShouldExposeDetails(statusCode)
                    ? WebUtility.UrlEncode(message)
                    : string.Empty;

                var redirectUrl = string.IsNullOrWhiteSpace(encodedMessage)
                    ? $"/Error?code={statusCode}"
                    : $"/Error?code={statusCode}&message={encodedMessage}";

                context.Response.Redirect(redirectUrl);
            }
        }

        private static (int statusCode, string message) GetStatusCodeAndMessage(Exception exception)
        {
            return exception switch
            {
                ValidationException => ((int)HttpStatusCode.UnprocessableEntity, exception.Message),
                BusinessRuleException => ((int)HttpStatusCode.BadRequest, exception.Message),
                NotFoundException => ((int)HttpStatusCode.NotFound, exception.Message),
                UnauthorizedException => ((int)HttpStatusCode.Unauthorized, exception.Message),
                _ => ((int)HttpStatusCode.InternalServerError, exception.Message)
            };
        }

        private static string GetTitle(int statusCode) =>
            statusCode switch
            {
                (int)HttpStatusCode.BadRequest => "Geçersiz İstek",
                (int)HttpStatusCode.NotFound => "Bulunamadı",
                (int)HttpStatusCode.Unauthorized => "Yetkisiz Erişim",
                (int)HttpStatusCode.UnprocessableEntity => "Geçersiz Veri",
                _ => "Sunucu Hatası"
            };

        private bool ShouldExposeDetails(int statusCode) =>
            _environment.IsDevelopment() || statusCode is >= 400 and < 500;

        private static bool IsApiRequest(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                return true;
            }

            if (context.Request.Headers.TryGetValue("Accept", out var acceptHeader))
            {
                foreach (var header in acceptHeader)
                {
                    if (!string.IsNullOrWhiteSpace(header) &&
                        header.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

