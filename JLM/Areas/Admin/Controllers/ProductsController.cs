using JLM.Data;
using JLM.Helpers;
using JLM.Models;
using JLM.Services;
using JLM.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JLM.Areas.Admin.Controllers;

public class ProductsController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IFileUploadService _uploads;

    public ProductsController(ApplicationDbContext db, IFileUploadService uploads)
    {
        _db = db;
        _uploads = uploads;
    }

    public async Task<IActionResult> Index(string? q, int? categoryId, string? status, int page = 1)
    {
        var query = _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Collection)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{term}%") || EF.Functions.Like(p.Sku, $"%{term}%"));
        }

        if (categoryId.HasValue && categoryId > 0)
            query = query.Where(p => p.CategoryId == categoryId);

        if (status == "active") query = query.Where(p => p.IsActive);
        if (status == "inactive") query = query.Where(p => !p.IsActive);
        if (status == "low") query = query.Where(p => p.Stock <= 15);

        const int pageSize = 10;
        var total = await query.CountAsync();
        var products = await query.OrderByDescending(p => p.CreatedAt)
            .Skip((Math.Max(1, page) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Search = q;
        ViewBag.CategoryId = categoryId;
        ViewBag.Status = status;
        ViewBag.Page = Math.Max(1, page);
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Total = total;
        ViewBag.Categories = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ProductFormViewModel();
        await PopulateOptionsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        await PopulateOptionsAsync(model);
        await ValidateProductAsync(model);

        if (!ModelState.IsValid) return View(model);

        var slug = await EnsureUniqueSlugAsync(string.IsNullOrWhiteSpace(model.Slug) ? model.Name.ToSlug() : model.Slug.ToSlug());
        string imageUrl;
        try
        {
            imageUrl = await _uploads.SaveProductImageAsync(model.MainImageFile) ?? model.ImageUrl.Trim();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.MainImageFile), ex.Message);
            return View(model);
        }

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            ModelState.AddModelError(nameof(model.ImageUrl), "Envie uma imagem ou informe uma URL.");
            return View(model);
        }

        var product = new Product
        {
            Name = model.Name.Trim(),
            Slug = slug,
            ShortDescription = model.ShortDescription.Trim(),
            Description = model.Description.Trim(),
            Composition = model.Composition?.Trim(),
            CareInstructions = model.CareInstructions?.Trim(),
            Price = model.Price,
            ComparePrice = model.ComparePrice,
            Sku = model.Sku.Trim().ToUpperInvariant(),
            Stock = model.Stock,
            CategoryId = model.CategoryId,
            CollectionId = model.CollectionId,
            ImageUrl = imageUrl,
            Sizes = model.Sizes.Trim(),
            Colors = model.Colors.Trim(),
            IsFeatured = model.IsFeatured,
            IsNewArrival = model.IsNewArrival,
            IsBestSeller = model.IsBestSeller,
            IsActive = model.IsActive,
            Rating = model.Rating,
            ReviewCount = model.ReviewCount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        AddImages(product, model.GalleryUrls, imageUrl);
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Produto “{product.Name}” criado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.AsNoTracking().Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        var model = new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Composition = product.Composition,
            CareInstructions = product.CareInstructions,
            Price = product.Price,
            ComparePrice = product.ComparePrice,
            Sku = product.Sku,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            CollectionId = product.CollectionId,
            ImageUrl = product.ImageUrl,
            Sizes = product.Sizes,
            Colors = product.Colors,
            IsFeatured = product.IsFeatured,
            IsNewArrival = product.IsNewArrival,
            IsBestSeller = product.IsBestSeller,
            IsActive = product.IsActive,
            Rating = product.Rating,
            ReviewCount = product.ReviewCount,
            GalleryUrls = string.Join('\n', product.Images
                .Where(i => i.ImageUrl != product.ImageUrl)
                .OrderBy(i => i.SortOrder)
                .Select(i => i.ImageUrl))
        };
        await PopulateOptionsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductFormViewModel model)
    {
        await PopulateOptionsAsync(model);
        await ValidateProductAsync(model, model.Id);

        if (!ModelState.IsValid)
            return View(model);

        var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == model.Id);
        if (product is null) return NotFound();

        try
        {
            var uploaded = await _uploads.SaveProductImageAsync(model.MainImageFile);
            if (!string.IsNullOrWhiteSpace(uploaded)) product.ImageUrl = uploaded;
            else if (!string.IsNullOrWhiteSpace(model.ImageUrl)) product.ImageUrl = model.ImageUrl.Trim();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.MainImageFile), ex.Message);
            return View(model);
        }

        product.Name = model.Name.Trim();
        product.Slug = await EnsureUniqueSlugAsync(string.IsNullOrWhiteSpace(model.Slug) ? model.Name.ToSlug() : model.Slug.ToSlug(), product.Id);
        product.ShortDescription = model.ShortDescription.Trim();
        product.Description = model.Description.Trim();
        product.Composition = model.Composition?.Trim();
        product.CareInstructions = model.CareInstructions?.Trim();
        product.Price = model.Price;
        product.ComparePrice = model.ComparePrice;
        product.Sku = model.Sku.Trim().ToUpperInvariant();
        product.Stock = model.Stock;
        product.CategoryId = model.CategoryId;
        product.CollectionId = model.CollectionId;
        product.Sizes = model.Sizes.Trim();
        product.Colors = model.Colors.Trim();
        product.IsFeatured = model.IsFeatured;
        product.IsNewArrival = model.IsNewArrival;
        product.IsBestSeller = model.IsBestSeller;
        product.IsActive = model.IsActive;
        product.Rating = model.Rating;
        product.ReviewCount = model.ReviewCount;
        product.UpdatedAt = DateTime.UtcNow;

        _db.ProductImages.RemoveRange(product.Images);
        AddImages(product, model.GalleryUrls, product.ImageUrl);

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Produto “{product.Name}” atualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        product.IsActive = !product.IsActive;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = product.IsActive ? "Produto ativado na loja." : "Produto ocultado da loja.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.Include(p => p.OrderItems).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        if (product.OrderItems.Any())
        {
            product.IsActive = false;
            await _db.SaveChangesAsync();
            TempData["Error"] = "O produto possui pedidos vinculados e foi apenas desativado.";
        }
        else
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Produto removido definitivamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOptionsAsync(ProductFormViewModel model)
    {
        model.Categories = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        model.Collections = await _db.Collections.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
    }

    private async Task ValidateProductAsync(ProductFormViewModel model, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(model.Name) || model.Name.Trim().Length < 3)
            ModelState.AddModelError(nameof(model.Name), "Informe o nome do produto (mínimo 3 caracteres).");
        if (string.IsNullOrWhiteSpace(model.ShortDescription))
            ModelState.AddModelError(nameof(model.ShortDescription), "Informe a descrição curta.");
        if (string.IsNullOrWhiteSpace(model.Description))
            ModelState.AddModelError(nameof(model.Description), "Informe a descrição completa.");
        if (model.Price <= 0)
            ModelState.AddModelError(nameof(model.Price), "O preço deve ser maior que zero.");
        if (model.ComparePrice.HasValue && model.ComparePrice <= model.Price)
            ModelState.AddModelError(nameof(model.ComparePrice), "O preço comparativo deve ser maior que o preço atual.");
        if (string.IsNullOrWhiteSpace(model.Sku))
            ModelState.AddModelError(nameof(model.Sku), "Informe o SKU.");
        if (model.Stock < 0)
            ModelState.AddModelError(nameof(model.Stock), "O estoque não pode ser negativo.");
        if (model.CategoryId <= 0 || !await _db.Categories.AnyAsync(c => c.Id == model.CategoryId))
            ModelState.AddModelError(nameof(model.CategoryId), "Selecione uma categoria válida.");
        if (string.IsNullOrWhiteSpace(model.Sizes))
            ModelState.AddModelError(nameof(model.Sizes), "Informe ao menos um tamanho.");
        if (string.IsNullOrWhiteSpace(model.Colors) || !model.Colors.Contains(':'))
            ModelState.AddModelError(nameof(model.Colors), "Informe as cores no formato Nome:#hex.");
        if (await _db.Products.AnyAsync(p => p.Sku == model.Sku.Trim().ToUpper() && p.Id != id))
            ModelState.AddModelError(nameof(model.Sku), "Já existe um produto com este SKU.");
        if (id is null && model.MainImageFile is null && string.IsNullOrWhiteSpace(model.ImageUrl))
            ModelState.AddModelError(nameof(model.ImageUrl), "Envie uma imagem ou informe uma URL.");
    }

    private async Task<string> EnsureUniqueSlugAsync(string slug, int? currentId = null)
    {
        if (string.IsNullOrWhiteSpace(slug)) slug = "produto";
        var baseSlug = slug;
        var counter = 2;
        while (await _db.Products.AnyAsync(p => p.Slug == slug && p.Id != currentId))
            slug = $"{baseSlug}-{counter++}";
        return slug;
    }

    private static void AddImages(Product product, string? galleryUrls, string mainImage)
    {
        var order = 1;
        product.Images.Add(new ProductImage
        {
            ImageUrl = mainImage,
            AltText = product.Name,
            SortOrder = order++
        });

        if (string.IsNullOrWhiteSpace(galleryUrls)) return;

        foreach (var url in galleryUrls.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct())
        {
            if (url == mainImage) continue;
            product.Images.Add(new ProductImage
            {
                ImageUrl = url,
                AltText = $"{product.Name} — imagem {order}",
                SortOrder = order++
            });
        }
    }
}
