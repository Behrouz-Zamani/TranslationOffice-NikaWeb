
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TranslationOffice.Components;
using TranslationOffice.Data;
using TranslationOffice.Models;
using TranslationOffice.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------
// Blazor
// ---------------------------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ---------------------------------------
// Razor Pages و API Controllers
// ---------------------------------------
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// ---------------------------------------
// builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")));
// ---------------------------------------
// Identity
// ---------------------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;

    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ---------------------------------------
// Authentication Cookie
// ---------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ---------------------------------------
// Application Services
// ---------------------------------------
builder.Services.AddScoped<PricingService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddHttpContextAccessor();

// Payment Services
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<IPaymentGateway, UnavailablePaymentGateway>();
builder.Services.AddScoped<PaymentReceiptStorageService>();
builder.Services.AddScoped<BankTransferPaymentService>();
builder.Services.AddScoped<IFileSecurityScanner, FileSecurityScanner>();
builder.Services.AddScoped<OrderDocumentStorageService>();
builder.Services.AddScoped<DocumentSampleImageStorageService>();
builder.Services.AddScoped<OrderDeliveryFileStorageService>();

// ---------------------------------------
// Blazor Authentication State
// ---------------------------------------
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();

    var configuration = scope.ServiceProvider
        .GetRequiredService<IConfiguration>();

    await AdminSeeder.SeedAsync(userManager, configuration);
}

// ---------------------------------------
// Middleware
// ---------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.Use(async (context, next) =>
{
    if (!context.Request.Cookies.TryGetValue("nika-cart-id", out var guestCartId) ||
        !Guid.TryParseExact(guestCartId, "N", out _))
    {
        guestCartId = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(
            "nika-cart-id",
            guestCartId,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                MaxAge = TimeSpan.FromDays(365)
            });
    }

    context.Items["NikaGuestCartKey"] = guestCartId;
    await next();
});

app.UseAuthentication();

app.UseAuthorization();

app.UseAntiforgery();

// ---------------------------------------
// Endpoints
// ---------------------------------------
app.MapRazorPages();

app.MapControllers();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
