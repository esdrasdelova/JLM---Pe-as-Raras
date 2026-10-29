using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JLM.Models;

public enum OrderStatus
{
    Pending = 0,
    Paid = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}

public enum PaymentMethod
{
    Pix = 0,
    CreditCard = 1,
    Boleto = 2
}

public enum ShippingMethod
{
    Standard = 0,
    Express = 1
}

public enum DiscountType
{
    Percentage = 0,
    Fixed = 1
}

public class Order
{
    public int Id { get; set; }

    [MaxLength(30)]
    public string OrderNumber { get; set; } = string.Empty;

    public int? UserId { get; set; }
    public User? User { get; set; }

    [MaxLength(120)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(160)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Cpf { get; set; }

    [MaxLength(10)]
    public string ShippingZipCode { get; set; } = string.Empty;

    [MaxLength(180)]
    public string ShippingAddress { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ShippingNumber { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? ShippingComplement { get; set; }

    [MaxLength(80)]
    public string ShippingNeighborhood { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ShippingCity { get; set; } = string.Empty;

    [MaxLength(2)]
    public string ShippingState { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal ShippingCost { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Discount { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Total { get; set; }

    [MaxLength(30)]
    public string? CouponCode { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public ShippingMethod ShippingMethod { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    [NotMapped]
    public string StatusLabel => Status switch
    {
        OrderStatus.Pending => "Aguardando pagamento",
        OrderStatus.Paid => "Pagamento aprovado",
        OrderStatus.Processing => "Em preparação",
        OrderStatus.Shipped => "Enviado",
        OrderStatus.Delivered => "Entregue",
        OrderStatus.Cancelled => "Cancelado",
        _ => Status.ToString()
    };

    [NotMapped]
    public string PaymentLabel => PaymentMethod switch
    {
        PaymentMethod.Pix => "Pix",
        PaymentMethod.CreditCard => "Cartão de crédito",
        PaymentMethod.Boleto => "Boleto",
        _ => PaymentMethod.ToString()
    };
}

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    [MaxLength(160)]
    public string ProductName { get; set; } = string.Empty;

    [MaxLength(300)]
    public string ProductImageUrl { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Size { get; set; }

    [MaxLength(40)]
    public string? Color { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal LineTotal { get; set; }
}
