using Microsoft.AspNetCore.Hosting;
using TranslationOffice.Models;

namespace TranslationOffice.Services;

public class PaymentReceiptStorageService
{
    public const long MaxFileSize = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".jpg", ".jpeg", ".png"
        };

    private readonly string _storagePath;

    public PaymentReceiptStorageService(IWebHostEnvironment environment)
    {
        _storagePath = Path.Combine(
            environment.ContentRootPath,
            "App_Data",
            "PaymentReceipts");
    }

    public async Task<(string StoredFileName, string ContentType)> SaveAsync(
        string originalFileName,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        if (content == null || content.Length == 0)
            throw new InvalidOperationException("فایل رسید خالی است.");

        if (content.Length > MaxFileSize)
            throw new InvalidOperationException("حجم رسید نباید بیشتر از ۵ مگابایت باشد.");

        var extension = Path.GetExtension(originalFileName);
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("فرمت رسید باید PDF، JPG، JPEG یا PNG باشد.");

        ValidateFileSignature(extension, content);

        Directory.CreateDirectory(_storagePath);

        var storedFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var fullPath = GetSafePath(storedFileName);

        try
        {
            await using var stream = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);

            await stream.WriteAsync(content, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        catch
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            throw;
        }

        return (storedFileName, GetContentType(extension));
    }

    public async Task<byte[]?> ReadAsync(
        string storedFileName,
        CancellationToken cancellationToken = default)
    {
        var path = GetSafePath(storedFileName);
        if (!File.Exists(path))
            return null;

        return await File.ReadAllBytesAsync(path, cancellationToken);
    }

    public Task DeleteAsync(string? storedFileName)
    {
        if (string.IsNullOrWhiteSpace(storedFileName))
            return Task.CompletedTask;

        try
        {
            var path = GetSafePath(storedFileName);
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (InvalidOperationException)
        {
            // نام فایل نامعتبر است؛ از حذف خارج از مسیر مجاز جلوگیری می‌شود.
        }

        return Task.CompletedTask;
    }

    private string GetSafePath(string storedFileName)
    {
        if (string.IsNullOrWhiteSpace(storedFileName) ||
            storedFileName != Path.GetFileName(storedFileName))
            throw new InvalidOperationException("نام فایل رسید معتبر نیست.");

        var extension = Path.GetExtension(storedFileName);
        var fileName = Path.GetFileNameWithoutExtension(storedFileName);

        if (!AllowedExtensions.Contains(extension) ||
            !Guid.TryParseExact(fileName, "N", out _))
            throw new InvalidOperationException("شناسه فایل رسید معتبر نیست.");

        var fullPath = Path.GetFullPath(Path.Combine(_storagePath, storedFileName));
        var root = Path.GetFullPath(_storagePath)
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("مسیر فایل رسید معتبر نیست.");

        return fullPath;
    }

    private static void ValidateFileSignature(string extension, byte[] content)
    {
        var valid = extension.ToLowerInvariant() switch
        {
            ".pdf" => content.Length >= 5 &&
                      content[0] == 0x25 && content[1] == 0x50 &&
                      content[2] == 0x44 && content[3] == 0x46 && content[4] == 0x2D,

            ".jpg" or ".jpeg" => content.Length >= 3 &&
                                 content[0] == 0xFF && content[1] == 0xD8 &&
                                 content[2] == 0xFF,

            ".png" => content.Length >= 8 &&
                      content[0] == 0x89 && content[1] == 0x50 &&
                      content[2] == 0x4E && content[3] == 0x47 &&
                      content[4] == 0x0D && content[5] == 0x0A &&
                      content[6] == 0x1A && content[7] == 0x0A,

            _ => false
        };

        if (!valid)
            throw new InvalidOperationException("محتوای فایل با پسوند آن مطابقت ندارد.");
    }

    private static string GetContentType(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
}
