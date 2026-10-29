using System.Security.Claims;
using JLM.Data;
using JLM.Helpers;
using JLM.Models;
using JLM.Services;
using JLM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JLM.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ICartService _cart;
    private readonly IOrderService _orders;
    private readonly ApplicationDbContext _db;

    public CheckoutController(ICartService cart, IOrderService orders, ApplicationDbContext db)
    {
        _cart = cart;
        _orders = orders;
        _db = db;
    }

    [HttpGet("/checkout")]
    public async Task<IActionResult> Index()
    {
        var cart = await _cart.GetCartAsync();
        if (cart.IsEmpty)
        {
            TempData["Info"] = "Seu carrinho está vazio. Encontre sua próxima peça na coleção.";
            return Redirect("/carrinho");
        }

        var user = await CurrentUserAsync();
        if (user is null) return Challenge();

        var model = BuildModel(user, cart, new CheckoutViewModel());
        return View(model);
    }

    [HttpPost("/checkout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        var cart = await _cart.GetCartAsync();
        if (cart.IsEmpty)
        {
            TempData["Info"] = "Seu carrinho está vazio.";
            return Redirect("/carrinho");
        }

        var user = await CurrentUserAsync();
        if (user is null) return Challenge();

        if (!ModelState.IsValid)
            return View(BuildModel(user, cart, model));

        var result = await _orders.PlaceOrderAsync(user, model);
        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);
            return View(BuildModel(user, cart, model));
        }

        TempData["Success"] = "Pedido confirmado. Obrigado por escolher a JLM.";
        return Redirect($"/checkout/sucesso/{result.Order!.OrderNumber}");
    }

    [HttpGet("/checkout/sucesso/{orderNumber}")]
    public async Task<IActionResult> Success(string orderNumber)
    {
        var user = await CurrentUserAsync();
        if (user is null) return Challenge();

        var order = await _orders.GetOrderAsync(orderNumber, user.Id);
        if (order is null) return NotFound();
        return View(order);
    }

    private async Task<User?> CurrentUserAsync()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var userId)
            ? await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            : null;
    }

    private CheckoutViewModel BuildModel(User user, CartViewModel cart, CheckoutViewModel model)
    {
        model.CustomerName = string.IsNullOrWhiteSpace(model.CustomerName) ? user.FullName : model.CustomerName;
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? user.Email : model.Email;
        model.Phone = string.IsNullOrWhiteSpace(model.Phone) ? user.Phone ?? string.Empty : model.Phone;
        model.Cpf = string.IsNullOrWhiteSpace(model.Cpf) ? user.Cpf : model.Cpf;
        model.ShippingZipCode = string.IsNullOrWhiteSpace(model.ShippingZipCode) ? user.ZipCode ?? string.Empty : model.ShippingZipCode;
        model.ShippingAddress = string.IsNullOrWhiteSpace(model.ShippingAddress) ? user.Address ?? string.Empty : model.ShippingAddress;
        model.ShippingNumber = string.IsNullOrWhiteSpace(model.ShippingNumber) ? user.AddressNumber ?? string.Empty : model.ShippingNumber;
        model.ShippingComplement = string.IsNullOrWhiteSpace(model.ShippingComplement) ? user.Complement : model.ShippingComplement;
        model.ShippingNeighborhood = string.IsNullOrWhiteSpace(model.ShippingNeighborhood) ? user.Neighborhood ?? string.Empty : model.ShippingNeighborhood;
        model.ShippingCity = string.IsNullOrWhiteSpace(model.ShippingCity) ? user.City ?? string.Empty : model.ShippingCity;
        model.ShippingState = string.IsNullOrWhiteSpace(model.ShippingState) ? user.State ?? string.Empty : model.ShippingState;
        model.CouponCode ??= cart.CouponCode;

        model.Items = cart.Items;
        model.Subtotal = cart.Subtotal;
        model.Discount = cart.Discount;
        model.Coupon = cart.Coupon;
        model.ItemCount = cart.ItemCount;

        model.ShippingCost = model.ShippingMethod switch
        {
            ShippingMethod.Express => cart.Subtotal >= 900m ? 0 : 49.90m,
            _ => cart.Subtotal >= 500m ? 0 : 29.90m
        };
        model.Total = Math.Max(0, model.Subtotal + model.ShippingCost - model.Discount);
        return model;
    }
}
