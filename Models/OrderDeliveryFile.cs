
namespace TranslationOffice.Models;

public class OrderDeliveryFile
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } = null!;

    // مترجمی که فایل را بارگذاری کرده است
    public string TranslatorId { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    // نام تصادفی فایل در سرور
    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.Now;
}
