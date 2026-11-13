
using Microsoft.AspNetCore.Hosting;
using TranslationOffice.Models;

namespace TranslationOffice.Services;

public class OrderDocumentStorageService
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx"
        };

    private readonly string _storagePath;
    private readonly IFileSecurityScanner _scanner;

    public OrderDocumentStorageService(IWebHostEnvironment environment, IFileSecurityScanner scanner)
    {
        _scanner = scanner;
        _storagePath = Path.Combine(
            environment.ContentRootPath,
            "App_Data",
            "OrderDocuments");
    }

    public async Task<OrderDocument> SaveAsync(
        string originalFileName,
        byte[] content,
        int orderId,
        int orderItemId,
        CancellationToken cancellationToken = default)
    {
        if (content == null || content.Length == 0)
            throw new InvalidOperationException("فایل خالی است.");

        if (content.Length > MaxFileSize)
            throw new InvalidOperationException("حجم فایل بیشتر از ۱۰ مگابایت است.");

        var extension = Path.GetExtension(originalFileName);

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("فرمت فایل مجاز نیست.");

        var scan = await _scanner.ScanAsync(originalFileName, content, cancellationToken);
        if (!scan.IsClean)
            throw new InvalidOperationException(scan.Message);

        Directory.CreateDirectory(_storagePath);

        var storedFileName =
            $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        var fullPath = GetSafePath(storedFileName);

        try
        {
            await using var destination = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);

            await destination.WriteAsync(content, cancellationToken);
            await destination.FlushAsync(cancellationToken);
        }
        catch
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            throw;
        }

        return new OrderDocument
        {
            OrderId = orderId,
            OrderItemId = orderItemId,
            OriginalFileName = Path.GetFileName(originalFileName),
            StoredFileName = storedFileName,
            ContentType = GetContentType(extension),
            FileSize = content.Length,
            UploadedAt = DateTime.Now,
            ReviewStatus = "Pending"
        };
    }

    // خواندن امن فایل برای دانلود
    public async Task<byte[]?> ReadAsync(
        string storedFileName,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetSafePath(storedFileName);

        if (!File.Exists(fullPath))
            return null;

        return await File.ReadAllBytesAsync(
            fullPath,
            cancellationToken);
    }

    public Task DeleteAsync(string storedFileName)
    {
        string fullPath;

        try
        {
            fullPath = GetSafePath(storedFileName);
        }
        catch (InvalidOperationException)
        {
            return Task.CompletedTask;
        }

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    // جلوگیری از Path Traversal و نام فایل غیرمعتبر
    private string GetSafePath(string storedFileName)
    {
        if (string.IsNullOrWhiteSpace(storedFileName))
            throw new InvalidOperationException("نام فایل معتبر نیست.");

        if (storedFileName != Path.GetFileName(storedFileName))
            throw new InvalidOperationException("مسیر فایل معتبر نیست.");

        var extension = Path.GetExtension(storedFileName);

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("فرمت فایل معتبر نیست.");

        var fileNameWithoutExtension =
            Path.GetFileNameWithoutExtension(storedFileName);

        if (!Guid.TryParseExact(fileNameWithoutExtension, "N", out _))
            throw new InvalidOperationException("شناسه فایل معتبر نیست.");

        var fullPath = Path.GetFullPath(
            Path.Combine(_storagePath, storedFileName));

        var storageRoot = Path.GetFullPath(_storagePath)
            .TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(
                storageRoot,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("دسترسی به مسیر فایل مجاز نیست.");
        }

        return fullPath;
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }
}
