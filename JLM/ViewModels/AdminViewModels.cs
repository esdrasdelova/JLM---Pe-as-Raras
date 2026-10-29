using JLM.Models;

namespace JLM.ViewModels.Admin;

public class DashboardViewModel
{
    public decimal Revenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public int OrdersCount { get; set; }
    public int OrdersThisMonth { get; set; }
    public int CustomersCount { get; set; }
    public int ProductsCount { get; set; }
    public int LowStockCount { get; set; }
    public int PendingOrders { get; set; }
    public int UnreadMessages { get; set; }
    public decimal AverageTicket { get; set; }
    public IReadOnlyList<Order> RecentOrders { get; set; } = Array.Empty<Order>();
    public IReadOnlyList<Product> LowStockProducts { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<TopProductItem> TopProducts { get; set; } = Array.Empty<TopProductItem>();
    public IReadOnlyList<MonthlySalesItem> MonthlySales { get; set; } = Array.Empty<MonthlySalesItem>();
    public IReadOnlyDictionary<OrderStatus, int> StatusCounts { get; set; } = new Dictionary<OrderStatus, int>();
}

public class TopProductItem
{
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
}

public class MonthlySalesItem
{
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}

public class ProductFormViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Composition { get; set; }
    public string? CareInstructions { get; set; }
    public decimal Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public int? CollectionId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Sizes { get; set; } = "P,M,G,GG";
    public string Colors { get; set; } = "Preto:#101010";
    public bool IsFeatured { get; set; }
    public bool IsNewArrival { get; set; }
    public bool IsBestSeller { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal Rating { get; set; } = 4.9m;
    public int ReviewCount { get; set; }
    public IFormFile? MainImageFile { get; set; }
    public string? GalleryUrls { get; set; }
    public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();
    public IReadOnlyList<Collection> Collections { get; set; } = Array.Empty<Collection>();
    public bool IsEdit => Id > 0;
}

public class AdminOrdersViewModel
{
    public IReadOnlyList<Order> Orders { get; set; } = Array.Empty<Order>();
    public string? Search { get; set; }
    public OrderStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class AdminCustomersViewModel
{
    public IReadOnlyList<User> Customers { get; set; } = Array.Empty<User>();
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class AdminMessagesViewModel
{
    public IReadOnlyList<ContactMessage> Messages { get; set; } = Array.Empty<ContactMessage>();
    public string? Filter { get; set; }
    public int UnreadCount { get; set; }
}
