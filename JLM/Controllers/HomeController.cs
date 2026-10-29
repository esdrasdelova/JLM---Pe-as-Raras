using JLM.Data;
using JLM.Models;
using JLM.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JLM.Controllers;

public class HomeController : Controller
{
    private readonly ICatalogService _catalog;
    private readonly ApplicationDbContext _db;

    public HomeController(ICatalogService catalog, ApplicationDbContext db)
    {
        _catalog = catalog;
        _db = db;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var model = await _catalog.GetHomeAsync();
        return View(model);
    }

    [HttpGet("/quem-sou")]
    public IActionResult About() => View();

    [HttpGet("/privacidade")]
    public IActionResult Privacy() => View();

    [HttpPost("/newsletter")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Newsletter(string email, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(email) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
        {
            TempData["Error"] = "Informe um e-mail válido para receber novidades da JLM.";
            return LocalRedirect(SafeReturnUrl(returnUrl));
        }

        email = email.Trim().ToLowerInvariant();
        var exists = await _db.NewsletterSubscribers.AnyAsync(n => n.Email == email);
        if (exists)
        {
            TempData["Info"] = "Você já faz parte da lista JLM. Em breve, novidades.";
        }
        else
        {
            _db.NewsletterSubscribers.Add(new NewsletterSubscriber { Email = email });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Bem-vindo à lista JLM. Você acaba de garantir acesso antecipado aos lançamentos.";
        }

        return LocalRedirect(SafeReturnUrl(returnUrl));
    }

    private string SafeReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
}
