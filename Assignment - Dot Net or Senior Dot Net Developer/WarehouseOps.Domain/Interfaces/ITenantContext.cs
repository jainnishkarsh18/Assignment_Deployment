namespace WarehouseOps.Domain.Interfaces;

/// <summary>
/// Resolved per-request from the X-Tenant-Slug header.
/// Candidate must implement this and register it in DI.
/// </summary>
public interface ITenantContext
{
    int TenantId { get; }
    string TenantSlug { get; }
}
