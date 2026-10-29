using System.Text.Json;
using JLM.Data;
using JLM.Models;
using Microsoft.EntityFrameworkCore;

namespace JLM.Services;

public interface IWishlistService
{
    Task<IReadOnlyList<int>> GetIdsAsync();
    Task<IReadOnlyList<Product>> GetProductsAsync();
    Task<bool> ToggleAsync(int productId);
    Task<bool> ContainsAsync(int productId);
    Task<int> GetCountAsync();
    Task ClearAsync();
}

public class WishlistService : IWishlistService
{
    private const string Key = "jlm.wishlist.v1";
    private readonly IHttpContextAccessor _http;
    private readonly ApplicationDbContext _db;

    public WishlistService(IHttpContextAccessor http, ApplicationDbContext db)
    {
        _http = http;
        _db = db;
    }

    private ISession Session => _http.HttpContext!.Session;

    public Task<IReadOnlyList<int>> GetIdsAsync()
    {
        var json = Session.GetString(Key);
        if (string.IsNullOrWhiteSpace(json)) return Task.FromResult<IReadOnlyList<int>>(Array.Empty<int>());
        try
        {
            return Task.FromResult<IReadOnlyList<int>>(JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>());
        }
        catch
        {
            return Task.FromResult<IReadOnlyList<int>>(Array.Empty<int>());
        }
    }

    private void Save(IEnumerable<int> ids) =>
        Session.SetString(Key, JsonSerializer.Serialize(ids.Distinct().ToList()));

    public async Task<IReadOnlyList<Product>> GetProductsAsync()
    {
        var ids = await GetIdsAsync();
        if (ids.Count == 0) return Array.Empty<Product>();
        return await _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Where(p => ids.Contains(p.Id) && p.IsActive)
            .OrderByDescending(p => p.IsFeatured)
            .ToListAsync();
    }

    public async Task<bool> ToggleAsync(int productId)
    {
        var ids = (await GetIdsAsync()).ToList();
        var exists = ids.Remove(productId);
        if (!exists)
        {
            var valid = await _db.Products.AnyAsync(p => p.Id == productId && p.IsActive);
            if (!valid) return false;
            ids.Add(productId);
        }
        Save(ids);
        return !exists;
    }

    public async Task<bool> ContainsAsync(int productId) => (await GetIdsAsync()).Contains(productId);

    public async Task<int> GetCountAsync() => (await GetIdsAsync()).Count;

    public Task ClearAsync()
    {
        Session.Remove(Key);
        return Task.CompletedTask;
    }
}
