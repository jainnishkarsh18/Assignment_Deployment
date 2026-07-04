using Microsoft.EntityFrameworkCore;
using WarehouseOps.Domain.Entities;
using WarehouseOps.Domain.Enums;
using WarehouseOps.Domain.Interfaces;
using WarehouseOps.Infrastructure.Persistence;

namespace WarehouseOps.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(WarehouseDbContext db) : base(db) { }

    public async Task<IEnumerable<Product>> GetByTenantAsync(int tenantId) =>
        await _db.Products.Where(p => p.TenantId == tenantId && p.IsActive).ToListAsync();

    public async Task<IEnumerable<Product>> GetBelowReorderLevelAsync(int tenantId) =>
        await _db.Products
            .Where(p => p.TenantId == tenantId && p.IsActive && p.CurrentStock <= p.ReorderLevel)
            .ToListAsync();

    public async Task<Product?> GetBySkuAsync(int tenantId, string sku) =>
        await _db.Products.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.SKU == sku);
}

public class StockMovementRepository : Repository<StockMovement>, IStockMovementRepository
{
    public StockMovementRepository(WarehouseDbContext db) : base(db) { }

    public async Task<IEnumerable<StockMovement>> GetByTenantAsync(int tenantId, DateTime? from, DateTime? to)
    {
        var query = _db.StockMovements
            .Include(m => m.Product)
            .Where(m => m.TenantId == tenantId);

        if (from.HasValue) query = query.Where(m => m.CreatedAt >= from.Value);
        if (to.HasValue)   query = query.Where(m => m.CreatedAt <= to.Value);

        return await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<StockMovement>> GetByProductAsync(int tenantId, int productId) =>
        await _db.StockMovements
            .Include(m => m.Product)
            .Where(m => m.TenantId == tenantId && m.ProductId == productId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

    public async Task<Dictionary<int, int>> GetNetMovementByProductAsync(int tenantId, DateTime from, DateTime to)
    {
        var movements = await _db.StockMovements
            .Where(m => m.TenantId == tenantId && m.CreatedAt >= from && m.CreatedAt <= to)
            .ToListAsync();

        return movements
            .GroupBy(m => m.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(m => m.Type == StockMovementType.Inbound ? m.Quantity : -m.Quantity)
            );
    }
}

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(WarehouseDbContext db) : base(db) { }

    public async Task<IEnumerable<Order>> GetByTenantAsync(int tenantId, OrderStatus? status)
    {
        var query = _db.Orders
            .Include(o => o.Lines)
            .Where(o => o.TenantId == tenantId);

        if (status.HasValue) query = query.Where(o => o.Status == status.Value);

        return await query.OrderByDescending(o => o.OrderedAt).ToListAsync();
    }

    public async Task<Order?> GetWithLinesAsync(int tenantId, int orderId) =>
        await _db.Orders
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == orderId);

    public async Task<IEnumerable<Order>> GetOverdueAsync(int tenantId) =>
        await _db.Orders
            .Include(o => o.Lines)
            .Where(o => o.TenantId == tenantId
                     && o.Status == OrderStatus.Pending
                     && o.OrderedAt < DateTime.UtcNow.AddDays(-3))
            .OrderBy(o => o.OrderedAt)
            .ToListAsync();
}

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(WarehouseDbContext db) : base(db) { }

    public async Task<Tenant?> GetBySlugAsync(string slug) =>
        await _db.Tenants.FirstOrDefaultAsync(t => t.Slug == slug && t.IsActive);
}
