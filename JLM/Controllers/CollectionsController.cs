using JLM.Data;
using JLM.Services;
using JLM.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JLM.Controllers;

public class CollectionsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICatalogService _catalog;

    public CollectionsController(ApplicationDbContext db, ICatalogService catalog)
    {
        _db = db;
        _catalog = catalog;
    }

    [HttpGet("/colecoes")]
    public async Task<IActionResult> Index()
    {
        var collections = await _db.Collections.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
        return View(collections);
    }

    [HttpGet("/colecoes/{slug}")]
    public async Task<IActionResult> Details(string slug, string? ordenar = "relevance", int pagina = 1)
    {
        var collection = await _db.Collections.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);
        if (collection is null) return NotFound();

        var model = await _catalog.GetCatalogAsync(new CatalogViewModel
        {
            CollectionSlug = collection.Slug,
            Sort = string.IsNullOrWhiteSpace(ordenar) ? "relevance" : ordenar,
            Page = pagina,
            PageSize = 9,
            Title = collection.Name,
            Eyebrow = "Coleção JLM",
            Description = collection.Tagline,
            HeroImage = collection.ImageUrl
        });

        ViewBag.Collection = collection;
        ViewBag.CollectionDescription = collection.Description;
        return View(model);
    }
}
