using WarehouseOps.Domain.Enums;

namespace WarehouseOps.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderedAt { get; set; }
    public DateTime? FulfilledAt { get; set; }
    public OrderStatus Status { get; set; }

    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
}

public class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int QuantityOrdered { get; set; }
    public int QuantityFulfilled { get; set; }
    public decimal UnitPriceAtOrder { get; set; }
}
