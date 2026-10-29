using JLM.Services;
using JLM.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace JLM.Controllers;

public class ShopController : Controller
{
    private readonly ICatalogService _catalog;
    private readonly IWishlistService _wishlist;

    public ShopController(ICatalogService catalog, IWishlistService wishlist)
    {
        _catalog = catalog;
        _wishlist = wishlist;
    }

    [HttpGet("/catalogo")]
    public Task<IActionResult> Index(string? q, string? colecao, decimal? precoMin, decimal? precoMax,
        string? tamanho, string? ordenar = "relevance", int pagina = 1)
        => Catalog(null, q, colecao, precoMin, precoMax, tamanho, ordenar, pagina,
            "Coleção completa", "JLM Store", "Todas as peças da JLM em um só lugar. Vestuário, joias e acessórios com a assinatura da marca.",
            "/images/products/blazer-premium-gold-line.svg");

    [HttpGet("/roupas")]
    public Task<IActionResult> Roupas(string? q, string? colecao, decimal? precoMin, decimal? precoMax,
        string? tamanho, string? ordenar = "relevance", int pagina = 1)
        => Catalog("roupas", q, colecao, precoMin, precoMax, tamanho, ordenar, pagina,
            "Roupas masculinas", "Vestuário JLM", "Cortes precisos, tecidos nobres e silhuetas que impõem presença. Do essencial ao statement piece.",
            "/images/products/moletom-oversized-presenca.svg");

    [HttpGet("/joias")]
    public Task<IActionResult> Joias(string? q, string? colecao, decimal? precoMin, decimal? precoMax,
        string? tamanho, string? ordenar = "relevance", int pagina = 1)
        => Catalog("joias", q, colecao, precoMin, precoMax, tamanho, ordenar, pagina,
            "Joias masculinas", "Alta joalheria", "Ouro 18k, prata 925 e pedras naturais. Peças de acabamento impecável para assinar o seu estilo.",
            "/images/products/corrente-cubana-ouro-18k.svg");

    [HttpGet("/acessorios")]
    public Task<IActionResult> Acessorios(string? q, string? colecao, decimal? precoMin, decimal? precoMax,
        string? tamanho, string? ordenar = "relevance", int pagina = 1)
        => Catalog("acessorios", q, colecao, precoMin, precoMax, tamanho, ordenar, pagina,
            "Acessórios", "O detalhe final", "Óculos, relógios, couro e calçados. O detalhe que transforma um look em assinatura.",
            "/images/products/oculos-de-sol-laumir-gold.svg");

    [HttpGet("/busca")]
    public Task<IActionResult> Search(string? q, string? colecao, decimal? precoMin, decimal? precoMax,
        string? tamanho, string? ordenar = "relevance", int pagina = 1)
        => Catalog(null, q, colecao, precoMin, precoMax, tamanho, ordenar, pagina,
            string.IsNullOrWhiteSpace(q) ? "Busca" : $"Resultados para “{q.Trim()}”",
            "Busca JLM",
            "Encontre a peça que traduz o seu momento. Busque por nome, categoria ou detalhe.",
            "/images/products/camisa-linho-noir.svg");

    [HttpGet("/produto/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var product = await _catalog.GetBySlugAsync(slug);
        if (product is null) return NotFound();

        var related = await _catalog.GetRelatedAsync(product);
        var look = await _catalog.GetCompleteTheLookAsync(product);
        var gallery = new List<string> { product.ImageUrl };
        gallery.AddRange(product.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl));
        gallery = gallery.Where(g => !string.IsNullOrWhiteSpace(g)).Distinct().ToList();

        var model = new ProductDetailsViewModel
        {
            Product = product,
            RelatedProducts = related,
            CompleteTheLook = look,
            Gallery = gallery,
            IsWishlisted = await _wishlist.ContainsAsync(product.Id)
        };

        return View(model);
    }

    private async Task<IActionResult> Catalog(string? categorySlug, string? q, string? colecao,
        decimal? precoMin, decimal? precoMax, string? tamanho, string? ordenar, int pagina,
        string title, string eyebrow, string description, string heroImage)
    {
        var filters = new CatalogViewModel
        {
            CategorySlug = categorySlug,
            Search = q,
            CollectionSlug = colecao,
            MinPrice = precoMin,
            MaxPrice = precoMax,
            Size = tamanho,
            Sort = string.IsNullOrWhiteSpace(ordenar) ? "relevance" : ordenar,
            Page = pagina,
            PageSize = 9,
            Title = title,
            Eyebrow = eyebrow,
            Description = description,
            HeroImage = heroImage
        };

        var model = await _catalog.GetCatalogAsync(filters);

        if (!string.IsNullOrWhiteSpace(model.ActiveCategoryName) && categorySlug is null)
            model.Title = model.ActiveCategoryName;

        return View("~/Views/Shop/Catalog.cshtml", model);
    }
}
