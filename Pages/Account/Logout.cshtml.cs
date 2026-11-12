
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using TranslationOffice.Models;

//namespace TranslationOffice.Pages.Account
//{
//    public class LogoutModel : PageModel
//    {
//        private readonly SignInManager<ApplicationUser> _signInManager;

//        public LogoutModel(SignInManager<ApplicationUser> signInManager)
//        {
//            _signInManager = signInManager;
//        }

//        // نمایش صفحه تأیید خروج
//        public IActionResult OnGet()
//        {
//            return Page();
//        }

//        // انجام خروج پس از تأیید کاربر
//        public async Task<IActionResult> OnPostAsync()
//        {
//            await _signInManager.SignOutAsync();

//            return Redirect("/");
//        }
//    }
//}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TranslationOffice.Models;

namespace TranslationOffice.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LogoutModel(
            SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _signInManager.SignOutAsync();

            return Redirect("/");
        }
    }
}
