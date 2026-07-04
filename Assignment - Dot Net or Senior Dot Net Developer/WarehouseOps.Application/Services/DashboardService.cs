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
        _products  = products;
        _orders    = orders;
        _movements = movements;
        _tenant    = tenant;
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
        throw new NotImplementedException("TODO: implement dashboard aggregation");
    }
}
