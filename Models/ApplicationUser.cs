
using Microsoft.AspNetCore.Identity;

namespace TranslationOffice.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = "Customer";
}
