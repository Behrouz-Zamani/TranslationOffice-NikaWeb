
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace TranslationOffice.Services;

public static class GuestOrderToken
{
    // تولید توکن تصادفی ۳۲ بایتی
    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        return WebEncoders.Base64UrlEncode(bytes);
    }

    // ذخیره فقط هش توکن در دیتابیس
    public static string Hash(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);

        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }

    // مقایسه امن توکن دریافتی با هش ذخیره‌شده
    public static bool Verify(string? token, string? storedHash)
    {
        if (string.IsNullOrWhiteSpace(token) ||
            string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        var actualHash = Hash(token);

        if (actualHash.Length != storedHash.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(actualHash),
            Encoding.UTF8.GetBytes(storedHash));
    }
}
