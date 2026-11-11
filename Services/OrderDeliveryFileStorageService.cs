
using Microsoft.AspNetCore.Hosting;

namespace TranslationOffice.Services;

public class OrderDeliveryFileStorageService
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png"
        };

    private readonly string _storageRoot;

    public OrderDeliveryFileStorageService(IWebHostEnvironment environment)
    {
        _storageRoot = Path.Combine(
            environment.ContentRootPath,
            "App_Data",
            "OrderDeliveryFiles");

        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<string> SaveAsync(
        string originalFileName,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        if (content == null || content.Length == 0)
            throw new InvalidOperationException("فایل خالی است.");

        if (content.Length > MaxFileSize)
            throw new InvalidOperationException("حداکثر حجم فایل ۱۰ مگابایت است.");

        var extension = Path.GetExtension(originalFileName);

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("فرمت فایل مجاز نیست.");

        var storedFileName = Guid.NewGuid().ToString("N") + extension.ToLowerInvariant();

        var fullPath = GetSafePath(storedFileName);

        await File.WriteAllBytesAsync(
            fullPath,
            content,
            cancellationToken);

        return storedFileName;
    }

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

    public Task DeleteAsync(
        string storedFileName,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetSafePath(storedFileName);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    private string GetSafePath(string storedFileName)
    {
        if (string.IsNullOrWhiteSpace(storedFileName))
            throw new InvalidOperationException("نام فایل معتبر نیست.");

        if (storedFileName != Path.GetFileName(storedFileName))
            throw new InvalidOperationException("مسیر فایل معتبر نیست.");

        var extension = Path.GetExtension(storedFileName);

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("فرمت فایل معتبر نیست.");

        var nameWithoutExtension =
            Path.GetFileNameWithoutExtension(storedFileName);

        if (!Guid.TryParseExact(nameWithoutExtension, "N", out _))
            throw new InvalidOperationException("شناسه فایل معتبر نیست.");

        var fullPath = Path.GetFullPath(
            Path.Combine(_storageRoot, storedFileName));

        var rootPath = Path.GetFullPath(_storageRoot)
            .TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(
                rootPath,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("دسترسی به مسیر فایل مجاز نیست.");
        }

        return fullPath;
    }
}
