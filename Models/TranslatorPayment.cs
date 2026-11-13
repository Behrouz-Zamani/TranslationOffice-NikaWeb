namespace TranslationOffice.Models;

public class TranslatorPayment
{
    public int Id { get; set; }
    public string TranslatorId { get; set; } = string.Empty;
    public ApplicationUser Translator { get; set; } = null!;
    public decimal AmountToman { get; set; }
    public DateTime PaidAt { get; set; }
    public string Method { get; set; } = "BankTransfer";
    public string? TrackingNumber { get; set; }
    public string? Description { get; set; }
    public string CreatedByAdminId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsVoided { get; set; }
    public DateTime? VoidedAt { get; set; }
    public string? VoidedByAdminId { get; set; }
    public string? VoidReason { get; set; }
}