using System.ComponentModel.DataAnnotations;

namespace JLM.Models;

public class Category
{
    public int Id { get; set; }

    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(400)]
    public string? Description { get; set; }

    [MaxLength(300)]
    public string? ImageUrl { get; set; }

    [MaxLength(60)]
    public string? Eyebrow { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Collection
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? Tagline { get; set; }

    [MaxLength(1200)]
    public string? Description { get; set; }

    [MaxLength(300)]
    public string? ImageUrl { get; set; }

    public bool IsFeatured { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
