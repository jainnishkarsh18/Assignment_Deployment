using WarehouseOps.Domain.Enums;

namespace WarehouseOps.Domain.Entities;

public class StockMovement
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public string Reference { get; set; } = string.Empty;  // e.g. PO number, order ID
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
}
