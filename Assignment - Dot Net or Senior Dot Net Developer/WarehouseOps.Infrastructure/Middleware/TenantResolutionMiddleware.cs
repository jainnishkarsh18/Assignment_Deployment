using WarehouseOps.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace WarehouseOps.Infrastructure.Middleware;


public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantRepository tenantRepository,
        TenantContext tenantContext)
    {
        if (!context.Request.Headers.TryGetValue("X-Tenant-Slug", out var slug) ||
            string.IsNullOrWhiteSpace(slug))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Missing X-Tenant-Slug header.");
            return;
        }

        var tenant = await tenantRepository.GetBySlugAsync(slug!);

        if (tenant == null || !tenant.IsActive)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("Tenant not found.");
            return;
        }

        tenantContext.TenantId = tenant.Id;
        tenantContext.TenantSlug = tenant.Slug;

        await _next(context);
    }
}