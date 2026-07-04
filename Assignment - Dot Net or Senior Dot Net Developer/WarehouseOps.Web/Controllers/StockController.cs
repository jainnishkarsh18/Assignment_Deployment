using Microsoft.AspNetCore.Mvc;
using WarehouseOps.Application.DTOs;
using WarehouseOps.Application.Interfaces;
using WarehouseOps.Web.ViewModels;

namespace WarehouseOps.Web.Controllers;

/// <summary>
/// CANDIDATE — Implement all actions marked TODO.
/// </summary>
public class StockController : Controller
{
    private const int PageSize = 10;
    private readonly IStockService _stock;

    public StockController(IStockService stock) => _stock = stock;

    // TODO 1: Index — GET /Stock?page=1&category=Electronics&belowReorder=true
    // - Call _stock.GetProductsAsync with page, PageSize, category, belowReorder
    // - Map PagedResult<ProductDto> → ProductListViewModel
    // - Pass filter values to ViewModel so the view can preserve them in pagination links
    public async Task<IActionResult> Index(int page = 1, string? category = null, bool? belowReorder = null)
    {
        throw new NotImplementedException("TODO 1");
    }

    // TODO 2: RecordMovement POST — called via AJAX from the stock page
    // - Validate ModelState, return BadRequest with errors if invalid
    // - Call _stock.RecordMovementAsync, pass User.Identity!.Name as createdBy
    // - Return Json(new { success = true, ... }) or Json(new { success = false, error = ... })
    [HttpPost]
    public async Task<IActionResult> RecordMovement(RecordMovementViewModel vm)
    {
        throw new NotImplementedException("TODO 2");
    }

    // TODO 3: MovementHistory — GET /Stock/MovementHistory/5?from=2024-01-01&to=2024-12-31
    // - Call _stock.GetMovementHistoryAsync
    // - Return Json(results) — used by AJAX to populate a history panel
    public async Task<IActionResult> MovementHistory(int id, DateTime? from, DateTime? to)
    {
        throw new NotImplementedException("TODO 3");
    }

    // TODO 4: LowStockAlerts — GET /Stock/LowStockAlerts
    // - Call _stock.GetLowStockAlertsAsync
    // - Return Json(results) — used by the dashboard alert widget
    public async Task<IActionResult> LowStockAlerts()
    {
        throw new NotImplementedException("TODO 4");
    }
}
