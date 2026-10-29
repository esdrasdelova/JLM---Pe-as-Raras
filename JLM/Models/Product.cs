using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JLM.Models;

public class Product
{
    public int Id { get; set; }

    [MaxLength(160)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(180)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(280)]
    public string ShortDescription { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Composition { get; set; }

    public string? CareInstructions { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? ComparePrice { get; set; }

    [MaxLength(40)]
    public string Sku { get; set; } = string.Empty;

    public int Stock { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? CollectionId { get; set; }
    public Collection? Collection { get; set; }

    [MaxLength(300)]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Tamanhos separados por vírgula. Ex.: P,M,G,GG</summary>
    [MaxLength(120)]
    public string Sizes { get; set; } = string.Empty;

    /// <summary>Cores no formato Nome:#hex separadas por vírgula. Ex.: Preto:#101010,Grafite:#292929</summary>
    [MaxLength(300)]
    public string Colors { get; set; } = string.Empty;

    public bool IsFeatured { get; set; }
    public bool IsNewArrival { get; set; }
    public bool IsBestSeller { get; set; }
    public bool IsActive { get; set; } = true;

    [Column(TypeName = "decimal(3,2)")]
    public decimal Rating { get; set; } = 4.9m;

    public int ReviewCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [NotMapped]
    public string[] SizeList => Sizes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    [NotMapped]
    public IEnumerable<(string Name, string Hex)> ColorList =>
        Colors.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(c => c.Split(':', 2))
            .Where(parts => parts.Length == 2)
            .Select(parts => (parts[0].Trim(), parts[1].Trim()));

    [NotMapped]
    public bool HasDiscount => ComparePrice.HasValue && ComparePrice.Value > Price;

    [NotMapped]
    public int DiscountPercent => HasDiscount
        ? (int)Math.Round((1 - (Price / ComparePrice!.Value)) * 100)
        : 0;
}

public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [MaxLength(300)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? AltText { get; set; }

    public int SortOrder { get; set; }
}
