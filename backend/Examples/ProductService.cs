using BondTradingApi.Models;
using BondTradingApi.Services;

namespace BondTradingApi.Examples;

// Example entity - completely different from Bond
public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Brand { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    public string Version { get; set; } = "v1";
}

// Example row model for AG Grid
public class ProductRow
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
    public string? Brand { get; set; }
    public DateTime? CreatedDate { get; set; }
    public bool? IsActive { get; set; }
    public string? Version { get; set; }
    public bool IsGroup { get; set; }
}

// Example service implementation
public class ProductService : GenericGridService<Product>
{
    private readonly List<Product> _products;

    public ProductService()
    {
        _products = GenerateMockProducts(100);
    }

    protected override List<Product> GetAllData()
    {
        return _products;
    }

    protected override object ConvertToGridRow(Product item)
    {
        return new ProductRow
        {
            Id = item.Id,
            Name = item.Name,
            Category = item.Category,
            Price = item.Price,
            Stock = item.Stock,
            Brand = item.Brand,
            CreatedDate = item.CreatedDate,
            IsActive = item.IsActive,
            Version = item.Version,
            IsGroup = item.Version == "v1" // Only v1 products are expandable
        };
    }

    protected override bool IsGroupItem(Product item)
    {
        return item.Version == "v1"; // Only v1 products are shown as parent items
    }

    protected override List<Product> GetChildItems(string parentKey)
    {
        // Return product versions (v2, v3, etc.) for the given product
        var mainProduct = _products.FirstOrDefault(p => p.Id == parentKey && p.Version == "v1");
        if (mainProduct == null) return new List<Product>();

        return _products
            .Where(p => p.Name == mainProduct.Name && p.Version != "v1")
            .ToList();
    }

    private List<Product> GenerateMockProducts(int count)
    {
        var random = new Random();
        var categories = new[] { "Electronics", "Clothing", "Books", "Home", "Sports" };
        var brands = new[] { "BrandA", "BrandB", "BrandC", "BrandD", "BrandE" };
        var versions = new[] { "v1", "v2", "v3" };
        
        var products = new List<Product>();

        for (int i = 0; i < count; i++)
        {
            var productName = $"Product {i + 1}";
            
            // Create multiple versions of each product
            foreach (var version in versions)
            {
                products.Add(new Product
                {
                    Id = $"PROD{i:D3}",
                    Name = productName,
                    Category = categories[random.Next(categories.Length)],
                    Price = random.Next(10, 1000) + (decimal)random.NextDouble(),
                    Stock = random.Next(0, 100),
                    Brand = brands[random.Next(brands.Length)],
                    CreatedDate = DateTime.Today.AddDays(-random.Next(365)),
                    IsActive = random.Next(2) == 1,
                    Version = version
                });
            }
        }

        return products;
    }
}

// Example of how to use it in a controller:
/*
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpPost("rows")]
    public async Task<ServerSideResponse> GetRows([FromBody] ServerSideRequest request)
    {
        return await _productService.GetRowsAsync(request);
    }
}
*/ 