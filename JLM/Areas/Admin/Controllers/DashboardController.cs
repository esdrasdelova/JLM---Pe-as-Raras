using JLM.Data;
using JLM.Models;
using JLM.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JLM.Areas.Admin.Controllers;

public class DashboardController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var sixMonthsAgo = monthStart.AddMonths(-5);

        var validStatuses = new[]
        {
            OrderStatus.Paid, OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered
        };

        var orders = await _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var validOrders = orders.Where(o => validStatuses.Contains(o.Status)).ToList();
        var monthOrders = validOrders.Where(o => o.CreatedAt >= monthStart).ToList();
        var recentSales = orders.Where(o => o.CreatedAt >= sixMonthsAgo && validStatuses.Contains(o.Status)).ToList();

        var monthlySales = Enumerable.Range(0, 6)
            .Select(offset => monthStart.AddMonths(offset - 5))
            .Select(month => new MonthlySalesItem
            {
                Label = month.ToLocalTime().ToString("MMM", new System.Globalization.CultureInfo("pt-BR")).TrimEnd('.').ToUpperInvariant(),
                Revenue = recentSales.Where(o => o.CreatedAt.Year == month.Year && o.CreatedAt.Month == month.Month).Sum(o => o.Total),
                Orders = recentSales.Count(o => o.CreatedAt.Year == month.Year && o.CreatedAt.Month == month.Month)
            })
            .ToList();

        var topProducts = orders.Where(o => validStatuses.Contains(o.Status))
            .SelectMany(o => o.Items)
            .Where(i => i.ProductId.HasValue)
            .GroupBy(i => new { i.ProductId, i.ProductName, i.ProductImageUrl })
            .Select(g => new TopProductItem
            {
                Name = g.Key.ProductName,
                ImageUrl = g.Key.ProductImageUrl,
                UnitsSold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.LineTotal)
            })
            .OrderByDescending(t => t.UnitsSold)
            .Take(5)
            .ToList();

        var model = new DashboardViewModel
        {
            Revenue = validOrders.Sum(o => o.Total),
            RevenueThisMonth = monthOrders.Sum(o => o.Total),
            OrdersCount = orders.Count(o => o.Status != OrderStatus.Cancelled),
            OrdersThisMonth = orders.Count(o => o.CreatedAt >= monthStart && o.Status != OrderStatus.Cancelled),
            CustomersCount = await _db.Users.CountAsync(u => u.Role == "Customer"),
            ProductsCount = await _db.Products.CountAsync(p => p.IsActive),
            LowStockCount = await _db.Products.CountAsync(p => p.IsActive && p.Stock <= 15),
            PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
            UnreadMessages = await _db.ContactMessages.CountAsync(m => !m.IsRead),
            AverageTicket = validOrders.Count > 0 ? Math.Round(validOrders.Average(o => o.Total), 2) : 0,
            RecentOrders = orders.Take(6).ToList(),
            LowStockProducts = await _db.Products.AsNoTracking()
                .Where(p => p.IsActive && p.Stock <= 15)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync(),
            TopProducts = topProducts,
            MonthlySales = monthlySales,
            StatusCounts = orders.GroupBy(o => o.Status).ToDictionary(g => g.Key, g => g.Count())
        };

        return View(model);
    }
}
