using WarehouseOps.Application.Common;
using WarehouseOps.Application.DTOs;
using WarehouseOps.Application.Interfaces;
using WarehouseOps.Domain.Entities;
using WarehouseOps.Domain.Enums;
using WarehouseOps.Domain.Interfaces;

namespace WarehouseOps.Application.Services;

/// <summary>
/// CANDIDATE — This service has 5 bugs. Find them, fix them, and write a comment above each fix:
///   - What was wrong
///   - What the impact would be in production
///   - Why your fix is correct
/// </summary>
public class StockService : IStockService
{
    private readonly IProductRepository _products;
    private readonly IStockMovementRepository _movements;
    private readonly ITenantContext _tenant;

    public StockService(IProductRepository products, IStockMovementRepository movements, ITenantContext tenant)
    {
        _products = products;
        _movements = movements;
        _tenant = tenant;
    }

    // BUG 1: Filtering by category and belowReorder happens AFTER pagination.
    //        This means page 1 of 10 items may return fewer than 10 results (or zero)
    //        even when matching records exist on later "raw" pages.
    //        Fix: filter first, then paginate. TotalCount must also reflect the filtered count.

    // FIX 1:
    // Problem:
    // Filtering was applied after pagination, so matching records on later pages were never considered.
    //
    // Production Impact:
    // Users could receive incomplete or empty pages even though matching products exist.
    // TotalCount was also incorrect because it represented only the current page.
    //
    // Why this fix is correct:
    // Apply all filters first, calculate the total filtered count, then paginate the filtered result.
    public async Task<PagedResult<ProductDto>> GetProductsAsync(int page, int pageSize, string? category, bool? belowReorder)
    {
        var all = await _products.GetByTenantAsync(_tenant.TenantId);

        var filtered = all
            .Where(p => string.IsNullOrEmpty(category) || p.Category == category)
            .Where(p => belowReorder == null || (p.CurrentStock <= p.ReorderLevel) == belowReorder)
            .Select(MapToDto)
            .ToList();

        var paged = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();


        return new PagedResult<ProductDto>
        {
            Items = paged,
            TotalCount = filtered.Count,
            Page = page,
            PageSize = pageSize
        };
    }

    // BUG 2: Stock is updated AFTER the movement is saved.
    //        If SaveChangesAsync throws after the movement is recorded but before stock is updated,
    //        the movement exists in DB but stock level is wrong — data is inconsistent.
    //        Fix: update stock first, then save both changes in a single SaveChangesAsync call.
    //
    // BUG 3: Outbound movements do not check if CurrentStock is sufficient.
    //        A product can go negative. Fix: add a stock sufficiency check for Outbound type
    //        and return Result.Failure(...) if stock would go below zero.
    public async Task<Result<StockMovementDto>> RecordMovementAsync(RecordMovementDto dto, string createdBy)
    {
        if (!Enum.TryParse<StockMovementType>(dto.Type, out var movementType))
            return Result<StockMovementDto>.Failure($"Invalid movement type: {dto.Type}");

        var product = await _products.GetByIdAsync(dto.ProductId);
        if (product is null || product.TenantId != _tenant.TenantId)
            return Result<StockMovementDto>.Failure("Product not found.");

        // FIX 3:
        // Problem:
        // Outbound movements were allowed even when available stock was insufficient.
        //
        // Production Impact:
        // Product stock could become negative, leading to invalid inventory records.
        //
        // Why this fix is correct:
        // Validate stock before applying an outbound movement and reject the request if stock is insufficient.

        if (movementType == StockMovementType.Outbound &&
            product.CurrentStock < dto.Quantity)
        {
            return Result<StockMovementDto>.Failure("Insufficient stock available.");
        }

        var movement = new StockMovement
        {
            TenantId = _tenant.TenantId,
            ProductId = dto.ProductId,
            Type = movementType,
            Quantity = dto.Quantity,
            Reference = dto.Reference,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            Notes = dto.Notes
        };

        // BUG 2: saved before stock is updated

        // FIX 2:
        // Problem:
        // Stock movement was saved before updating product stock.
        //
        // Production Impact:
        // If product update failed afterwards, the movement would exist but stock would remain unchanged,
        // causing inconsistent inventory data.
        //
        // Why this fix is correct:
        // Update both entities first and persist them together with a single SaveChangesAsync call.

        await _movements.AddAsync(movement);

        // Stock adjustment — BUG 3: no check for negative stock on Outbound

        product.CurrentStock += movementType switch
        {
            StockMovementType.Inbound => dto.Quantity,
            StockMovementType.Outbound => -dto.Quantity,
            StockMovementType.Adjustment => dto.Quantity,
            _ => 0
        };

        _products.Update(product);

        // Save once after both movement and stock update.
        await _movements.SaveChangesAsync();

        return Result<StockMovementDto>.Success(new StockMovementDto(
            movement.Id,
            product.Name,
            product.SKU,
            movement.Type.ToString(),
            movement.Quantity,
            movement.Reference,
            movement.CreatedBy,
            movement.CreatedAt,
            movement.Notes
        ));
    }

    // BUG 4: GetMovementHistoryAsync ignores the tenant filter entirely.
    //        Any tenant can see another tenant's movement history just by passing a productId.
    //        Fix: pass _tenant.TenantId into GetByProductAsync.
    public async Task<IEnumerable<StockMovementDto>> GetMovementHistoryAsync(int productId, DateTime? from, DateTime? to)
    {
        // BUG 4: tenantId hardcoded to 0

        // FIX 4:
        // Problem:
        // Tenant ID was hardcoded to 0, allowing movement history from other tenants.
        //
        // Production Impact:
        // This creates a serious multi-tenant security issue by exposing another tenant's inventory history.
        //
        // Why this fix is correct:
        // Always query using the current tenant so each tenant only accesses its own data.

        var movements = await _movements.GetByProductAsync(_tenant.TenantId, productId);

        return movements
            .Where(m => from == null || m.CreatedAt >= from)
            .Where(m => to == null || m.CreatedAt <= to)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new StockMovementDto(
                m.Id,
                m.Product.Name,
                m.Product.SKU,
                m.Type.ToString(),
                m.Quantity,
                m.Reference,
                m.CreatedBy,
                m.CreatedAt,
                m.Notes
            ));
    }

    // BUG 5: Returns ALL products below reorder level across ALL tenants, not just the current tenant.
    //        Fix: pass _tenant.TenantId to GetBelowReorderLevelAsync.
    public async Task<IEnumerable<ProductDto>> GetLowStockAlertsAsync()
    {

        // BUG 5: tenantId hardcoded to 0

        // FIX 5:
        // Problem:
        // Low-stock alerts ignored tenant filtering and returned products for all tenants.
        //
        // Production Impact:
        // Users could view inventory information belonging to other organizations.
        //
        // Why this fix is correct:
        // Pass the current tenant ID so only the current tenant's products are returned.

        var products = await _products.GetBelowReorderLevelAsync(_tenant.TenantId);
        return products.Select(MapToDto);
    }

    private static ProductDto MapToDto(Product p) => new(
        p.Id, p.SKU, p.Name, p.Category,
        p.CurrentStock, p.ReorderLevel, p.UnitPrice, p.IsActive,
        p.CurrentStock <= p.ReorderLevel
    );
}
