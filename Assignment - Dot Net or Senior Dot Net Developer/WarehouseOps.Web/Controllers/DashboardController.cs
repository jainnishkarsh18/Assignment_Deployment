using Microsoft.AspNetCore.Mvc;
using WarehouseOps.Application.Interfaces;

namespace WarehouseOps.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IDashboardService _dashboard;
    private readonly IOrderService _orders;

    public DashboardController(IDashboardService dashboard, IOrderService orders)
    {
        _dashboard = dashboard;
        _orders    = orders;
    }

    // TODO: Implement Index
    // - Call _dashboard.GetDashboardAsync()
    // - Pass the DashboardDto directly to the view (ViewBag or strongly typed — your choice, justify it)
    // - Also fetch overdue orders via _orders.GetOverdueOrdersAsync() and pass to view
    public async Task<IActionResult> Index()
    {
        throw new NotImplementedException("TODO: implement dashboard");
    }
}
