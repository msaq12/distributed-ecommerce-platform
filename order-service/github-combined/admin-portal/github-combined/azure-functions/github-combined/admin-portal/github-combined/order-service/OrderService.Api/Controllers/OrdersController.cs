using Microsoft.AspNetCore.Mvc;
using OrderService.Api.DTOs;
using OrderService.Api.Services;

namespace OrderService.Api.Controllers;

/// <summary>
/// Orders API Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders (limited to 100 most recent)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAllOrders()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all orders");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetOrderById(int id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
                return NotFound($"Order with ID {id} not found");

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get order by order number
    /// </summary>
    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetOrderByNumber(string orderNumber)
    {
        try
        {
            var order = await _orderService.GetOrderByNumberAsync(orderNumber);

            if (order == null)
                return NotFound($"Order {orderNumber} not found");

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderNumber}", orderNumber);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get orders by customer email
    /// </summary>
    [HttpGet("customer/{email}")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrdersByCustomer(string email)
    {
        try
        {
            var orders = await _orderService.GetOrdersByCustomerEmailAsync(email);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for customer {Email}", email);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get orders by status
    /// </summary>
    [HttpGet("status/{statusId:int}")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrdersByStatus(int statusId)
    {
        try
        {
            var orders = await _orderService.GetOrdersByStatusAsync(statusId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders by status {StatusId}", statusId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create new order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.OrderId },
                order
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid order request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponse>> UpdateOrderStatus(
        int id,
        [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, request);

            if (order == null)
                return NotFound($"Order with ID {id} not found");

            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid status update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get order statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, object>>> GetStatistics()
    {
        try
        {
            var stats = await _orderService.GetOrderStatisticsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating statistics");
            return StatusCode(500, "Internal server error");
        }
    }
}
