using Microsoft.AspNetCore.Hosting;

namespace TranslationOffice.Services;

/// <summary>
/// Stores public sample images used only for displaying document examples.
/// Customer order uploads are handled by OrderDocumentStorageService.
/// </summary>
public sealed class DocumentSampleImageStorageService
{
    public const long MaxFileSize = 3 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };

    private readonly string _storagePath;

    public DocumentSampleImageStorageService(IWebHostEnvironment environment)
    {
        var webRoot = environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");

        _storagePath = Path.Combine(webRoot, "uploads", "document-samples");
    }

    public async Task<string> SaveAsync(
        string originalFileName,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        if (content == null || content.Length == 0 || content.LongLength > MaxFileSize)
            throw new InvalidOperationException("حجم تصویر نمونه باید بیشتر از صفر و حداکثر ۳ مگابایت باشد.");

        var extension = Path.GetExtension(originalFileName);
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("فرمت تصویر نمونه باید JPG یا PNG باشد.");

        ValidateSignature(extension, content);
        Directory.CreateDirectory(_storagePath);

        var storedName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var fullPath = GetSafePath(storedName);

        await using (var output = new FileStream(
            fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            81920, useAsync: true))
        {
            await output.WriteAsync(content, cancellationToken);
            await output.FlushAsync(cancellationToken);
        }

        return $"/uploads/document-samples/{storedName}";
    }

    public Task DeleteAsync(string? publicPath)
    {
        if (string.IsNullOrWhiteSpace(publicPath))
            return Task.CompletedTask;

        const string prefix = "/uploads/document-samples/";
        if (!publicPath.StartsWith(prefix, StringComparison.Ordinal))
            return Task.CompletedTask;

        var fileName = publicPath[prefix.Length..];
        try
        {
            var fullPath = GetSafePath(fileName);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch (InvalidOperationException)
        {
            // Ignore invalid paths; never delete outside the sample-image folder.
        }

        return Task.CompletedTask;
    }

    private string GetSafePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileName(fileName))
            throw new InvalidOperationException("نام تصویر معتبر نیست.");

        var extension = Path.GetExtension(fileName);
        var id = Path.GetFileNameWithoutExtension(fileName);
        if (!AllowedExtensions.Contains(extension) || !Guid.TryParseExact(id, "N", out _))
            throw new InvalidOperationException("شناسه تصویر معتبر نیست.");

        var fullPath = Path.GetFullPath(Path.Combine(_storagePath, fileName));
        var root = Path.GetFullPath(_storagePath).TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("مسیر تصویر معتبر نیست.");

        return fullPath;
    }

    private static void ValidateSignature(string extension, byte[] bytes)
    {
        var valid = extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => bytes.Length >= 3 &&
                                 bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
            ".png" => bytes.Length >= 8 &&
                      bytes[0] == 0x89 && bytes[1] == 0x50 &&
                      bytes[2] == 0x4E && bytes[3] == 0x47 &&
                      bytes[4] == 0x0D && bytes[5] == 0x0A &&
                      bytes[6] == 0x1A && bytes[7] == 0x0A,
            _ => false
        };

        if (!valid)
            throw new InvalidOperationException("محتوای فایل با فرمت تصویر مطابقت ندارد.");
    }
}
