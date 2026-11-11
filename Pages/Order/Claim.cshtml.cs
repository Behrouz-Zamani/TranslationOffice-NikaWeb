using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TranslationOffice.Data;
using TranslationOffice.Models;

namespace TranslationOffice.Pages.Order;

[Authorize]
public class ClaimModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ClaimModel(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var order = await _db.Orders
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null)
            return NotFound();

        // سفارش پرداخت‌شده یا سفارش با وضعیت غیرمجاز
        // نباید از این مسیر تغییر مالکیت پیدا کند.
        if (order.Status != "PendingPayment")
            return Redirect($"/order/success/{id}");

        // اگر سفارش قبلاً به شخص دیگری متصل شده باشد،
        // اجازه تصاحب آن را نمی‌دهیم.
        if (order.UserId != null && order.UserId != userId)
            return Forbid();

        if (order.UserId == null)
        {
            order.UserId = userId;
            await _db.SaveChangesAsync();
        }

        return Redirect($"/order/success/{id}");
    }
}