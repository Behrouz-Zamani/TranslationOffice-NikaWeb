using System.Buffers.Binary;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

namespace TranslationOffice.Services;

public sealed record FileScanResult(bool IsClean, string Message);

public interface IFileSecurityScanner
{
    Task<FileScanResult> ScanAsync(string fileName, byte[] content, CancellationToken cancellationToken = default);
}

/// <summary>
/// Validates file signatures and scans the complete payload with ClamAV INSTREAM.
/// Fail-closed: upload/download is rejected when ClamAV is unavailable.
/// </summary>
public sealed class FileSecurityScanner : IFileSecurityScanner
{
    private readonly IConfiguration _configuration;

    public FileSecurityScanner(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<FileScanResult> ScanAsync(
        string fileName,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        if (content is not { Length: > 0 })
            return new(false, "فایل خالی یا نامعتبر است.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!HasValidSignature(extension, content))
            return new(false, "ساختار یا امضای فایل با پسوند آن مطابقت ندارد.");

        var host = _configuration["ClamAV:Host"];
        var portValue = _configuration["ClamAV:Port"];
        if (string.IsNullOrWhiteSpace(host) ||
            !int.TryParse(portValue, out var port) ||
            port is < 1 or > 65535)
        {
            return new(false, "سرویس ضدبدافزار پیکربندی نشده است؛ فایل برای حفظ امنیت پذیرفته نشد.");
        }

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(20));

            using var client = new TcpClient();
            await client.ConnectAsync(host, port, timeout.Token);
            await using var stream = client.GetStream();

            var command = Encoding.ASCII.GetBytes("zINSTREAM\0");
            await stream.WriteAsync(command, timeout.Token);

            const int chunkSize = 64 * 1024;
            for (var offset = 0; offset < content.Length; offset += chunkSize)
            {
                var count = Math.Min(chunkSize, content.Length - offset);
                var length = new byte[4];
                BinaryPrimitives.WriteInt32BigEndian(length, count);
                await stream.WriteAsync(length, timeout.Token);
                await stream.WriteAsync(content.AsMemory(offset, count), timeout.Token);
            }

            await stream.WriteAsync(new byte[4], timeout.Token);
            await stream.FlushAsync(timeout.Token);

            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            var response = await reader.ReadLineAsync(timeout.Token);

            if (response == null)
                return new(false, "پاسخی از سرویس ضدبدافزار دریافت نشد.");

            if (response.EndsWith(" OK", StringComparison.Ordinal))
                return new(true, "فایل سالم است.");

            if (response.Contains("FOUND", StringComparison.OrdinalIgnoreCase))
                return new(false, "محتوای مخرب در فایل شناسایی شد.");

            return new(false, "اسکن فایل با خطا مواجه شد؛ فایل پذیرفته نشد.");
        }
        catch (OperationCanceledException)
        {
            return new(false, "زمان بررسی امنیتی فایل به پایان رسید؛ فایل پذیرفته نشد.");
        }
        catch
        {
            return new(false, "سرویس ضدبدافزار در دسترس نیست؛ فایل برای حفظ امنیت پذیرفته نشد.");
        }
    }

    private static bool HasValidSignature(string extension, byte[] bytes)
    {
        return extension switch
        {
            ".pdf" => bytes.Length >= 5 && Encoding.ASCII.GetString(bytes, 0, 5) == "%PDF-",
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