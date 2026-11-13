
using Microsoft.AspNetCore.Identity;

namespace TranslationOffice.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = "Customer";

    // اطلاعات بانکی مترجم؛ فقط برای خود مترجم و مدیر قابل نمایش است.
    public string? BankName { get; set; }
    public string? BankAccountHolder { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankCardNumber { get; set; }
    public string? BankSheba { get; set; }

    // درصد سهم مترجم از مبلغ خالص حق‌الترجمه (۰ تا ۱۰۰).
    public decimal TranslatorCommissionPercent { get; set; } = 0m;
}
