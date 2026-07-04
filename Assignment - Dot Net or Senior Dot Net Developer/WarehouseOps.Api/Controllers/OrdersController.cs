using Microsoft.AspNetCore.Mvc;
using WarehouseOps.Application.DTOs;
using WarehouseOps.Application.Interfaces;

namespace WarehouseOps.Api.Controllers;

/// <summary>
/// CANDIDATE — Implement all endpoints marked TODO.
/// This is a REST API — use correct HTTP status codes and response shapes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders) => _orders = orders;

    // TODO 1: GET api/orders?page=1&pageSize=10&status=Pending
    // - Call _orders.GetOrdersAsync
    // - Return 200 with PagedResult<OrderDto>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
    {
        throw new NotImplementedException("TODO 1");
    }

    // TODO 2: GET api/orders/{id}
    // - Call _orders.GetOrderDetailAsync
    // - Return 200 with OrderDetailDto, or 404 if Result.IsSuccess == false
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        throw new NotImplementedException("TODO 2");
    }

    // TODO 3: POST api/orders/{id}/fulfill
    // - Call _orders.FulfillOrderAsync, pass User.Identity!.Name as fulfilledBy
    // - Return 200 with updated OrderDto on success
    // - Return 400 with { error = "..." } if Result.IsSuccess == false
    // - Return 404 if order not found
    [HttpPost("{id:int}/fulfill")]
    public async Task<IActionResult> Fulfill(int id)
    {
        throw new NotImplementedException("TODO 3");
    }

    // TODO 4: GET api/orders/overdue
    // - Call _orders.GetOverdueOrdersAsync
    // - Return 200 with list of OrderDto
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdue()
    {
        throw new NotImplementedException("TODO 4");
    }
}
