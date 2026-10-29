using JLM.Data;
using JLM.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JLM.Areas.Admin.Controllers;

public class CustomersController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;

    public CustomersController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var query = _db.Users.AsNoTracking().Where(u => u.Role == "Customer");

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(u => EF.Functions.Like(u.FullName, $"%{term}%") || EF.Functions.Like(u.Email, $"%{term}%"));
        }

        const int pageSize = 12;
        var total = await query.CountAsync();
        var customers = await query
            .Include(u => u.Orders)
            .OrderByDescending(u => u.CreatedAt)
            .Skip((Math.Max(1, page) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View(new AdminCustomersViewModel
        {
            Customers = customers,
            Search = q,
            Page = Math.Max(1, page),
            PageSize = pageSize,
            TotalCount = total
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var customer = await _db.Users.AsNoTracking()
            .Include(u => u.Orders.OrderByDescending(o => o.CreatedAt))
            .FirstOrDefaultAsync(u => u.Id == id && u.Role == "Customer");
        if (customer is null) return NotFound();
        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "Customer");
        if (user is null) return NotFound();

        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = user.IsActive ? "Cliente reativado." : "Cliente desativado.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
