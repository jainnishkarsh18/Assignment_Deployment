using WarehouseOps.Application.DTOs;
using WarehouseOps.Application.Interfaces;
using WarehouseOps.Domain.Enums;
using WarehouseOps.Domain.Interfaces;

namespace WarehouseOps.Application.Services;

/// <summary>
/// CANDIDATE — Implement GetDashboardAsync.
/// This tests your ability to write efficient LINQ aggregations without N+1 queries.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IProductRepository _products;
    private readonly IOrderRepository _orders;
    private readonly IStockMovementRepository _movements;
    private readonly ITenantContext _tenant;

    public DashboardService(
        IProductRepository products,
        IOrderRepository orders,
        IStockMovementRepository movements,
        ITenantContext tenant)
    {
        _products = products;
        _orders = orders;
        _movements = movements;
        _tenant = tenant;
    }

    // TODO: Implement GetDashboardAsync
    // All data must be scoped to _tenant.TenantId.
    //
    // DashboardDto fields:
    //   TotalProducts      → count of active products
    //   BelowReorderCount  → count of active products where CurrentStock <= ReorderLevel
    //   PendingOrders      → count of orders with Status == Pending
    //   OverdueOrders      → count of Pending orders older than 3 days
    //   TotalStockValue    → sum of (CurrentStock * UnitPrice) for active products
    //
    //   StockByCategory    → group active products by Category, for each group:
    //                          Category, ProductCount, TotalValue (sum CurrentStock * UnitPrice)
    //                        ordered by TotalValue descending
    //
    //   TopMovedProducts   → top 5 products by total Outbound quantity in the last 30 days
    //                          ProductName, SKU, TotalOutbound
    //                        ordered by TotalOutbound descending
    //
    // IMPORTANT: Do NOT call GetAllAsync multiple times for the same data.
    //            Load each collection once and reuse it in memory with LINQ.
    //            We will ask you about this in the walkthrough.
    public async Task<DashboardDto> GetDashboardAsync()
    {
        var products = (await _products.GetByTenantAsync(_tenant.TenantId)).ToList();
        var orders = (await _orders.GetByTenantAsync(_tenant.TenantId, null)).ToList();
        var movements = (await _movements.GetByTenantAsync(
            _tenant.TenantId,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow)).ToList();

        return new DashboardDto(
            TotalProducts: products.Count,

            BelowReorderCount: products.Count(p =>
                p.CurrentStock <= p.ReorderLevel),

            PendingOrders: orders.Count(o =>
                o.Status == OrderStatus.Pending),

            OverdueOrders: orders.Count(o =>
                o.Status == OrderStatus.Pending &&
                o.OrderedAt < DateTime.UtcNow.AddDays(-3)),

            TotalStockValue: products.Sum(p =>
                p.CurrentStock * p.UnitPrice),

            StockByCategory: products
                .GroupBy(p => p.Category)
                .Select(g => new CategoryStockDto(
                    g.Key,
                    g.Count(),
                    g.Sum(p => p.CurrentStock * p.UnitPrice)))
                .OrderByDescending(c => c.TotalValue),

            TopMovedProducts: movements
                .Where(m => m.Type == StockMovementType.Outbound)
                .GroupBy(m => new
                {
                    m.Product.Name,
                    m.Product.SKU
                })
                .Select(g => new TopMovedProductDto(
                    g.Key.Name,
                    g.Key.SKU,
                    g.Sum(m => m.Quantity)))
                .OrderByDescending(p => p.TotalOutbound)
                .Take(5)
        );
    }
}
