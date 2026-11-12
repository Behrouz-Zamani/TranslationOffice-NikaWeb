
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TranslationOffice.Data;
using TranslationOffice.Models;
using TranslationOffice.Services;

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

    public async Task<IActionResult> OnGetAsync(
        int? id,
        int? orderId,
        string? token)
    {
        var currentUserId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(currentUserId))
            return Challenge();

        // پشتیبانی از هر دو نام پارامتر id و orderId
        var targetOrderId = orderId ?? id;

        if (!targetOrderId.HasValue || targetOrderId.Value <= 0)
            return BadRequest("شناسه سفارش معتبر نیست.");

        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("توکن دسترسی سفارش ارسال نشده است.");

        var order = await _db.Orders
            .FirstOrDefaultAsync(x => x.Id == targetOrderId.Value);

        if (order == null)
            return NotFound("سفارش موردنظر پیدا نشد.");

        // فقط سفارش‌های در انتظار پرداخت قابلیت اتصال دارند.
        if (order.Status != "PendingPayment")
            return Redirect($"/order/success/{order.Id}");

        // اعتبارسنجی توکن مهمان
        if (!GuestOrderToken.Verify(token, order.GuestAccessTokenHash))
            return Forbid();

        // سفارش متعلق به کاربر دیگری است.
        if (order.UserId != null &&
            order.UserId != currentUserId)
        {
            return Forbid();
        }

        // اگر هنوز مهمان است، به کاربر فعلی متصل شود.
        if (order.UserId == null)
        {
            order.UserId = currentUserId;

            await _db.SaveChangesAsync();
        }

        // پس از اتصال، توکن را در آدرس قرار نمی‌دهیم.
        return Redirect($"/order/success/{order.Id}?claimed=1");
    }
}
