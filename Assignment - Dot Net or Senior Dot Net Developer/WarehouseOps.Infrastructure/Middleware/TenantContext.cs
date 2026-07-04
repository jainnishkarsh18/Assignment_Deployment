using WarehouseOps.Domain.Interfaces;

namespace WarehouseOps.Infrastructure.Middleware;

/// <summary>
/// Resolved per-request. Populated by TenantResolutionMiddleware.
/// Candidate must implement the middleware that sets TenantId and TenantSlug
/// by reading the X-Tenant-Slug header and looking up the tenant in the database.
/// </summary>
public class TenantContext : ITenantContext
{
    public int TenantId { get; set; }
    public string TenantSlug { get; set; } = string.Empty;
}
