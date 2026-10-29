using JLM.Data;
using JLM.Models;
using Microsoft.EntityFrameworkCore;

namespace JLM.Services;

public class CouponValidationResult
{
    public bool IsValid { get; init; }
    public Coupon? Coupon { get; init; }
    public decimal Discount { get; init; }
    public string? Error { get; init; }

    public static CouponValidationResult Invalid(string error) => new() { IsValid = false, Error = error };
}

public interface ICouponService
{
    Task<CouponValidationResult> ValidateAsync(string? code, decimal subtotal);
    Task<decimal> CalculateDiscountAsync(Coupon coupon, decimal subtotal);
}

public class CouponService : ICouponService
{
    private readonly ApplicationDbContext _db;

    public CouponService(ApplicationDbContext db) => _db = db;

    public async Task<CouponValidationResult> ValidateAsync(string? code, decimal subtotal)
    {
        if (string.IsNullOrWhiteSpace(code))
            return CouponValidationResult.Invalid("Informe um cupom.");

        var normalized = code.Trim().ToUpperInvariant();
        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == normalized);
        if (coupon is null)
            return CouponValidationResult.Invalid("Cupom não encontrado.");

        var now = DateTime.UtcNow;
        if (!coupon.IsActive)
            return CouponValidationResult.Invalid("Este cupom não está mais ativo.");
        if (coupon.StartsAt.HasValue && coupon.StartsAt.Value > now)
            return CouponValidationResult.Invalid("Este cupom ainda não está válido.");
        if (coupon.EndsAt.HasValue && coupon.EndsAt.Value < now)
            return CouponValidationResult.Invalid("Este cupom expirou.");
        if (coupon.MaxUses.HasValue && coupon.UsedCount >= coupon.MaxUses.Value)
            return CouponValidationResult.Invalid("Este cupom atingiu o limite de utilizações.");
        if (subtotal < coupon.MinimumOrder)
            return CouponValidationResult.Invalid($"Este cupom exige pedido mínimo de {coupon.MinimumOrder:C}.");

        var discount = await CalculateDiscountAsync(coupon, subtotal);
        return new CouponValidationResult { IsValid = true, Coupon = coupon, Discount = discount };
    }

    public Task<decimal> CalculateDiscountAsync(Coupon coupon, decimal subtotal)
    {
        var discount = coupon.DiscountType == DiscountType.Percentage
            ? Math.Round(subtotal * (coupon.Value / 100m), 2)
            : coupon.Value;
        discount = Math.Min(discount, subtotal);
        return Task.FromResult(discount);
    }
}
