using System.IO.Compression;
using System.Text;

namespace TranslationOffice.Services;

public sealed record FileScanResult(bool IsClean, string Message);

public interface IFileSecurityScanner
{
    Task<FileScanResult> ScanAsync(string fileName, byte[] content, CancellationToken cancellationToken = default);
}

/// <summary>
/// Performs fast file-format/signature validation only.
/// No ClamAV, network calls, or malware scanning are performed.
/// </summary>
public sealed class FileSecurityScanner : IFileSecurityScanner
{
    public Task<FileScanResult> ScanAsync(
        string fileName,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (content is not { Length: > 0 })
            return Task.FromResult(new FileScanResult(false, "فایل خالی یا نامعتبر است."));

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var valid = HasValidSignature(extension, content);

        return Task.FromResult(valid
            ? new FileScanResult(true, "ساختار فایل معتبر است.")
            : new FileScanResult(false, "ساختار فایل با پسوند آن مطابقت ندارد."));
    }

    private static bool HasValidSignature(string extension, byte[] bytes)
    {
        return extension switch
        {
            ".pdf" => HasPdfHeader(bytes),
            ".jpg" or ".jpeg" => bytes.Length >= 3 &&
                bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
            ".png" => bytes.Length >= 8 &&
                bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".doc" => bytes.Length >= 8 &&
                bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }),
            ".docx" => IsValidDocx(bytes),
            _ => false
        };
    }

    private static bool HasPdfHeader(byte[] bytes)
    {
        // PDF specification permits the header to appear within the first 1024 bytes.
        // This also handles files with a small BOM/leading prefix added by some exporters.
        var limit = Math.Min(bytes.Length - 5, 1024);
        for (var i = 0; i <= limit; i++)
        {
            if (bytes[i] == (byte)'%' &&
                bytes[i + 1] == (byte)'P' &&
                bytes[i + 2] == (byte)'D' &&
                bytes[i + 3] == (byte)'F' &&
                bytes[i + 4] == (byte)'-')
                return true;
        }

        return false;
    }

    private static bool IsValidDocx(byte[] bytes)
    {
        if (bytes.Length < 4 || bytes[0] != 0x50 || bytes[1] != 0x4B)
            return false;

        try
        {
            using var memory = new MemoryStream(bytes, writable: false);
            using var archive = new ZipArchive(memory, ZipArchiveMode.Read);
            return archive.GetEntry("[Content_Types].xml") != null &&
                   archive.GetEntry("word/document.xml") != null;
        }
        catch
        {
            return false;
        }
    }
}