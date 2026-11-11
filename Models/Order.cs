
namespace TranslationOffice.Models;

public class Order
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    // برای سفارش مهمان null است.
    public string? UserId { get; set; }

    // هش توکن دسترسی سفارش مهمان؛ خود توکن ذخیره نمی‌شود.
    public string? GuestAccessTokenHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // وضعیت پرداخت سفارش:
    // PendingPayment, Paid, Failed, Refunded, Cancelled
    public string Status { get; set; } = "PendingPayment";

    // وضعیت پیشرفت انجام سفارش:
    // Pending, InProgress, Completed, Cancelled
    public string ProgressStatus { get; set; } = "Pending";

    // تمام مبالغ سیستم به تومان هستند.
    public decimal TotalPrice { get; set; }

    public string? Notes { get; set; }

    public List<OrderItem> Items { get; set; } = new();

    public List<Payment> Payments { get; set; } = new();

    public List<OrderDocument> Documents { get; set; } = new();
}
