using Microsoft.AspNetCore.Mvc;
using Orders.Models;
using Orders.Services;
using Orders.ViewModels;

namespace Orders.Controllers
{
    /// <summary>
    /// Controller for managing orders.
    /// Provides endpoints to retrieve, create, update, and delete orders.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdersController"/> class.
        /// </summary>
        /// <param name="orderService">Service for order operations.</param>
        /// <param name="logger">Logger instance.</param>
        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all orders.
        /// </summary>
        /// <returns>A list of all orders.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            try
            {
                _logger.LogInformation("Request received for GetAllOrders");
                var orders = await _orderService.GetAllOrdersAsync();
                _logger.LogInformation("Successfully retrieved {Count} orders", orders.Count);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all orders");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets an order by ID.
        /// </summary>
        /// <param name="id">The ID of the order.</param>
        /// <returns>The order with the specified ID.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Order>> GetOrderById(string id)
        {
            try
            {
                _logger.LogInformation("Request received for GetOrderById: {OrderId}", id);
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found", id);
                    return NotFound($"Order with ID {id} not found");
                }

                _logger.LogInformation("Successfully retrieved order with ID: {OrderId}", id);
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting order with ID: {OrderId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Creates a new order.
        /// </summary>
        /// <param name="orderViewModel">The order view model containing order details.</param>
        /// <returns>The created order.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] OrderViewModel orderViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid order model received");
                    return BadRequest(ModelState);
                }

                var order = new Order
                {
                    UserId = orderViewModel.UserId,
                    ProductId = orderViewModel.ProductId,
                    ProductPrice = orderViewModel.ProductPrice,
                    Quantity = orderViewModel.Quantity,
                    OrderStatus = orderViewModel.OrderStatus ?? "Pending"
                };

                _logger.LogInformation("Request received to create new order for user: {UserId}", order.UserId);
                var createdOrder = await _orderService.CreateOrderAsync(order);
                _logger.LogInformation("Successfully created order with ID: {OrderId}", createdOrder.Id);

                return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, createdOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Updates an existing order.
        /// </summary>
        /// <param name="id">The ID of the order to update.</param>
        /// <param name="orderViewModel">The order view model containing updated order details.</param>
        /// <returns>The updated order.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Order>> UpdateOrder(string id, [FromBody] OrderViewModel orderViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid order model received for update");
                    return BadRequest(ModelState);
                }

                var order = new Order
                {
                    Id = id,
                    UserId = orderViewModel.UserId,
                    ProductId = orderViewModel.ProductId,
                    ProductPrice = orderViewModel.ProductPrice,
                    Quantity = orderViewModel.Quantity,
                    OrderStatus = orderViewModel.OrderStatus ?? "Pending"
                };

                _logger.LogInformation("Request received to update order with ID: {OrderId}", id);
                var updatedOrder = await _orderService.UpdateOrderAsync(id, order);

                if (updatedOrder == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found for update", id);
                    return NotFound($"Order with ID {id} not found");
                }

                _logger.LogInformation("Successfully updated order with ID: {OrderId}", id);
                return Ok(updatedOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating order with ID: {OrderId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Deletes an order by ID.
        /// </summary>
        /// <param name="id">The ID of the order to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            try
            {
                _logger.LogInformation("Request received to delete order with ID: {OrderId}", id);
                var result = await _orderService.DeleteOrderAsync(id);

                if (!result)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found for deletion", id);
                    return NotFound($"Order with ID {id} not found");
                }

                _logger.LogInformation("Successfully deleted order with ID: {OrderId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting order with ID: {OrderId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Health check endpoint.
        /// </summary>
        /// <returns>Health status of the API.</returns>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok("Orders API is running");
        }
    }
}
