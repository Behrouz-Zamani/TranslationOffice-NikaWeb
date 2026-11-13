
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranslationOffice.Data;
using TranslationOffice.Services;

namespace TranslationOffice.Controllers;

[ApiController]
[Route("api/delivery-files")]
[Authorize]
public class DeliveryFilesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly OrderDeliveryFileStorageService _storage;
    private readonly IFileSecurityScanner _scanner;

    public DeliveryFilesController(
        ApplicationDbContext db,
        OrderDeliveryFileStorageService storage,
        IFileSecurityScanner scanner)
    {
        _db = db;
        _storage = storage;
        _scanner = scanner;
    }

    [HttpGet("{fileId:int}/download")]
    public async Task<IActionResult> Download(
        int fileId,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var file = await _db.OrderDeliveryFiles
            .AsNoTracking()
            .Include(x => x.Order)
            .Include(x => x.OrderItem)
            .FirstOrDefaultAsync(
                x => x.Id == fileId,
                cancellationToken);

        if (file == null)
            return NotFound("فایل تحویلی پیدا نشد.");

        var currentUser = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken);

        if (currentUser == null)
            return Unauthorized();

        var isAdmin = currentUser.Role == "Admin";

        var isCustomer =
            currentUser.Role == "Customer" &&
            file.Order.UserId == userId;

        var isAssignedTranslator =
            currentUser.Role == "Translator" &&
            file.OrderItem.TranslatorId == userId &&
            file.TranslatorId == userId;

        if (!isAdmin && !isCustomer && !isAssignedTranslator)
            return Forbid();

        var fileBytes = await _storage.ReadAsync(
            file.StoredFileName,
            cancellationToken);

        if (fileBytes == null)
            return NotFound("فایل در فضای ذخیره‌سازی پیدا نشد.");

        var scan = await _scanner.ScanAsync(file.OriginalFileName, fileBytes, cancellationToken);
        if (!scan.IsClean)
            return UnprocessableEntity("این فایل به دلیل محتوای مخرب قابل دانلود نیست.");

        var fileName = Path.GetFileName(file.OriginalFileName);

        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType;

        return File(
            fileBytes,
            contentType,
            fileName);
    }
}
