namespace TranslationOffice.Models;

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2
}

public class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // مبلغ همیشه به تومان
    public decimal AmountToman { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string Gateway { get; set; } = "Zarinpal";

    // شناسه‌ای که درگاه برای تراکنش برمی‌گرداند
    public string? Authority { get; set; }

    // شماره پیگیری پرداخت موفق
    public string? ReferenceId { get; set; }

    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? VerifiedAt { get; set; }

    // اطلاعات رسید پرداخت کارت‌به‌کارت
    public string? ReceiptOriginalFileName { get; set; }
    public string? ReceiptStoredFileName { get; set; }
    public string? ReceiptContentType { get; set; }
    public long? ReceiptFileSize { get; set; }
    public string? PayerName { get; set; }
    public string? TransferReference { get; set; }

    // اطلاعات بررسی رسید توسط مدیر
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByUserId { get; set; }
}