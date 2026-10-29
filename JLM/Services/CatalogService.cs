using JLM.Data;
using JLM.Models;
using JLM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace JLM.Services;

public interface ICatalogService
{
    Task<HomeViewModel> GetHomeAsync();
    Task<CatalogViewModel> GetCatalogAsync(CatalogViewModel filters);
    Task<Product?> GetBySlugAsync(string slug);
    Task<Product?> GetByIdAsync(int id);
    Task<IReadOnlyList<Product>> GetRelatedAsync(Product product, int take = 4);
    Task<IReadOnlyList<Product>> GetCompleteTheLookAsync(Product product, int take = 3);
}

public class CatalogService : ICatalogService
{
    private readonly ApplicationDbContext _db;

    public CatalogService(ApplicationDbContext db) => _db = db;

    public async Task<HomeViewModel> GetHomeAsync()
    {
        var products = _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Collection)
            .Where(p => p.IsActive);

        return new HomeViewModel
        {
            FeaturedProducts = await products.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt).Take(8).ToListAsync(),
            NewArrivals = await products.OrderByDescending(p => p.CreatedAt).Take(4).ToListAsync(),
            BestSellers = await products.OrderByDescending(p => p.ReviewCount).Take(4).ToListAsync(),
            Categories = await _db.Categories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(),
            Collections = await _db.Collections.AsNoTracking().Where(c => c.IsActive && c.IsFeatured).OrderBy(c => c.SortOrder).Take(4).ToListAsync(),
            ProductCount = await products.CountAsync()
        };
    }

    public async Task<CatalogViewModel> GetCatalogAsync(CatalogViewModel filters)
    {
        var query = _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Collection)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var term = filters.Search.Trim();
            query = query.Where(p =>
                EF.Functions.Like(p.Name, $"%{term}%") ||
                EF.Functions.Like(p.ShortDescription, $"%{term}%") ||
                EF.Functions.Like(p.Description, $"%{term}%") ||
                EF.Functions.Like(p.Category!.Name, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(filters.CategorySlug))
        {
            query = query.Where(p => p.Category!.Slug == filters.CategorySlug);
            filters.ActiveCategoryName = await _db.Categories.Where(c => c.Slug == filters.CategorySlug).Select(c => c.Name).FirstOrDefaultAsync();
        }

        if (!string.IsNullOrWhiteSpace(filters.CollectionSlug))
        {
            query = query.Where(p => p.Collection!.Slug == filters.CollectionSlug);
            filters.ActiveCollectionName = await _db.Collections.Where(c => c.Slug == filters.CollectionSlug).Select(c => c.Name).FirstOrDefaultAsync();
        }

        if (filters.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filters.MinPrice.Value);

        if (filters.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filters.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(filters.Size))
        {
            var size = filters.Size.Trim();
            query = query.Where(p => ("," + p.Sizes.Replace(" ", "") + ",").Contains("," + size + ","));
        }

        query = filters.Sort switch
        {
            "price-asc" => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "rating" => query.OrderByDescending(p => p.Rating).ThenByDescending(p => p.ReviewCount),
            "name" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.IsBestSeller).ThenByDescending(p => p.CreatedAt)
        };

        filters.TotalCount = await query.CountAsync();
        filters.Page = Math.Max(1, filters.Page);
        filters.PageSize = filters.PageSize is < 3 or > 48 ? 9 : filters.PageSize;

        filters.Products = await query
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        filters.Categories = await _db.Categories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync();
        filters.Collections = await _db.Collections.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync();

        var sizeData = await _db.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => p.Sizes)
            .ToListAsync();
        filters.AvailableSizes = sizeData
            .SelectMany(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct()
            .OrderBy(s => s.Length)
            .ThenBy(s => s)
            .ToList();

        return filters;
    }

    public Task<Product?> GetBySlugAsync(string slug) =>
        _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Collection)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

    public Task<Product?> GetByIdAsync(int id) =>
        _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Collection)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IReadOnlyList<Product>> GetRelatedAsync(Product product, int take = 4)
    {
        return await _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.Id != product.Id &&
                        (p.CategoryId == product.CategoryId || p.CollectionId == product.CollectionId))
            .OrderByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.Rating)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Product>> GetCompleteTheLookAsync(Product product, int take = 3)
    {
        var targetCategory = product.Category?.Slug == "roupas" ? "acessorios" : "roupas";
        return await _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.Category!.Slug == targetCategory)
            .OrderByDescending(p => p.IsFeatured)
            .Take(take)
            .ToListAsync();
    }
}
