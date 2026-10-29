using JLM.Models;

namespace JLM.ViewModels;

public class HomeViewModel
{
    public IReadOnlyList<Product> FeaturedProducts { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<Product> NewArrivals { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<Product> BestSellers { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();
    public IReadOnlyList<Collection> Collections { get; set; } = Array.Empty<Collection>();
    public int ProductCount { get; set; }
}

public class CatalogViewModel
{
    public IReadOnlyList<Product> Products { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();
    public IReadOnlyList<Collection> Collections { get; set; } = Array.Empty<Collection>();
    public IReadOnlyList<string> AvailableSizes { get; set; } = Array.Empty<string>();

    public string? Search { get; set; }
    public string? CategorySlug { get; set; }
    public string? CollectionSlug { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Size { get; set; }
    public string Sort { get; set; } = "relevance";

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 9;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public string Title { get; set; } = "Coleção completa";
    public string? Eyebrow { get; set; } = "JLM Store";
    public string? Description { get; set; }
    public string? HeroImage { get; set; }
    public string? ActiveCategoryName { get; set; }
    public string? ActiveCollectionName { get; set; }

    public bool HasFilters => !string.IsNullOrWhiteSpace(Search) || !string.IsNullOrWhiteSpace(CategorySlug)
        || !string.IsNullOrWhiteSpace(CollectionSlug) || MinPrice.HasValue || MaxPrice.HasValue || !string.IsNullOrWhiteSpace(Size);
}

public class PaginationViewModel
{
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
}

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = new();
    public IReadOnlyList<Product> RelatedProducts { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<Product> CompleteTheLook { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<string> Gallery { get; set; } = Array.Empty<string>();
    public bool IsWishlisted { get; set; }
}

public class CartLine
{
    public string Key { get; set; } = string.Empty;
    public Product Product { get; set; } = new();
    public string? Size { get; set; }
    public string? Color { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice => Product.Price;
    public decimal LineTotal => UnitPrice * Quantity;
    public int AvailableStock => Product.Stock;
}

public class CartViewModel
{
    public IReadOnlyList<CartLine> Items { get; set; } = Array.Empty<CartLine>();
    public int ItemCount => Items.Sum(i => i.Quantity);
    public decimal Subtotal => Items.Sum(i => i.LineTotal);
    public decimal ShippingEstimate => Subtotal >= 500 || Subtotal == 0 ? 0 : 29.90m;
    public decimal Discount { get; set; }
    public decimal Total => Math.Max(0, Subtotal + ShippingEstimate - Discount);
    public Coupon? Coupon { get; set; }
    public string? CouponError { get; set; }
    public string? CouponCode { get; set; }
    public decimal FreeShippingThreshold => 500m;
    public decimal RemainingForFreeShipping => Math.Max(0, FreeShippingThreshold - Subtotal);
    public bool IsEmpty => Items.Count == 0;
}

public class CheckoutViewModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe o nome de quem receberá o pedido.")]
    [System.ComponentModel.DataAnnotations.MaxLength(120)]
    public string CustomerName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe o e-mail.")]
    [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [System.ComponentModel.DataAnnotations.MaxLength(160)]
    public string Email { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe o telefone para contato.")]
    [System.ComponentModel.DataAnnotations.MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string? Cpf { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe o CEP.")]
    [System.ComponentModel.DataAnnotations.MaxLength(10)]
    public string ShippingZipCode { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe o endereço.")]
    [System.ComponentModel.DataAnnotations.MaxLength(180)]
    public string ShippingAddress { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe o número.")]
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string ShippingNumber { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(80)]
    public string? ShippingComplement { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe o bairro.")]
    [System.ComponentModel.DataAnnotations.MaxLength(80)]
    public string ShippingNeighborhood { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe a cidade.")]
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string ShippingCity { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe o estado.")]
    [System.ComponentModel.DataAnnotations.StringLength(2, MinimumLength = 2, ErrorMessage = "Use a sigla do estado (ex.: SP).")]
    public string ShippingState { get; set; } = string.Empty;

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Pix;
    public ShippingMethod ShippingMethod { get; set; } = ShippingMethod.Standard;
    public string? CouponCode { get; set; }
    public string? Notes { get; set; }

    public IReadOnlyList<CartLine> Items { get; set; } = Array.Empty<CartLine>();
    public Coupon? Coupon { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
}

public class CheckoutResult
{
    public bool Success { get; init; }
    public Order? Order { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static CheckoutResult Ok(Order order) => new() { Success = true, Order = order };
    public static CheckoutResult Fail(params string[] errors) => new() { Success = false, Errors = errors };
}

public class ContactViewModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe seu nome.")]
    [System.ComponentModel.DataAnnotations.MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe seu e-mail.")]
    [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [System.ComponentModel.DataAnnotations.MaxLength(160)]
    public string Email { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(30)]
    public string? Phone { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Selecione um assunto.")]
    [System.ComponentModel.DataAnnotations.MaxLength(120)]
    public string Subject { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Escreva sua mensagem.")]
    [System.ComponentModel.DataAnnotations.MinLength(10, ErrorMessage = "A mensagem deve ter pelo menos 10 caracteres.")]
    [System.ComponentModel.DataAnnotations.MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
}

public class NewsletterViewModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe seu e-mail.")]
    [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;
}
