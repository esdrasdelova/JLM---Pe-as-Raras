using JLM.Data;
using JLM.Models;
using JLM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace JLM.Services;

public interface IOrderService
{
    Task<CheckoutResult> PlaceOrderAsync(User user, CheckoutViewModel model);
    Task<Order?> GetOrderAsync(string orderNumber, int? userId = null);
    Task<IReadOnlyList<Order>> GetUserOrdersAsync(int userId);
    Task<bool> CancelOrderAsync(string orderNumber, int userId);
}

public class OrderService : IOrderService
{
    private const decimal FreeStandardShippingThreshold = 500m;
    private const decimal FreeExpressShippingThreshold = 900m;
    private const decimal StandardShipping = 29.90m;
    private const decimal ExpressShipping = 49.90m;

    private readonly ApplicationDbContext _db;
    private readonly ICartService _cart;
    private readonly ICouponService _coupons;
    private readonly ILogger<OrderService> _logger;

    public OrderService(ApplicationDbContext db, ICartService cart, ICouponService coupons, ILogger<OrderService> logger)
    {
        _db = db;
        _cart = cart;
        _coupons = coupons;
        _logger = logger;
    }

    public async Task<CheckoutResult> PlaceOrderAsync(User user, CheckoutViewModel model)
    {
        var raw = _cart.GetRawItems().ToList();
        if (raw.Count == 0)
            return CheckoutResult.Fail("Seu carrinho está vazio.");

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var productIds = raw.Select(i => i.ProductId).Distinct().ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id) && p.IsActive)
                .ToDictionaryAsync(p => p.Id);

            var items = new List<OrderItem>();
            decimal subtotal = 0;

            foreach (var record in raw)
            {
                if (!products.TryGetValue(record.ProductId, out var product))
                    return CheckoutResult.Fail("Um dos produtos do carrinho não está mais disponível.");

                if (product.Stock < record.Quantity)
                    return CheckoutResult.Fail($"Estoque insuficiente para {product.Name}. Disponível: {product.Stock} unidade(s).");

                var lineTotal = product.Price * record.Quantity;
                subtotal += lineTotal;

                items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImageUrl = product.ImageUrl,
                    Size = record.Size,
                    Color = record.Color,
                    UnitPrice = product.Price,
                    Quantity = record.Quantity,
                    LineTotal = lineTotal
                });

                product.Stock -= record.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
            }

            var couponCode = string.IsNullOrWhiteSpace(model.CouponCode)
                ? await _cart.GetCouponCodeAsync()
                : model.CouponCode;

            decimal discount = 0;
            Coupon? coupon = null;
            if (!string.IsNullOrWhiteSpace(couponCode))
            {
                var validation = await _coupons.ValidateAsync(couponCode, subtotal);
                if (!validation.IsValid)
                    return CheckoutResult.Fail(validation.Error ?? "Cupom inválido.");
                coupon = validation.Coupon;
                discount = validation.Discount;
            }

            var shippingCost = model.ShippingMethod switch
            {
                ShippingMethod.Express => subtotal >= FreeExpressShippingThreshold ? 0 : ExpressShipping,
                _ => subtotal >= FreeStandardShippingThreshold ? 0 : StandardShipping
            };

            var total = Math.Max(0, subtotal + shippingCost - discount);
            var orderNumber = await GenerateOrderNumberAsync();

            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = user.Id,
                CustomerName = model.CustomerName,
                Email = model.Email,
                Phone = model.Phone,
                Cpf = model.Cpf,
                ShippingZipCode = model.ShippingZipCode,
                ShippingAddress = model.ShippingAddress,
                ShippingNumber = model.ShippingNumber,
                ShippingComplement = model.ShippingComplement,
                ShippingNeighborhood = model.ShippingNeighborhood,
                ShippingCity = model.ShippingCity,
                ShippingState = model.ShippingState,
                Subtotal = subtotal,
                ShippingCost = shippingCost,
                Discount = discount,
                Total = total,
                CouponCode = coupon?.Code,
                PaymentMethod = model.PaymentMethod,
                ShippingMethod = model.ShippingMethod,
                Status = OrderStatus.Pending,
                Notes = model.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = items
            };

            if (coupon is not null)
                coupon.UsedCount += 1;

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            await _cart.ClearAsync();
            _logger.LogInformation("Pedido {OrderNumber} criado para {Email}. Total {Total}", order.OrderNumber, order.Email, order.Total);

            return CheckoutResult.Ok(order);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Falha ao criar pedido para {Email}", model.Email);
            return CheckoutResult.Fail("Não foi possível finalizar o pedido. Tente novamente em instantes.");
        }
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var candidate = $"JLM-{DateTime.UtcNow:yyMMdd}-{Random.Shared.Next(1000, 9999)}";
            if (!await _db.Orders.AnyAsync(o => o.OrderNumber == candidate))
                return candidate;
        }
        return $"JLM-{DateTime.UtcNow:yyMMddHHmmss}";
    }

    public Task<Order?> GetOrderAsync(string orderNumber, int? userId = null)
    {
        var query = _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .AsQueryable();
        if (userId.HasValue)
            query = query.Where(o => o.UserId == userId.Value);
        return query.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<IReadOnlyList<Order>> GetUserOrdersAsync(int userId)
    {
        return await _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> CancelOrderAsync(string orderNumber, int userId)
    {
        var order = await _db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.UserId == userId);
        if (order is null || order.Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled)
            return false;

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        foreach (var item in order.Items.Where(i => i.ProductId.HasValue))
        {
            var product = await _db.Products.FindAsync(item.ProductId!.Value);
            if (product is not null) product.Stock += item.Quantity;
        }

        await _db.SaveChangesAsync();
        return true;
    }
}
