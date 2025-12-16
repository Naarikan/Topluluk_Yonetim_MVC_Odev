
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Topluluk_Yonetim.MVC.Data;
using Topluluk_Yonetim.MVC.Data.Repositories.Implementations;
using Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;
using Topluluk_Yonetim.MVC.Models.Entities;
using Topluluk_Yonetim.MVC.Services.Implementations;
using Topluluk_Yonetim.MVC.Services.Interfaces;
using Topluluk_Yonetim.MVC.Middleware;
using Topluluk_Yonetim.MVC.Logging;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Logging.AddConsole();
builder.Logging.AddDebug();

var configuredPath = builder.Configuration["Logging:FilePath"];
string logPath;

if (string.IsNullOrWhiteSpace(configuredPath))
{
    logPath = Path.Combine(builder.Environment.ContentRootPath, "Logs", "app-log.txt");
}
else if (Path.IsPathRooted(configuredPath))
{
    logPath = configuredPath;
}
else
{
    logPath = Path.Combine(builder.Environment.ContentRootPath, configuredPath);
}

builder.Logging.AddProvider(new FileLoggerProvider(logPath, LogLevel.Information));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireLowercase = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.Name = ".AspNetCore.Identity.Application";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS'de Secure, HTTP'de değil
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Path = "/";
    options.ReturnUrlParameter = "ReturnUrl";
});

builder.Services.AddHttpContextAccessor(); // AuthService için gerekli

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IClubRepository, ClubRepository>();
builder.Services.AddScoped<IClubApplicationRepository, ClubApplicationRepository>();
builder.Services.AddScoped<IMembershipRepository, MembershipRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventApprovalRepository, EventApprovalRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<IAnnouncementReadRepository, AnnouncementReadRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClubService, ClubService>();
builder.Services.AddScoped<IClubApplicationService, ClubApplicationService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IEventApprovalService, EventApprovalService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var roles = new[] { "Admin", "President", "Member" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }

    var adminEmail = "admin@admin.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Sistem Yöneticisi",
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(adminUser, "123456");
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    else
    {
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
