namespace ECommerce.Api.Domain;

public class Product
{
    public const int MaxNameLength = 200;
    public const int MaxSkuLength = 64;
    public const int MaxBrandLength = 100;

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string Brand { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Product() { }

    public void UpdateDetails(string name, string sku, string brand, decimal price, int stockQuantity)
    {
        this.Name = name.Trim();
        this.Sku = sku.Trim().ToUpperInvariant();
        this.Brand = brand.Trim();
        this.Price = price;
        this.StockQuantity = stockQuantity;
        this.UpdatedAtUtc = DateTime.UtcNow;
    }
    public static Product Create(string name, string sku, string brand, decimal price, int stockQuantity)  => new()
    {
        Name = name.Trim(),
        Sku = sku.Trim().ToUpperInvariant(),
        Brand = brand.Trim(),
        Price = price,
        StockQuantity = stockQuantity,
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow
    };
    
}
