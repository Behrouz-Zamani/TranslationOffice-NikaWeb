
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranslationOffice.Data;
using TranslationOffice.Services;

namespace TranslationOffice.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly OrderDocumentStorageService _storage;

    public DocumentsController(
        ApplicationDbContext db,
        OrderDocumentStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet("{documentId:int}/download")]
    public async Task<IActionResult> Download(
        int documentId,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var document = await _db.OrderDocuments
            .AsNoTracking()
            .Include(x => x.OrderItem)
                .ThenInclude(x => x!.Order)
            .FirstOrDefaultAsync(
                x => x.Id == documentId,
                cancellationToken);

        if (document == null)
            return NotFound("فایل موردنظر پیدا نشد.");

        var currentUser = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken);

        if (currentUser == null)
            return Unauthorized();

        var isAdmin = string.Equals(
            currentUser.Role,
            "Admin",
            StringComparison.OrdinalIgnoreCase);

        var isCustomer = string.Equals(
            currentUser.Role,
            "Customer",
            StringComparison.OrdinalIgnoreCase);

        var isTranslator = string.Equals(
            currentUser.Role,
            "Translator",
            StringComparison.OrdinalIgnoreCase);

        // مالک سفارش فقط مدارک سفارش خودش را می‌تواند دانلود کند.
        var isOrderOwner =
            isCustomer &&
            document.OrderItem?.Order != null &&
            document.OrderItem.Order.UserId == userId;

        // مترجم فقط مدارک آیتم‌هایی را می‌تواند دانلود کند
        // که به خودش تخصیص داده شده‌اند.
        var isAssignedTranslator =
            isTranslator &&
            document.OrderItem != null &&
            document.OrderItem.TranslatorId == userId;

        // مدیر، مالک سفارش یا مترجم مسئول مجاز هستند.
        if (!isAdmin && !isOrderOwner && !isAssignedTranslator)
            return Forbid();

        var fileBytes = await _storage.ReadAsync(
            document.StoredFileName,
            cancellationToken);

        if (fileBytes == null)
            return NotFound("فایل در فضای ذخیره‌سازی پیدا نشد.");

        var fileName = Path.GetFileName(document.OriginalFileName);

        var contentType = string.IsNullOrWhiteSpace(document.ContentType)
            ? "application/octet-stream"
            : document.ContentType;

        return File(
            fileBytes,
            contentType,
            fileName);
    }
}
