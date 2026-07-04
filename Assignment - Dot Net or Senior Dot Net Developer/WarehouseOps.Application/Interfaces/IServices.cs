using WarehouseOps.Application.Common;
using WarehouseOps.Application.DTOs;

namespace WarehouseOps.Application.Interfaces;

public interface IStockService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(int page, int pageSize, string? category, bool? belowReorder);
    Task<Result<StockMovementDto>> RecordMovementAsync(RecordMovementDto dto, string createdBy);
    Task<IEnumerable<StockMovementDto>> GetMovementHistoryAsync(int productId, DateTime? from, DateTime? to);
    Task<IEnumerable<ProductDto>> GetLowStockAlertsAsync();
}

public interface IOrderService
{
    Task<PagedResult<OrderDto>> GetOrdersAsync(int page, int pageSize, string? status);
    Task<Result<OrderDetailDto>> GetOrderDetailAsync(int orderId);
    Task<Result<OrderDto>> FulfillOrderAsync(FulfillOrderDto dto, string fulfilledBy);
    Task<IEnumerable<OrderDto>> GetOverdueOrdersAsync();
}

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync();
}
