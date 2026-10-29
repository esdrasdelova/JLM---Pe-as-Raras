using System.Security.Claims;
using JLM.Data;
using JLM.Helpers;
using JLM.Models;
using JLM.Services;
using JLM.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JLM.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IOrderService _orders;

    public AccountController(ApplicationDbContext db, IPasswordHasher<User> hasher, IOrderService orders)
    {
        _db = db;
        _hasher = hasher;
        _orders = orders;
    }

    [HttpGet("/conta/entrar")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction(nameof(Index));
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("/conta/entrar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha incorretos.");
            return View(model);
        }

        var verification = _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha incorretos.");
            return View(model);
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, model.Password);
            await _db.SaveChangesAsync();
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Esta conta está desativada. Fale com o atendimento JLM.");
            return View(model);
        }

        await SignInAsync(user, model.RememberMe);
        TempData["Success"] = $"Bem-vindo de volta, {user.FullName.Split(' ')[0]}.";

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        return user.Role == "Admin" ? Redirect("/admin") : RedirectToAction(nameof(Index));
    }

    [HttpGet("/conta/cadastrar")]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction(nameof(Index));
        return View(new RegisterViewModel());
    }

    [HttpPost("/conta/cadastrar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!model.AcceptTerms)
            ModelState.AddModelError(nameof(model.AcceptTerms), "É necessário aceitar os termos e a política de privacidade.");

        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError(nameof(model.Email), "Este e-mail já está cadastrado. Faça login.");
            return View(model);
        }

        var user = new User
        {
            FullName = model.FullName.Trim(),
            Email = email,
            Phone = model.Phone?.Trim(),
            Role = "Customer",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _hasher.HashPassword(user, model.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await SignInAsync(user, true);
        TempData["Success"] = "Sua conta JLM foi criada. Bem-vindo à presença.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/conta/sair")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Info"] = "Você saiu da sua conta.";
        return Redirect("/");
    }

    [HttpGet("/conta")]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Challenge();

        var orders = await _orders.GetUserOrdersAsync(user.Id);
        var model = MapProfile(user);
        model.OrdersCount = orders.Count;
        model.TotalSpent = orders.Where(o => o.Status != OrderStatus.Cancelled).Sum(o => o.Total);
        ViewBag.RecentOrders = orders.Take(3).ToList();
        return View(model);
    }

    [HttpGet("/conta/editar")]
    [Authorize]
    public async Task<IActionResult> Edit()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Challenge();
        return View(MapProfile(user));
    }

    [HttpPost("/conta/editar")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await GetCurrentUserAsync();
        if (user is null) return Challenge();

        var email = model.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email && u.Id != user.Id))
        {
            ModelState.AddModelError(nameof(model.Email), "Este e-mail já está em uso por outra conta.");
            return View(model);
        }

        user.FullName = model.FullName.Trim();
        user.Email = email;
        user.Phone = model.Phone?.Trim();
        user.Cpf = model.Cpf?.Trim();
        user.Address = model.Address?.Trim();
        user.AddressNumber = model.AddressNumber?.Trim();
        user.Complement = model.Complement?.Trim();
        user.Neighborhood = model.Neighborhood?.Trim();
        user.City = model.City?.Trim();
        user.State = model.State?.Trim().ToUpperInvariant();
        user.ZipCode = model.ZipCode?.Trim();

        await _db.SaveChangesAsync();
        await SignInAsync(user, true);
        TempData["Success"] = "Seus dados foram atualizados.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/conta/senha")]
    [Authorize]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost("/conta/senha")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await GetCurrentUserAsync();
        if (user is null) return Challenge();

        var verification = _hasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword);
        if (verification == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "A senha atual está incorreta.");
            return View(model);
        }

        user.PasswordHash = _hasher.HashPassword(user, model.NewPassword);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Sua senha foi alterada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/conta/pedidos")]
    [Authorize]
    public async Task<IActionResult> Orders()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Challenge();
        var orders = await _orders.GetUserOrdersAsync(user.Id);
        return View(orders);
    }

    [HttpGet("/conta/pedidos/{orderNumber}")]
    [Authorize]
    public async Task<IActionResult> OrderDetails(string orderNumber)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Challenge();
        var order = await _orders.GetOrderAsync(orderNumber, user.Id);
        if (order is null) return NotFound();
        return View(order);
    }

    [HttpPost("/conta/pedidos/{orderNumber}/cancelar")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> CancelOrder(string orderNumber)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Challenge();

        if (await _orders.CancelOrderAsync(orderNumber, user.Id))
            TempData["Success"] = "Pedido cancelado. O estorno será processado conforme a forma de pagamento.";
        else
            TempData["Error"] = "Este pedido não pode mais ser cancelado. Fale com o atendimento.";

        return RedirectToAction(nameof(OrderDetails), new { orderNumber });
    }

    [HttpGet("/conta/acesso-negado")]
    public IActionResult AccessDenied() => View();

    private async Task<User?> GetCurrentUserAsync()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var userId)
            ? await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            : null;
    }

    private async Task SignInAsync(User user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(isPersistent ? 7 : 1)
            });
    }

    private static ProfileViewModel MapProfile(User user) => new()
    {
        FullName = user.FullName,
        Email = user.Email,
        Phone = user.Phone,
        Cpf = user.Cpf,
        Address = user.Address,
        AddressNumber = user.AddressNumber,
        Complement = user.Complement,
        Neighborhood = user.Neighborhood,
        City = user.City,
        State = user.State,
        ZipCode = user.ZipCode,
        CreatedAt = user.CreatedAt
    };
}
