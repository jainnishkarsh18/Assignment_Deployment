namespace WarehouseOps.Application.DTOs;

public record ProductDto(
    int Id,
    string SKU,
    string Name,
    string Category,
    int CurrentStock,
    int ReorderLevel,
    decimal UnitPrice,
    bool IsActive,
    bool IsBelowReorder
);

public record StockMovementDto(
    int Id,
    string ProductName,
    string SKU,
    string Type,
    int Quantity,
    string Reference,
    string CreatedBy,
    DateTime CreatedAt,
    string? Notes
);

public record RecordMovementDto(
    int ProductId,
    string Type,       // "Inbound" | "Outbound" | "Adjustment"
    int Quantity,
    string Reference,
    string? Notes
);

public record OrderDto(
    int Id,
    string OrderNumber,
    string CustomerName,
    DateTime OrderedAt,
    DateTime? FulfilledAt,
    string Status,
    decimal TotalValue,
    int TotalLines,
    int FulfilledLines
);

public record OrderDetailDto(
    int Id,
    string OrderNumber,
    string CustomerName,
    DateTime OrderedAt,
    string Status,
    IEnumerable<OrderLineDto> Lines
);

public record OrderLineDto(
    int ProductId,
    string ProductName,
    string SKU,
    int QuantityOrdered,
    int QuantityFulfilled,
    decimal UnitPriceAtOrder,
    bool IsFullyFulfilled
);

public record FulfillOrderDto(int OrderId);

public record DashboardDto(
    int TotalProducts,
    int BelowReorderCount,
    int PendingOrders,
    int OverdueOrders,
    decimal TotalStockValue,
    IEnumerable<CategoryStockDto> StockByCategory,
    IEnumerable<TopMovedProductDto> TopMovedProducts
);

public record CategoryStockDto(string Category, int ProductCount, decimal TotalValue);

public record TopMovedProductDto(string ProductName, string SKU, int TotalOutbound);
