using System.ComponentModel.DataAnnotations;

namespace Shop.Products.Domain.Models;

public class Product
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }

    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}