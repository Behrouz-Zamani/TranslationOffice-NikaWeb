
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TranslationOffice.Models;

namespace TranslationOffice.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
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
