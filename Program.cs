using GymManagementSystem.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// DATABASE CONNECTION
// =========================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// =========================================================
// MVC SERVICES
// =========================================================
builder.Services.AddControllersWithViews();

// =========================================================
// AUTHENTICATION / ROLES (cookie-based)
// =========================================================
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;
    });

// Session-ka loo isticmaalo gaadhiga (cart) ee martida
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// =========================================================
// MIDDLEWARE PIPELINE
// =========================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// =========================================================
// DEFAULT ROUTE
// =========================================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Guest}/{action=HomePage}/{id?}"
);

// Tables-ka waxay si automatic ah u samaysmayaan, iyo akoonnada default-ka
AccountSeedData.SeedAccounts(app.Services);

app.Run();