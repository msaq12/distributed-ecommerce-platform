namespace func_utilities_dev.Models;

public class OrderStatistics
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<OrderStatusCount> OrdersByStatus { get; set; } = new();
    public int OrdersLast24Hours { get; set; }
}

public class OrderStatusCount
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}
