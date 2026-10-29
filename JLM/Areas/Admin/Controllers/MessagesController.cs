using JLM.Data;
using JLM.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JLM.Areas.Admin.Controllers;

public class MessagesController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;

    public MessagesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? filter)
    {
        var query = _db.ContactMessages.AsNoTracking().AsQueryable();
        if (filter == "unread") query = query.Where(m => !m.IsRead);

        return View(new AdminMessagesViewModel
        {
            Messages = await query.OrderByDescending(m => m.CreatedAt).ToListAsync(),
            Filter = filter,
            UnreadCount = await _db.ContactMessages.CountAsync(m => !m.IsRead)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRead(int id)
    {
        var message = await _db.ContactMessages.FindAsync(id);
        if (message is null) return NotFound();
        message.IsRead = !message.IsRead;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var message = await _db.ContactMessages.FindAsync(id);
        if (message is null) return NotFound();
        _db.ContactMessages.Remove(message);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Mensagem removida.";
        return RedirectToAction(nameof(Index));
    }
}
