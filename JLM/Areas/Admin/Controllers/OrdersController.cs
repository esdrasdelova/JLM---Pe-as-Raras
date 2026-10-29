using JLM.Data;
using JLM.Models;
using JLM.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JLM.Areas.Admin.Controllers;

public class OrdersController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ApplicationDbContext db, ILogger<OrdersController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? q, OrderStatus? status, int page = 1)
    {
        var query = _db.Orders.AsNoTracking().Include(o => o.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(o => EF.Functions.Like(o.OrderNumber, $"%{term}%") ||
                                     EF.Functions.Like(o.CustomerName, $"%{term}%") ||
                                     EF.Functions.Like(o.Email, $"%{term}%"));
        }

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        const int pageSize = 12;
        var total = await query.CountAsync();
        var orders = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((Math.Max(1, page) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View(new AdminOrdersViewModel
        {
            Orders = orders,
            Search = q,
            Status = status,
            Page = Math.Max(1, page),
            PageSize = pageSize,
            TotalCount = total
        });
    }

    public async Task<IActionResult> Details(string orderNumber)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        if (order is null) return NotFound();

        ViewBag.Statuses = Enum.GetValues<OrderStatus>();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string orderNumber, OrderStatus status, string? trackingNote)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        if (order is null) return NotFound();

        if (order.Status == status)
        {
            TempData["Info"] = "O pedido já está com este status.";
            return RedirectToAction(nameof(Details), new { orderNumber });
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            if (status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                foreach (var item in order.Items.Where(i => i.ProductId.HasValue))
                {
                    var product = await _db.Products.FindAsync(item.ProductId!.Value);
                    if (product is not null) product.Stock += item.Quantity;
                }
            }
            else if (order.Status == OrderStatus.Cancelled && status != OrderStatus.Cancelled)
            {
                foreach (var item in order.Items.Where(i => i.ProductId.HasValue))
                {
                    var product = await _db.Products.FindAsync(item.ProductId!.Value);
                    if (product is not null)
                    {
                        if (product.Stock < item.Quantity)
                        {
                            await transaction.RollbackAsync();
                            TempData["Error"] = $"Estoque insuficiente para reativar o pedido ({product.Name}).";
                            return RedirectToAction(nameof(Details), new { orderNumber });
                        }
                        product.Stock -= item.Quantity;
                    }
                }
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(trackingNote))
                order.Notes = trackingNote.Trim();

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Success"] = $"Status atualizado para “{order.StatusLabel}”.";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Erro ao atualizar status do pedido {OrderNumber}", orderNumber);
            TempData["Error"] = "Não foi possível atualizar o status.";
        }

        return RedirectToAction(nameof(Details), new { orderNumber });
    }
}
