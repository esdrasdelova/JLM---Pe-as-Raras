using JLM.Data;
using JLM.Helpers;
using JLM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JLM.Areas.Admin.Controllers;

public class CollectionsController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;

    public CollectionsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var collections = await _db.Collections.AsNoTracking()
            .Include(c => c.Products)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
        return View(collections);
    }

    [HttpGet]
    public IActionResult Create() => View(new Collection { IsActive = true, IsFeatured = true, SortOrder = 1 });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Collection model)
    {
        await ValidateAsync(model);
        if (!ModelState.IsValid) return View(model);

        model.Name = model.Name.Trim();
        model.Slug = await UniqueSlugAsync(string.IsNullOrWhiteSpace(model.Slug) ? model.Name.ToSlug() : model.Slug.ToSlug());
        _db.Collections.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Coleção criada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var collection = await _db.Collections.FindAsync(id);
        if (collection is null) return NotFound();
        return View(collection);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Collection model)
    {
        await ValidateAsync(model, model.Id);
        if (!ModelState.IsValid) return View(model);

        var collection = await _db.Collections.FindAsync(model.Id);
        if (collection is null) return NotFound();

        collection.Name = model.Name.Trim();
        collection.Slug = await UniqueSlugAsync(string.IsNullOrWhiteSpace(model.Slug) ? model.Name.ToSlug() : model.Slug.ToSlug(), model.Id);
        collection.Tagline = model.Tagline;
        collection.Description = model.Description;
        collection.ImageUrl = model.ImageUrl;
        collection.IsFeatured = model.IsFeatured;
        collection.SortOrder = model.SortOrder;
        collection.IsActive = model.IsActive;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Coleção atualizada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var collection = await _db.Collections.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        if (collection is null) return NotFound();

        if (collection.Products.Any())
        {
            collection.IsActive = false;
            await _db.SaveChangesAsync();
            TempData["Error"] = "A coleção possui produtos e foi apenas desativada.";
        }
        else
        {
            _db.Collections.Remove(collection);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Coleção removida.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateAsync(Collection model, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Informe o nome da coleção.");
        var slug = string.IsNullOrWhiteSpace(model.Slug) ? model.Name?.ToSlug() : model.Slug.ToSlug();
        if (!string.IsNullOrWhiteSpace(slug) && await _db.Collections.AnyAsync(c => c.Slug == slug && c.Id != id))
            ModelState.AddModelError(nameof(model.Slug), "Já existe uma coleção com este slug.");
    }

    private async Task<string> UniqueSlugAsync(string slug, int? id = null)
    {
        var baseSlug = string.IsNullOrWhiteSpace(slug) ? "colecao" : slug;
        slug = baseSlug;
        var i = 2;
        while (await _db.Collections.AnyAsync(c => c.Slug == slug && c.Id != id))
            slug = $"{baseSlug}-{i++}";
        return slug;
    }
}
