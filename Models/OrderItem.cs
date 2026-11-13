namespace TranslationOffice.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int DocumentTypeId { get; set; }
    public DocumentType DocumentType { get; set; } = null!;

    public int Quantity { get; set; } = 1;
    public int Units { get; set; }
    public bool NeedJudiciary { get; set; }
    public bool NeedForeignAffairs { get; set; }
    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = "Pending";
    public string? TranslatorId { get; set; }

    // Snapshot مالی هنگام تکمیل ترجمه؛ تغییر نرخ مترجم سوابق قبلی را عوض نمی‌کند.
    public decimal? TranslationFeeToman { get; set; }
    public decimal? TranslatorCommissionPercentSnapshot { get; set; }
    public decimal? TranslatorEarningToman { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<OrderDocument> Documents { get; set; } = new();
    public List<OrderDeliveryFile> DeliveryFiles { get; set; } = new();
}