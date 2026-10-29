using JLM.Data;
using JLM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JLM.Areas.Admin.Controllers;

public class CouponsController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;

    public CouponsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var coupons = await _db.Coupons.AsNoTracking().OrderByDescending(c => c.CreatedAt).ToListAsync();
        return View(coupons);
    }

    [HttpGet]
    public IActionResult Create() => View(new Coupon { IsActive = true, StartsAt = DateTime.UtcNow, EndsAt = DateTime.UtcNow.AddMonths(1) });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Coupon model)
    {
        await ValidateAsync(model);
        if (!ModelState.IsValid) return View(model);

        model.Code = model.Code.Trim().ToUpperInvariant();
        model.CreatedAt = DateTime.UtcNow;
        _db.Coupons.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cupom criado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var coupon = await _db.Coupons.FindAsync(id);
        if (coupon is null) return NotFound();
        return View(coupon);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Coupon model)
    {
        await ValidateAsync(model, model.Id);
        if (!ModelState.IsValid) return View(model);

        var coupon = await _db.Coupons.FindAsync(model.Id);
        if (coupon is null) return NotFound();

        coupon.Code = model.Code.Trim().ToUpperInvariant();
        coupon.Description = model.Description;
        coupon.DiscountType = model.DiscountType;
        coupon.Value = model.Value;
        coupon.MinimumOrder = model.MinimumOrder;
        coupon.MaxUses = model.MaxUses;
        coupon.StartsAt = model.StartsAt;
        coupon.EndsAt = model.EndsAt;
        coupon.IsActive = model.IsActive;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Cupom atualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var coupon = await _db.Coupons.FindAsync(id);
        if (coupon is null) return NotFound();
        coupon.IsActive = !coupon.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = coupon.IsActive ? "Cupom ativado." : "Cupom desativado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var coupon = await _db.Coupons.FindAsync(id);
        if (coupon is null) return NotFound();
        _db.Coupons.Remove(coupon);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cupom removido.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateAsync(Coupon model, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(model.Code))
            ModelState.AddModelError(nameof(model.Code), "Informe o código do cupom.");
        else if (await _db.Coupons.AnyAsync(c => c.Code == model.Code.Trim().ToUpper() && c.Id != id))
            ModelState.AddModelError(nameof(model.Code), "Já existe um cupom com este código.");
        if (model.Value <= 0)
            ModelState.AddModelError(nameof(model.Value), "O valor do desconto deve ser positivo.");
        if (model.DiscountType == DiscountType.Percentage && model.Value > 100)
            ModelState.AddModelError(nameof(model.Value), "O percentual não pode ser maior que 100.");
        if (model.StartsAt.HasValue && model.EndsAt.HasValue && model.EndsAt <= model.StartsAt)
            ModelState.AddModelError(nameof(model.EndsAt), "A data final deve ser posterior à inicial.");
    }
}
