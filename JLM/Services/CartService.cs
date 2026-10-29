using System.Text.Json;
using JLM.Data;
using JLM.Helpers;
using JLM.Models;
using JLM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace JLM.Services;

public class CartItemRecord
{
    public string Key { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public int Quantity { get; set; }
}

public interface ICartService
{
    Task<CartViewModel> GetCartAsync();
    Task<int> GetCountAsync();
    Task<decimal> GetSubtotalAsync();
    Task AddAsync(int productId, string? size, string? color, int quantity = 1);
    Task<bool> UpdateAsync(string key, int quantity);
    Task RemoveAsync(string key);
    Task ClearAsync();
    Task SetCouponAsync(string? code);
    Task<string?> GetCouponCodeAsync();
    IReadOnlyList<CartItemRecord> GetRawItems();
}

public class CartService : ICartService
{
    private const string CartSessionKey = "jlm.cart.v1";
    private const string CouponSessionKey = "jlm.coupon";
    private readonly ApplicationDbContext _db;
    private readonly ICouponService _coupons;
    private readonly IHttpContextAccessor _http;

    public CartService(ApplicationDbContext db, ICouponService coupons, IHttpContextAccessor http)
    {
        _db = db;
        _coupons = coupons;
        _http = http;
    }

    private ISession Session => _http.HttpContext!.Session;

    public IReadOnlyList<CartItemRecord> GetRawItems()
    {
        var json = Session.GetString(CartSessionKey);
        if (string.IsNullOrWhiteSpace(json)) return Array.Empty<CartItemRecord>();
        try
        {
            return JsonSerializer.Deserialize<List<CartItemRecord>>(json) ?? new List<CartItemRecord>();
        }
        catch
        {
            return Array.Empty<CartItemRecord>();
        }
    }

    private void SaveRawItems(IEnumerable<CartItemRecord> items)
    {
        var list = items.Where(i => i.Quantity > 0).ToList();
        Session.SetString(CartSessionKey, JsonSerializer.Serialize(list));
    }

    public async Task<CartViewModel> GetCartAsync()
    {
        var raw = GetRawItems();
        var ids = raw.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => ids.Contains(p.Id) && p.IsActive)
            .ToDictionaryAsync(p => p.Id);

        var lines = new List<CartLine>();
        foreach (var item in raw)
        {
            if (!products.TryGetValue(item.ProductId, out var product)) continue;
            var quantity = Math.Clamp(item.Quantity, 1, Math.Max(1, product.Stock));
            lines.Add(new CartLine
            {
                Key = item.Key,
                Product = product,
                Size = item.Size,
                Color = item.Color,
                Quantity = quantity
            });
        }

        var subtotal = lines.Sum(l => l.LineTotal);
        var vm = new CartViewModel { Items = lines, CouponCode = await GetCouponCodeAsync() };

        if (!string.IsNullOrWhiteSpace(vm.CouponCode) && lines.Count > 0)
        {
            var validation = await _coupons.ValidateAsync(vm.CouponCode, subtotal);
            if (validation.IsValid)
            {
                vm.Coupon = validation.Coupon;
                vm.Discount = validation.Discount;
            }
            else
            {
                vm.CouponError = validation.Error;
            }
        }

        return vm;
    }

    public Task<int> GetCountAsync()
    {
        var count = GetRawItems().Sum(i => i.Quantity);
        return Task.FromResult(count);
    }

    public Task<decimal> GetSubtotalAsync()
    {
        var raw = GetRawItems();
        var ids = raw.Select(i => i.ProductId).Distinct().ToList();
        var prices = _db.Products.AsNoTracking()
            .Where(p => ids.Contains(p.Id) && p.IsActive)
            .Select(p => new { p.Id, p.Price })
            .ToDictionary(p => p.Id, p => p.Price);
        var subtotal = raw.Where(i => prices.ContainsKey(i.ProductId))
            .Sum(i => prices[i.ProductId] * i.Quantity);
        return Task.FromResult(subtotal);
    }

    public async Task AddAsync(int productId, string? size, string? color, int quantity = 1)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId && p.IsActive)
            ?? throw new InvalidOperationException("Produto indisponível.");

        if (product.Stock <= 0)
            throw new InvalidOperationException("Produto esgotado.");

        var sizes = product.SizeList;
        if (sizes.Length > 0)
        {
            if (string.IsNullOrWhiteSpace(size) || !sizes.Contains(size, StringComparer.OrdinalIgnoreCase))
                size = sizes[0];
        }
        else
        {
            size = "Único";
        }

        var colors = product.ColorList.ToList();
        if (colors.Count > 0)
        {
            if (string.IsNullOrWhiteSpace(color) || !colors.Any(c => string.Equals(c.Name, color, StringComparison.OrdinalIgnoreCase)))
                color = colors[0].Name;
        }
        else
        {
            color = "Padrão";
        }

        var key = $"{productId}-{size.ToSlug()}-{color.ToSlug()}";
        var items = GetRawItems().ToList();
        var existing = items.FirstOrDefault(i => i.Key == key);
        if (existing is null)
        {
            items.Add(new CartItemRecord
            {
                Key = key,
                ProductId = productId,
                Size = size,
                Color = color,
                Quantity = Math.Clamp(quantity, 1, product.Stock)
            });
        }
        else
        {
            existing.Quantity = Math.Clamp(existing.Quantity + Math.Max(1, quantity), 1, product.Stock);
        }

        SaveRawItems(items);
    }

    public Task<bool> UpdateAsync(string key, int quantity)
    {
        var items = GetRawItems().ToList();
        var item = items.FirstOrDefault(i => i.Key == key);
        if (item is null) return Task.FromResult(false);

        if (quantity <= 0)
        {
            items.Remove(item);
        }
        else
        {
            item.Quantity = Math.Min(quantity, 99);
        }

        SaveRawItems(items);
        return Task.FromResult(true);
    }

    public Task RemoveAsync(string key)
    {
        SaveRawItems(GetRawItems().Where(i => i.Key != key));
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        Session.Remove(CartSessionKey);
        Session.Remove(CouponSessionKey);
        return Task.CompletedTask;
    }

    public Task SetCouponAsync(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) Session.Remove(CouponSessionKey);
        else Session.SetString(CouponSessionKey, code.Trim().ToUpperInvariant());
        return Task.CompletedTask;
    }

    public Task<string?> GetCouponCodeAsync() =>
        Task.FromResult(Session.GetString(CouponSessionKey));
}
