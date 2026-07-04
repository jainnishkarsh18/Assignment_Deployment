using WarehouseOps.Domain.Enums;

namespace WarehouseOps.Domain.Entities;

public class StockAlert
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
