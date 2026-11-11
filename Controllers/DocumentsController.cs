
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

        // دریافت فایل و اطلاعات مدرک از دیتابیس
        var document = await _db.OrderDocuments
            .AsNoTracking()
            .Include(x => x.OrderItem)
            .FirstOrDefaultAsync(
                x => x.Id == documentId,
                cancellationToken);

        if (document == null)
            return NotFound("فایل موردنظر پیدا نشد.");

        // بررسی نقش کاربر
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

        var isAssignedTranslator =
            string.Equals(
                currentUser.Role,
                "Translator",
                StringComparison.OrdinalIgnoreCase)
            &&
            document.OrderItem.TranslatorId == userId;

        // فقط مدیر یا مترجم مسئول مدرک اجازه دانلود دارد.
        if (!isAdmin && !isAssignedTranslator)
            return Forbid();

        // خواندن فایل از فضای ذخیره‌سازی خصوصی
        var fileBytes = await _storage.ReadAsync(
            document.StoredFileName,
            cancellationToken);

        if (fileBytes == null)
            return NotFound("فایل در فضای ذخیره‌سازی پیدا نشد.");

        var fileName = Path.GetFileName(document.OriginalFileName);

        var contentType = string.IsNullOrWhiteSpace(document.ContentType)
            ? "application/octet-stream"
            : document.ContentType;

        // ارسال فایل به صورت دانلود
        return File(
            fileBytes,
            contentType,
            fileName);
    }
}
