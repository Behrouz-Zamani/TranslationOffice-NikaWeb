
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TranslationOffice.Models;

namespace TranslationOffice.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    [Required(ErrorMessage = "نام را وارد کنید.")]
    public string FirstName { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "نام خانوادگی را وارد کنید.")]
    public string LastName { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "شماره موبایل را وارد کنید.")]
    public string Phone { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "رمز عبور را وارد کنید.")]
    [MinLength(6, ErrorMessage = "رمز عبور حداقل باید ۶ کاراکتر باشد.")]
    public string Password { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "تکرار رمز عبور را وارد کنید.")]
    [Compare(nameof(Password), ErrorMessage = "رمزهای عبور یکسان نیستند.")]
    public string ConfirmPassword { get; set; } = "";

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
        FirstName = FirstName.Trim();
        LastName = LastName.Trim();

        if (!Regex.IsMatch(Phone, @"^09\d{9}$"))
        {
            ModelState.AddModelError(
                nameof(Phone),
                "شماره موبایل باید با 09 شروع شده و ۱۱ رقم باشد.");
        }

        if (!ModelState.IsValid)
            return Page();

        var existingUser = await _userManager.FindByNameAsync(Phone);

        if (existingUser != null)
        {
            ModelState.AddModelError(
                nameof(Phone),
                "این شماره موبایل قبلاً ثبت شده است.");

            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Phone,
            PhoneNumber = Phone,
            FirstName = FirstName,
            LastName = LastName,
            FullName = $"{FirstName} {LastName}",
            Role = "Customer"
        };

        var result = await _userManager.CreateAsync(user, Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return Page();
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToLocal(ReturnUrl);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return Redirect("/");
    }
}
