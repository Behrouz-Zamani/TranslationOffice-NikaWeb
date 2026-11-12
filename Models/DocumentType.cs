using System.ComponentModel.DataAnnotations;

namespace TranslationOffice.Models;

public enum PricingType
{
    Fixed = 0,
    PerLine = 1,
    PerPage = 2
}

public class DocumentType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [MaxLength(300)]
    public string? SampleImagePath { get; set; }
    public PricingType PricingType { get; set; }
    public decimal BasePrice { get; set; }
    public int FreeUnits { get; set; }
    public decimal PricePerExtraUnit { get; set; }
    public decimal ExtraCopyPrice { get; set; }
    public decimal JudiciaryFee { get; set; }
    public decimal ForeignAffairsFee { get; set; }
    public bool IsActive { get; set; } = true;
}