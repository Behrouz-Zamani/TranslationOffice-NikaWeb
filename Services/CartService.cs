using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TranslationOffice.Data;
using TranslationOffice.Models;

namespace TranslationOffice.Services;

public sealed class CartService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public CartService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public string GuestCartKey { get; set; } = string.Empty;

    public event Action<string, int>? CartChanged;

    public async Task<string?> LoadAsync(string cartKey)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.ShoppingCarts.AsNoTracking()
            .Where(x => x.CartKey == cartKey)
            .Select(x => x.CartJson)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetCountAsync(string cartKey)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.ShoppingCarts.AsNoTracking()
            .Where(x => x.CartKey == cartKey)
            .Select(x => (int?)x.ItemCount)
            .FirstOrDefaultAsync() ?? 0;
    }

    public async Task SaveAsync(string cartKey, string cartJson, int? orderId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var cart = await db.ShoppingCarts.FirstOrDefaultAsync(x => x.CartKey == cartKey);
        if (cart == null)
        {
            cart = new ShoppingCart { CartKey = cartKey };
            db.ShoppingCarts.Add(cart);
        }

        cart.CartJson = cartJson;
        cart.OrderId = orderId;
        cart.ItemCount = ReadItemCount(cartJson);
        cart.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        CartChanged?.Invoke(cartKey, ReadItemCount(cartJson));
    }

    public async Task DeleteAsync(string cartKey)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var cart = await db.ShoppingCarts.FirstOrDefaultAsync(x => x.CartKey == cartKey);
        if (cart != null)
        {
            db.ShoppingCarts.Remove(cart);
            await db.SaveChangesAsync();
        }

        CartChanged?.Invoke(cartKey, 0);
    }

    public async Task DeleteByOrderIdAsync(int orderId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var carts = await db.ShoppingCarts.Where(x => x.OrderId == orderId).ToListAsync();
        if (carts.Count == 0) return;

        var keys = carts.Select(x => x.CartKey).ToList();
        db.ShoppingCarts.RemoveRange(carts);
        await db.SaveChangesAsync();

        foreach (var key in keys)
            CartChanged?.Invoke(key, 0);
    }

    private static int ReadItemCount(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("Items", out var items) &&
                items.ValueKind == JsonValueKind.Array)
                return items.GetArrayLength();
        }
        catch (JsonException) { }

        return 0;
    }
}
