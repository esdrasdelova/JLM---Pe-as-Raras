using JLM.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JLM.Areas.Admin.Controllers;

public class NewsletterController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;

    public NewsletterController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var subscribers = await _db.NewsletterSubscribers.AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
        return View(subscribers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        var subscriber = await _db.NewsletterSubscribers.FindAsync(id);
        if (subscriber is null) return NotFound();
        _db.NewsletterSubscribers.Remove(subscriber);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Inscrito removido da lista.";
        return RedirectToAction(nameof(Index));
    }
}
