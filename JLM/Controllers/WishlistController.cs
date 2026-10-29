using JLM.Services;
using Microsoft.AspNetCore.Mvc;

namespace JLM.Controllers;

public class WishlistController : Controller
{
    private readonly IWishlistService _wishlist;

    public WishlistController(IWishlistService wishlist) => _wishlist = wishlist;

    [HttpGet("/favoritos")]
    public async Task<IActionResult> Index()
    {
        var products = await _wishlist.GetProductsAsync();
        return View(products);
    }

    [HttpPost("/favoritos/alternar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int productId, string? returnUrl = null)
    {
        var added = await _wishlist.ToggleAsync(productId);
        var count = await _wishlist.GetCountAsync();

        if (IsAjax())
            return Json(new
            {
                success = true,
                added,
                count,
                message = added ? "Peça salva nos favoritos." : "Peça removida dos favoritos."
            });

        TempData[added ? "Success" : "Info"] = added ? "Peça salva nos favoritos." : "Peça removida dos favoritos.";
        return LocalRedirect(SafeReturnUrl(returnUrl, "/favoritos"));
    }

    [HttpPost("/favoritos/limpar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        await _wishlist.ClearAsync();
        TempData["Info"] = "Sua lista de favoritos foi limpa.";
        return RedirectToAction(nameof(Index));
    }

    private bool IsAjax() =>
        string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase) ||
        (Request.Headers.Accept.ToString()?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false);

    private string SafeReturnUrl(string? returnUrl, string fallback) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : fallback;
}
