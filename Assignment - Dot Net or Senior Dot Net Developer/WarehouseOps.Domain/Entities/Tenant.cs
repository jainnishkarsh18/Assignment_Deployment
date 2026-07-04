namespace WarehouseOps.Domain.Entities;

public class Tenant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;   // e.g. "warehouse-a", used in request header
    public bool IsActive { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
