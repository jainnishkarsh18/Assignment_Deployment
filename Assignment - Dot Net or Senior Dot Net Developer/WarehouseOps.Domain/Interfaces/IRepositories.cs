using WarehouseOps.Domain.Entities;
using WarehouseOps.Domain.Enums;

namespace WarehouseOps.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByTenantAsync(int tenantId);
    Task<IEnumerable<Product>> GetBelowReorderLevelAsync(int tenantId);
    Task<Product?> GetBySkuAsync(int tenantId, string sku);
}

public interface IStockMovementRepository : IRepository<StockMovement>
{
    Task<IEnumerable<StockMovement>> GetByTenantAsync(int tenantId, DateTime? from, DateTime? to);
    Task<IEnumerable<StockMovement>> GetByProductAsync(int tenantId, int productId);
    Task<Dictionary<int, int>> GetNetMovementByProductAsync(int tenantId, DateTime from, DateTime to);
}

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByTenantAsync(int tenantId, OrderStatus? status);
    Task<Order?> GetWithLinesAsync(int tenantId, int orderId);
    Task<IEnumerable<Order>> GetOverdueAsync(int tenantId);  // Pending > 3 days old
}

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetBySlugAsync(string slug);
}
