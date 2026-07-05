using System.ComponentModel.DataAnnotations;
using WarehouseOps.Application.DTOs;

namespace WarehouseOps.Web.ViewModels;

public class ProductListViewModel
{
    public IEnumerable<ProductRowViewModel> Products { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string? CategoryFilter { get; set; }
    public bool? BelowReorderFilter { get; set; }
}

public class ProductRowViewModel
{
    public int Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsBelowReorder { get; set; }
}

public class RecordMovementViewModel
{
    [Required] public int ProductId { get; set; }
    [Required] public string Type { get; set; } = string.Empty;
    [Required, Range(1, int.MaxValue)] public int Quantity { get; set; }
    [Required] public string Reference { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class OrderListViewModel
{
    public IEnumerable<OrderRowViewModel> Orders { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string? StatusFilter { get; set; }
}

public class OrderRowViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public int TotalLines { get; set; }
    public int FulfilledLines { get; set; }
    public bool IsOverdue { get; set; }
}