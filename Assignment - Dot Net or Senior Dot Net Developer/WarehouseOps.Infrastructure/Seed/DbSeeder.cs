using WarehouseOps.Domain.Entities;
using WarehouseOps.Domain.Enums;
using WarehouseOps.Infrastructure.Persistence;

namespace WarehouseOps.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(WarehouseDbContext db)
    {
        if (db.Tenants.Any()) return;

        var tenants = new List<Tenant>
        {
            new() { Name = "Warehouse Alpha", Slug = "warehouse-alpha", IsActive = true },
            new() { Name = "Warehouse Beta",  Slug = "warehouse-beta",  IsActive = true }
        };
        await db.Tenants.AddRangeAsync(tenants);
        await db.SaveChangesAsync();

        var productsA = new List<Product>
        {
            new() { TenantId = tenants[0].Id, SKU = "ELEC-001", Name = "USB-C Hub",         Category = "Electronics", CurrentStock = 5,   ReorderLevel = 10,  UnitPrice = 29.99m,  IsActive = true },
            new() { TenantId = tenants[0].Id, SKU = "ELEC-002", Name = "Wireless Mouse",    Category = "Electronics", CurrentStock = 50,  ReorderLevel = 15,  UnitPrice = 19.99m,  IsActive = true },
            new() { TenantId = tenants[0].Id, SKU = "ELEC-003", Name = "Mechanical Keyboard",Category = "Electronics",CurrentStock = 3,   ReorderLevel = 8,   UnitPrice = 89.99m,  IsActive = true },
            new() { TenantId = tenants[0].Id, SKU = "FURN-001", Name = "Desk Lamp",         Category = "Furniture",   CurrentStock = 20,  ReorderLevel = 5,   UnitPrice = 45.00m,  IsActive = true },
            new() { TenantId = tenants[0].Id, SKU = "FURN-002", Name = "Monitor Stand",     Category = "Furniture",   CurrentStock = 0,   ReorderLevel = 5,   UnitPrice = 35.00m,  IsActive = true },
            new() { TenantId = tenants[0].Id, SKU = "STAT-001", Name = "Notebook Pack",     Category = "Stationery",  CurrentStock = 100, ReorderLevel = 20,  UnitPrice = 8.99m,   IsActive = true },
        };

        var productsB = new List<Product>
        {
            new() { TenantId = tenants[1].Id, SKU = "ELEC-001", Name = "HDMI Cable",        Category = "Electronics", CurrentStock = 30,  ReorderLevel = 10,  UnitPrice = 12.99m,  IsActive = true },
            new() { TenantId = tenants[1].Id, SKU = "TOOL-001", Name = "Power Drill",       Category = "Tools",       CurrentStock = 4,   ReorderLevel = 5,   UnitPrice = 149.99m, IsActive = true },
        };

        await db.Products.AddRangeAsync(productsA);
        await db.Products.AddRangeAsync(productsB);
        await db.SaveChangesAsync();

        var movements = new List<StockMovement>
        {
            new() { TenantId = tenants[0].Id, ProductId = productsA[0].Id, Type = StockMovementType.Inbound,  Quantity = 20, Reference = "PO-001", CreatedBy = "admin", CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new() { TenantId = tenants[0].Id, ProductId = productsA[0].Id, Type = StockMovementType.Outbound, Quantity = 15, Reference = "ORD-001", CreatedBy = "admin", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new() { TenantId = tenants[0].Id, ProductId = productsA[1].Id, Type = StockMovementType.Inbound,  Quantity = 60, Reference = "PO-002", CreatedBy = "admin", CreatedAt = DateTime.UtcNow.AddDays(-8) },
            new() { TenantId = tenants[0].Id, ProductId = productsA[1].Id, Type = StockMovementType.Outbound, Quantity = 10, Reference = "ORD-002", CreatedBy = "admin", CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new() { TenantId = tenants[0].Id, ProductId = productsA[2].Id, Type = StockMovementType.Outbound, Quantity = 5,  Reference = "ORD-003", CreatedBy = "admin", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new() { TenantId = tenants[0].Id, ProductId = productsA[4].Id, Type = StockMovementType.Outbound, Quantity = 8,  Reference = "ORD-004", CreatedBy = "admin", CreatedAt = DateTime.UtcNow.AddDays(-1) },
        };
        await db.StockMovements.AddRangeAsync(movements);

        var orders = new List<Order>
        {
            // Normal pending
            new() { TenantId = tenants[0].Id, OrderNumber = "ORD-2024-001", CustomerName = "Acme Corp",    OrderedAt = DateTime.UtcNow.AddDays(-1), Status = OrderStatus.Pending,
                Lines = new List<OrderLine> {
                    new() { ProductId = productsA[1].Id, QuantityOrdered = 5,  QuantityFulfilled = 0, UnitPriceAtOrder = 19.99m },
                    new() { ProductId = productsA[3].Id, QuantityOrdered = 2,  QuantityFulfilled = 0, UnitPriceAtOrder = 45.00m }
                }},
            // Overdue pending (4 days old)
            new() { TenantId = tenants[0].Id, OrderNumber = "ORD-2024-002", CustomerName = "Beta Ltd",     OrderedAt = DateTime.UtcNow.AddDays(-4), Status = OrderStatus.Pending,
                Lines = new List<OrderLine> {
                    new() { ProductId = productsA[0].Id, QuantityOrdered = 10, QuantityFulfilled = 0, UnitPriceAtOrder = 29.99m }
                }},
            // Partially fulfilled
            new() { TenantId = tenants[0].Id, OrderNumber = "ORD-2024-003", CustomerName = "Gamma Inc",    OrderedAt = DateTime.UtcNow.AddDays(-2), Status = OrderStatus.PartiallyFulfilled,
                Lines = new List<OrderLine> {
                    new() { ProductId = productsA[2].Id, QuantityOrdered = 5,  QuantityFulfilled = 3, UnitPriceAtOrder = 89.99m },
                    new() { ProductId = productsA[4].Id, QuantityOrdered = 3,  QuantityFulfilled = 0, UnitPriceAtOrder = 35.00m }
                }},
            // Fulfilled
            new() { TenantId = tenants[0].Id, OrderNumber = "ORD-2024-004", CustomerName = "Delta Co",     OrderedAt = DateTime.UtcNow.AddDays(-7), FulfilledAt = DateTime.UtcNow.AddDays(-6), Status = OrderStatus.Fulfilled,
                Lines = new List<OrderLine> {
                    new() { ProductId = productsA[5].Id, QuantityOrdered = 10, QuantityFulfilled = 10, UnitPriceAtOrder = 8.99m }
                }},
        };
        await db.Orders.AddRangeAsync(orders);
        await db.SaveChangesAsync();
    }
}
