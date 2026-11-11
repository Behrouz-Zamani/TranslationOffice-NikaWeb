
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TranslationOffice.Data;
using TranslationOffice.Services;

namespace TranslationOffice.Pages.Account;

[Authorize]
public class ClaimOrderModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ClaimOrderModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public int OrderId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = "";

    public string OrderNumber { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        var order = await _db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == OrderId);

        if (order == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (order.UserId == userId)
            return Redirect($"/order/success/{order.Id}");

        if (order.UserId != null)
            return Forbid();

        if (!GuestOrderToken.Verify(Token, order.GuestAccessTokenHash))
            return NotFound();

        OrderNumber = order.OrderNumber;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var order = await _db.Orders
            .FirstOrDefaultAsync(x => x.Id == OrderId);

        if (order == null)
            return NotFound();

        // اگر قبلاً به همین حساب متصل شده باشد، عملیات تکراری مشکلی ندارد.
        if (order.UserId == userId)
            return Redirect($"/order/success/{order.Id}");

        // سفارش متعلق به شخص دیگری قابل انتقال نیست.
        if (order.UserId != null)
            return Forbid();

        if (!GuestOrderToken.Verify(Token, order.GuestAccessTokenHash))
            return NotFound();

        order.UserId = userId;

        // پس از اتصال، توکن مهمان دیگر لازم نیست.
        order.GuestAccessTokenHash = null;

        await _db.SaveChangesAsync();

        return Redirect($"/order/success/{order.Id}");
    }
}
