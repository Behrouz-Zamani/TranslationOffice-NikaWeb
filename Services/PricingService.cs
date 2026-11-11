using TranslationOffice.Models;

namespace TranslationOffice.Services;

public class PricingService
{
    public decimal Calculate(DocumentType doc, int units, int quantity,
                             bool needJudiciary, bool needForeignAffairs)
    {
        decimal price = doc.BasePrice;

        if (doc.PricingType != PricingType.Fixed && units > doc.FreeUnits)
        {
            price += (units - doc.FreeUnits) * doc.PricePerExtraUnit;
        }

        if (quantity > 1)
        {
            price += (quantity - 1) * doc.ExtraCopyPrice;
        }

        if (needJudiciary)
            price += doc.JudiciaryFee;

        if (needForeignAffairs)
            price += doc.ForeignAffairsFee;

        return price;
    }
}