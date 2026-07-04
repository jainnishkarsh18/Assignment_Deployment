using Microsoft.EntityFrameworkCore;
using WarehouseOps.Application.Interfaces;
using WarehouseOps.Application.Services;
using WarehouseOps.Domain.Interfaces;
using WarehouseOps.Infrastructure.Middleware;
using WarehouseOps.Infrastructure.Persistence;
using WarehouseOps.Infrastructure.Repositories;
using WarehouseOps.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Services
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// TODO (CANDIDATE): Register TenantContext as scoped
// - Register TenantContext as both ITenantContext and TenantContext (same instance per request)
// - Implement TenantResolutionMiddleware that:
//     1. Reads the X-Tenant-Slug header from the request
//     2. Looks up the tenant using ITenantRepository.GetBySlugAsync
//     3. Populates TenantContext.TenantId and TenantContext.TenantSlug
//     4. Returns 400 if header is missing, 404 if tenant not found or inactive
// - Register the middleware below using app.UseMiddleware<TenantResolutionMiddleware>()

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
    db.Database.EnsureCreated();
    await DbSeeder.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();

// TODO (CANDIDATE): app.UseMiddleware<TenantResolutionMiddleware>(); — add here after implementing it

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
