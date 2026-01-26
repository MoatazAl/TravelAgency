using Microsoft.EntityFrameworkCore;
using TravelAgency.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using TravelAgency.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------
// 1. Add Database (EF Core + SQLite)
// ---------------------------------------
builder.Services.AddDbContext<TravelAgencyContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------
// 2. Add Identity (Users + Roles)
// ---------------------------------------
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<TravelAgencyContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IEmailSender, ConsoleEmailSender>();

// Allow Login Redirection
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ← Require HTTPS for cookies
});

// ---------------------------------------
// ✅ HTTPS ENFORCEMENT
// ---------------------------------------
builder.Services.AddHsts(options =>
{
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// MVC Controllers + Views + Services
builder.Services.AddControllersWithViews();
builder.Services.AddHostedService<PendingBookingCleanupService>();

var app = builder.Build();

// ---------------------------------------
// SEED ADMIN USER + ROLE
// ---------------------------------------
await SeedAdminAsync(app);

// ---------------------------------------
// 3. Middleware Pipeline
// ---------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // ← Enable HSTS in production
}

// ✅ FORCE HTTPS REDIRECTION
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TravelAgencyContext>();
    DbInitializer.Initialize(context);
}

app.Run();

// ---------------------------------------
// SEEDING METHOD
// ---------------------------------------
static async Task SeedAdminAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // 1) Ensure Admin Role Exists
    const string adminRole = "Admin";
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    // 2) Create Admin User
    string adminEmail = "admin@travel.com";
    string adminPassword = "Admin123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);

        if (!createResult.Succeeded)
        {
            throw new Exception("Admin user creation failed: " +
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }
    }

    // 3) Assign Role to Admin
    if (!await userManager.IsInRoleAsync(adminUser, adminRole))
    {
        await userManager.AddToRoleAsync(adminUser, adminRole);
    }
}