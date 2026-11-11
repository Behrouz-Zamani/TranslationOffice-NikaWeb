
using Microsoft.AspNetCore.Identity;
using TranslationOffice.Models;

namespace TranslationOffice.Data;

public static class AdminSeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var phone = configuration["AdminAccount:Phone"];
        var password = configuration["AdminAccount:Password"];

        if (string.IsNullOrWhiteSpace(phone) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "اطلاعات حساب مدیر در تنظیمات برنامه تعریف نشده است.");
        }

        var admin = await userManager.FindByNameAsync(phone);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = phone,
                PhoneNumber = phone,
                PhoneNumberConfirmed = true,
                FullName = "مدیر سیستم",
                Role = "Admin"
            };

            var result = await userManager.CreateAsync(admin, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    " | ",
                    result.Errors.Select(x => x.Description));

                throw new InvalidOperationException(
                    $"ساخت حساب مدیر ناموفق بود: {errors}");
            }
        }
        else if (admin.Role != "Admin")
        {
            admin.Role = "Admin";

            var result = await userManager.UpdateAsync(admin);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    " | ",
                    result.Errors.Select(x => x.Description));

                throw new InvalidOperationException(
                    $"ارتقای حساب به مدیر ناموفق بود: {errors}");
            }
        }
    }
}
