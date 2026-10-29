using JLM.Data;
using JLM.Helpers;
using JLM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JLM.Areas.Admin.Controllers;

public class CategoriesController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;

    public CategoriesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var categories = await _db.Categories.AsNoTracking()
            .Include(c => c.Products)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
        return View(categories);
    }

    [HttpGet]
    public IActionResult Create() => View(new Category { IsActive = true, SortOrder = 1 });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category model)
    {
        await ValidateAsync(model);
        if (!ModelState.IsValid) return View(model);

        model.Name = model.Name.Trim();
        model.Slug = await UniqueSlugAsync(string.IsNullOrWhiteSpace(model.Slug) ? model.Name.ToSlug() : model.Slug.ToSlug());
        _db.Categories.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Categoria criada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound();
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category model)
    {
        await ValidateAsync(model, model.Id);
        if (!ModelState.IsValid) return View(model);

        var category = await _db.Categories.FindAsync(model.Id);
        if (category is null) return NotFound();

        category.Name = model.Name.Trim();
        category.Slug = await UniqueSlugAsync(string.IsNullOrWhiteSpace(model.Slug) ? model.Name.ToSlug() : model.Slug.ToSlug(), model.Id);
        category.Description = model.Description;
        category.Eyebrow = model.Eyebrow;
        category.ImageUrl = model.ImageUrl;
        category.SortOrder = model.SortOrder;
        category.IsActive = model.IsActive;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Categoria atualizada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return NotFound();

        if (category.Products.Any())
        {
            category.IsActive = false;
            await _db.SaveChangesAsync();
            TempData["Error"] = "A categoria possui produtos e foi apenas desativada.";
        }
        else
        {
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Categoria removida.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateAsync(Category model, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Informe o nome da categoria.");
        var slug = string.IsNullOrWhiteSpace(model.Slug) ? model.Name?.ToSlug() : model.Slug.ToSlug();
        if (!string.IsNullOrWhiteSpace(slug) && await _db.Categories.AnyAsync(c => c.Slug == slug && c.Id != id))
            ModelState.AddModelError(nameof(model.Slug), "Já existe uma categoria com este slug.");
    }

    private async Task<string> UniqueSlugAsync(string slug, int? id = null)
    {
        var baseSlug = string.IsNullOrWhiteSpace(slug) ? "categoria" : slug;
        slug = baseSlug;
        var i = 2;
        while (await _db.Categories.AnyAsync(c => c.Slug == slug && c.Id != id))
            slug = $"{baseSlug}-{i++}";
        return slug;
    }
}
