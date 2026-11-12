namespace TranslationOffice.Models;

public sealed class ShoppingCart
{
    public int Id { get; set; }
    public string CartKey { get; set; } = string.Empty;
    public string CartJson { get; set; } = "{}";
    public int? OrderId { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
