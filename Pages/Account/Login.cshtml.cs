using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TranslationOffice.Models;

namespace TranslationOffice.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    [Required(ErrorMessage = "شماره موبایل را وارد کنید.")]
    public string Phone { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "رمز عبور را وارد کنید.")]
    public string Password { get; set; } = "";

    [BindProperty]
    public bool RememberMe { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public void OnGet()
    {
        if (string.IsNullOrWhiteSpace(ReturnUrl))
            ReturnUrl = "/";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Phone = Phone.Trim();

        if (!ModelState.IsValid)
            return Page();

        var result = await _signInManager.PasswordSignInAsync(
            Phone,
            Password,
            RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByNameAsync(Phone);
            var role = user?.Role?.Trim();

            if (string.Equals(role, "Admin",
                StringComparison.OrdinalIgnoreCase))
            {
                return Redirect("/admin/orders");
            }

            if (string.Equals(role, "Translator",
                StringComparison.OrdinalIgnoreCase))
            {
                return Redirect("/translator/orders");
            }

            // مشتری باید به آدرسی برگردد که قبل از ورود درخواست کرده بود.
            // این موضوع برای اتصال سفارش مهمان به حساب کاربری ضروری است.
            if (!string.IsNullOrWhiteSpace(ReturnUrl) &&
                Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return Redirect("/");
        }

        ModelState.AddModelError(
            "",
            "شماره موبایل یا رمز عبور اشتباه است.");

        return Page();
    }
}