namespace TranslationOffice.Models;

public class OrderDocument
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // فایل می‌تواند به یک مدرک مشخص از سفارش متصل باشد
    public int? OrderItemId { get; set; }
    public OrderItem? OrderItem { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    // نام تصادفی فایل ذخیره‌شده در سرور
    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.Now;

    // Pending، Approved، NeedsCorrection
    public string ReviewStatus { get; set; } = "Pending";

    public string? ReviewNote { get; set; }
}