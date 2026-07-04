using WarehouseOps.Application.Common;
using WarehouseOps.Application.DTOs;
using WarehouseOps.Application.Interfaces;
using WarehouseOps.Domain.Enums;
using WarehouseOps.Domain.Interfaces;

namespace WarehouseOps.Application.Services;

/// <summary>
/// CANDIDATE — Implement all methods marked TODO.
/// Follow the same patterns used in StockService.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;
    private readonly IProductRepository _products;
    private readonly IStockMovementRepository _movements;
    private readonly ITenantContext _tenant;

    public OrderService(
        IOrderRepository orders,
        IProductRepository products,
        IStockMovementRepository movements,
        ITenantContext tenant)
    {
        _orders    = orders;
        _products  = products;
        _movements = movements;
        _tenant    = tenant;
    }

    // TODO 1: GetOrdersAsync
    // - Get orders for current tenant filtered by status (null = all)
    // - Apply pagination (filter first, then paginate — you saw the bug in StockService)
    // - Map to OrderDto: TotalValue = sum of (QuantityOrdered * UnitPriceAtOrder) per line
    //                    FulfilledLines = count of lines where QuantityFulfilled >= QuantityOrdered
    public async Task<PagedResult<OrderDto>> GetOrdersAsync(int page, int pageSize, string? status)
    {
        throw new NotImplementedException("TODO 1");
    }

    // TODO 2: GetOrderDetailAsync
    // - Get single order with all lines for current tenant
    // - Return Result.Failure if not found or belongs to different tenant
    // - Map to OrderDetailDto including all OrderLineDtos
    public async Task<Result<OrderDetailDto>> GetOrderDetailAsync(int orderId)
    {
        throw new NotImplementedException("TODO 2");
    }

    // TODO 3: FulfillOrderAsync — this is the most complex task
    // Business rules (you must enforce ALL of them):
    //   a) Order must be in Pending or PartiallyFulfilled status — reject Fulfilled/Cancelled
    //   b) For each order line, check current stock of the product
    //   c) Fulfill as much as stock allows per line (partial fulfillment is OK)
    //   d) For each line fulfilled (even partially), record an Outbound StockMovement
    //      with Reference = order.OrderNumber and CreatedBy = fulfilledBy param
    //   e) Deduct fulfilled quantity from product.CurrentStock
    //   f) If ALL lines are fully fulfilled → set Order.Status = Fulfilled, set FulfilledAt = now
    //      If SOME lines are fulfilled → set Order.Status = PartiallyFulfilled
    //      If NO lines could be fulfilled (all out of stock) → return Result.Failure
    //   g) Save everything in one SaveChangesAsync call at the end
    public async Task<Result<OrderDto>> FulfillOrderAsync(FulfillOrderDto dto, string fulfilledBy)
    {
        throw new NotImplementedException("TODO 3");
    }

    // TODO 4: GetOverdueOrdersAsync
    // - Return orders that are still Pending AND were ordered more than 3 days ago
    // - Order by OrderedAt ascending (oldest first)
    // - Map to OrderDto
    public async Task<IEnumerable<OrderDto>> GetOverdueOrdersAsync()
    {
        throw new NotImplementedException("TODO 4");
    }

    private static OrderDto MapToOrderDto(Domain.Entities.Order o) => new(
        o.Id,
        o.OrderNumber,
        o.CustomerName,
        o.OrderedAt,
        o.FulfilledAt,
        o.Status.ToString(),
        o.Lines.Sum(l => l.QuantityOrdered * l.UnitPriceAtOrder),
        o.Lines.Count,
        o.Lines.Count(l => l.QuantityFulfilled >= l.QuantityOrdered)
    );
}
