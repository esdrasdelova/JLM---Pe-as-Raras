using JLM.Data;
using JLM.Models;
using JLM.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JLM.Controllers;

public class ContactController : Controller
{
    private readonly ApplicationDbContext _db;

    public ContactController(ApplicationDbContext db) => _db = db;

    [HttpGet("/contato")]
    public IActionResult Index() => View(new ContactViewModel());

    [HttpPost("/contato")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        _db.ContactMessages.Add(new ContactMessage
        {
            Name = model.Name.Trim(),
            Email = model.Email.Trim().ToLowerInvariant(),
            Phone = model.Phone?.Trim(),
            Subject = model.Subject.Trim(),
            Message = model.Message.Trim()
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = "Mensagem recebida. Nosso time responde em até 1 dia útil.";
        return RedirectToAction(nameof(Index));
    }
}
