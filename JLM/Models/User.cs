using System.ComponentModel.DataAnnotations;

namespace JLM.Models;

public class User
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(160)]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(20)]
    public string? Cpf { get; set; }

    [MaxLength(20)]
    public string Role { get; set; } = "Customer";

    [MaxLength(180)]
    public string? Address { get; set; }

    [MaxLength(20)]
    public string? AddressNumber { get; set; }

    [MaxLength(80)]
    public string? Complement { get; set; }

    [MaxLength(80)]
    public string? Neighborhood { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(2)]
    public string? State { get; set; }

    [MaxLength(10)]
    public string? ZipCode { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
