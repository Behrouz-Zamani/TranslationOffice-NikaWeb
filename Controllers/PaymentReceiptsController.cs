using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranslationOffice.Data;
using TranslationOffice.Services;

namespace TranslationOffice.Controllers;

[ApiController]
[Route("api/payment-receipts")]
[Authorize]
public class PaymentReceiptsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly PaymentReceiptStorageService _storage;

    public PaymentReceiptsController(
        ApplicationDbContext db,
        PaymentReceiptStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet("{paymentId:int}/download")]
    public async Task<IActionResult> Download(
        int paymentId,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var payment = await _db.Payments
            .AsNoTracking()
            .Include(x => x.Order)
            .FirstOrDefaultAsync(
                x => x.Id == paymentId,
                cancellationToken);

        if (payment == null ||
            string.IsNullOrWhiteSpace(payment.ReceiptStoredFileName))
            return NotFound();

        var isAdmin = await _db.Users
            .AnyAsync(
                x => x.Id == userId && x.Role == "Admin",
                cancellationToken);

        if (!isAdmin && payment.Order.UserId != userId)
            return Forbid();

        var content = await _storage.ReadAsync(
            payment.ReceiptStoredFileName,
            cancellationToken);

        if (content == null)
            return NotFound("فایل رسید در سرور پیدا نشد.");

        return File(
            content,
            payment.ReceiptContentType ?? "application/octet-stream",
            payment.ReceiptOriginalFileName ?? "payment-receipt");
    }
}
