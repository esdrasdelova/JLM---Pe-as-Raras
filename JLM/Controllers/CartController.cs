using JLM.Helpers;
using JLM.Services;
using Microsoft.AspNetCore.Mvc;

namespace JLM.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cart;
    private readonly ICouponService _coupons;

    public CartController(ICartService cart, ICouponService coupons)
    {
        _cart = cart;
        _coupons = coupons;
    }

    [HttpGet("/carrinho")]
    public async Task<IActionResult> Index()
    {
        var model = await _cart.GetCartAsync();
        return View(model);
    }

    [HttpPost("/carrinho/adicionar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, string? size, string? color, int quantity = 1, string? returnUrl = null)
    {
        var isAjax = IsAjax();
        try
        {
            await _cart.AddAsync(productId, size, color, quantity);
            var cart = await _cart.GetCartAsync();

            if (isAjax)
                return Json(new
                {
                    success = true,
                    message = "Peça adicionada ao carrinho.",
                    count = cart.ItemCount,
                    subtotal = cart.Subtotal.ToBrl()
                });

            TempData["Success"] = "Peça adicionada ao carrinho.";
            return LocalRedirect(SafeReturnUrl(returnUrl, "/carrinho"));
        }
        catch (InvalidOperationException ex)
        {
            if (isAjax)
                return BadRequest(new { success = false, message = ex.Message });

            TempData["Error"] = ex.Message;
            return LocalRedirect(SafeReturnUrl(returnUrl, "/carrinho"));
        }
    }

    [HttpPost("/carrinho/atualizar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string key, int quantity)
    {
        await _cart.UpdateAsync(key, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/carrinho/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(string key)
    {
        await _cart.RemoveAsync(key);
        TempData["Info"] = "Peça removida do carrinho.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/carrinho/limpar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        await _cart.ClearAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/carrinho/cupom")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyCoupon(string code, string? returnUrl)
    {
        var cart = await _cart.GetCartAsync();
        var validation = await _coupons.ValidateAsync(code, cart.Subtotal);
        if (!validation.IsValid)
        {
            TempData["CouponError"] = validation.Error;
            await _cart.SetCouponAsync(null);
        }
        else
        {
            await _cart.SetCouponAsync(validation.Coupon!.Code);
            TempData["CouponSuccess"] = $"Cupom {validation.Coupon.Code} aplicado: {validation.Discount.ToBrl()} de desconto.";
        }

        return LocalRedirect(SafeReturnUrl(returnUrl, "/carrinho"));
    }

    [HttpPost("/carrinho/cupom/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveCoupon(string? returnUrl)
    {
        await _cart.SetCouponAsync(null);
        TempData["Info"] = "Cupom removido.";
        return LocalRedirect(SafeReturnUrl(returnUrl, "/carrinho"));
    }

    [HttpGet("/carrinho/contador")]
    public async Task<IActionResult> Count()
    {
        var count = await _cart.GetCountAsync();
        var subtotal = await _cart.GetSubtotalAsync();
        return Json(new { count, subtotal = subtotal.ToBrl() });
    }

    private bool IsAjax() =>
        string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase) ||
        (Request.Headers.Accept.ToString()?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false);

    private string SafeReturnUrl(string? returnUrl, string fallback) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : fallback;
}
