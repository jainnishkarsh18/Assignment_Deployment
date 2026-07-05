using WarehouseOps.Application.Common;
using WarehouseOps.Application.DTOs;
using WarehouseOps.Application.Interfaces;
using WarehouseOps.Domain.Entities;
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
        _orders = orders;
        _products = products;
        _movements = movements;
        _tenant = tenant;
    }

    // TODO 1: GetOrdersAsync
    // - Get orders for current tenant filtered by status (null = all)
    // - Apply pagination (filter first, then paginate — you saw the bug in StockService)
    // - Map to OrderDto: TotalValue = sum of (QuantityOrdered * UnitPriceAtOrder) per line
    //                    FulfilledLines = count of lines where QuantityFulfilled >= QuantityOrdered
    public async Task<PagedResult<OrderDto>> GetOrdersAsync(int page, int pageSize, string? status)
    {
        OrderStatus? orderStatus = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                return new PagedResult<OrderDto>
                {
                    Items = Enumerable.Empty<OrderDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }

            orderStatus = parsedStatus;
        }

        var orders = await _orders.GetByTenantAsync(_tenant.TenantId, orderStatus);

        var totalCount = orders.Count();

        var pagedOrders = orders
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToOrderDto)
            .ToList();

        return new PagedResult<OrderDto>
        {
            Items = pagedOrders,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // TODO 2: GetOrderDetailAsync
    // - Get single order with all lines for current tenant
    // - Return Result.Failure if not found or belongs to different tenant
    // - Map to OrderDetailDto including all OrderLineDtos

    public async Task<Result<OrderDetailDto>> GetOrderDetailAsync(int orderId)
    {
        var order = await _orders.GetByIdAsync(orderId);

        if (order == null || order.TenantId != _tenant.TenantId)
        {
            return Result<OrderDetailDto>.Failure("Order not found.");
        }

        var dto = new OrderDetailDto(
            order.Id,
            order.OrderNumber,
            order.CustomerName,
            order.OrderedAt,
            order.Status.ToString(),
            order.Lines.Select(l => new OrderLineDto(
                l.ProductId,
                l.Product.Name,
                l.Product.SKU,
                l.QuantityOrdered,
                l.QuantityFulfilled,
                l.UnitPriceAtOrder,
                l.QuantityFulfilled >= l.QuantityOrdered
            ))
        );

        return Result<OrderDetailDto>.Success(dto);
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
        var order = await _orders.GetWithLinesAsync(_tenant.TenantId, dto.OrderId);

        if (order == null)
            return Result<OrderDto>.Failure("Order not found.");

        if (order.Status == OrderStatus.Fulfilled ||
            order.Status == OrderStatus.Cancelled)
        {
            return Result<OrderDto>.Failure($"Order is already {order.Status}.");
        }

        bool anythingFulfilled = false;

        foreach (var line in order.Lines)
        {
            var remaining = line.QuantityOrdered - line.QuantityFulfilled;

            if (remaining <= 0)
                continue;

            var product = line.Product;

            if (product == null)
                continue;

            var quantityToFulfill = Math.Min(product.CurrentStock, remaining);

            if (quantityToFulfill <= 0)
                continue;

            line.QuantityFulfilled += quantityToFulfill;
            product.CurrentStock -= quantityToFulfill;

            await _movements.AddAsync(new StockMovement
            {
                TenantId = _tenant.TenantId,
                ProductId = product.Id,
                Type = StockMovementType.Outbound,
                Quantity = quantityToFulfill,
                Reference = order.OrderNumber,
                CreatedBy = fulfilledBy,
                CreatedAt = DateTime.UtcNow
            });

            _products.Update(product);

            anythingFulfilled = true;
        }

        if (!anythingFulfilled)
            return Result<OrderDto>.Failure("No stock available to fulfill this order.");

        if (order.Lines.All(l => l.QuantityFulfilled >= l.QuantityOrdered))
        {
            order.Status = OrderStatus.Fulfilled;
            order.FulfilledAt = DateTime.UtcNow;
        }
        else
        {
            order.Status = OrderStatus.PartiallyFulfilled;
        }

        _orders.Update(order);

        // Save all changes once
        await _orders.SaveChangesAsync();

        return Result<OrderDto>.Success(MapToOrderDto(order));
    }

    // TODO 4: GetOverdueOrdersAsync
    // - Return orders that are still Pending AND were ordered more than 3 days ago
    // - Order by OrderedAt ascending (oldest first)
    // - Map to OrderDto
    public async Task<IEnumerable<OrderDto>> GetOverdueOrdersAsync()
    {
        var overDueOrders = await _orders.GetOverdueAsync(_tenant.TenantId);

        return overDueOrders.Select(MapToOrderDto);
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
