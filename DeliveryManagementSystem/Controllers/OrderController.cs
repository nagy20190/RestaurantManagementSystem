using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DeliveryManagementSystem.Core.Interfaces;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly IGenericRepository<OrderItem> _orderItemRepository;
        private readonly IGenericRepository<Meal> _mealRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Restaurant> _restaurantRepository;
        private readonly IGenericRepository<Payment> _paymentRepository;
        private readonly IMapper _mapper;

        public OrderController(
            IGenericRepository<Order> orderRepository,
            IGenericRepository<OrderItem> orderItemRepository,
            IGenericRepository<Meal> mealRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<Restaurant> restaurantRepository,
            IGenericRepository<Payment> paymentRepository,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _mealRepository = mealRepository;
            _userRepository = userRepository;
            _restaurantRepository = restaurantRepository;
            _paymentRepository = paymentRepository;
            _mapper = mapper;
        }

        // GET: api/Order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrders
            ([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var orders = _orderRepository.GetPaged(page, pageSize)
                    .Include(o => o.User)
                    .Include(o => o.Resturant)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Meal);

                var orderDTOs = _mapper.Map<IEnumerable<OrderDTO>>(orders);
                return Ok(orderDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Order/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderWithItemsDTO>> GetOrder(int id)
        {
            try
            {
                var order = _orderRepository.GetAll()
                    .Include(o => o.User)
                    .Include(o => o.Resturant)
                    .Include(o => o.Payment)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Meal)
                    .FirstOrDefault(o => o.ID == id);

                if (order == null)
                {
                    return NotFound($"Order with ID {id} not found");
                }

                var orderWithItemsDTO = _mapper.Map<OrderWithItemsDTO>(order);
                return Ok(orderWithItemsDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Order
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<OrderDTO>> CreateOrder([FromBody] CreateOrderDTO createOrderDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate user exists
                var user = await _userRepository.GetByIdAsync(createOrderDTO.UserID);
                if (user == null)
                {
                    return BadRequest($"User with ID {createOrderDTO.UserID} not found");
                }

                // Validate order items and calculate total
                decimal totalAmount = 0;
                int restaurantId = 0;
                var orderItems = new List<OrderItem>();

                foreach (var item in createOrderDTO.OrderItems)
                {
                    var meal = await _mealRepository.GetByIdAsync(item.MealID);
                    if (meal == null)
                    {
                        return BadRequest($"Meal with ID {item.MealID} not found");
                    }

                    if (restaurantId == 0)
                    {
                        restaurantId = meal.ResturantID;
                    }
                    else if (restaurantId != meal.ResturantID)
                    {
                        return BadRequest("All meals must be from the same restaurant");
                    }

                    var orderItem = new OrderItem
                    {
                        MealId = item.MealID,
                        Name = meal.Name,
                        UnitPrice = meal.Price,
                        Quantity = item.Quantity
                    };

                    orderItems.Add(orderItem);
                    totalAmount += orderItem.TotalPrice;
                }

                // Create order
                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    UserID = createOrderDTO.UserID,
                    ResturantID = restaurantId,
                    TotalAmount = totalAmount,
                    Status = Core.Entities.OrderStatus.Pending,
                    DeliveryAddress = createOrderDTO.DeliveryAddress,
                    PaymentMethod = createOrderDTO.PaymentMethod,
                    OrderItems = orderItems
                };

                await _orderRepository.AddAsync(order);

                var orderDTO = _mapper.Map<OrderDTO>(order);
                return CreatedAtAction(nameof(GetOrder), new { id = order.ID }, orderDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Order/5/status
        [Authorize(Roles = "SuperAdmin, RestaurantOwner")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus
            (int id, [FromBody] UpdateOrderStatusDTO updateStatusDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound($"Order with ID {id} not found");
                }

                order.Status = (Core.Entities.OrderStatus)updateStatusDTO.Status;

                // Update delivery time if provided
                if (updateStatusDTO.EstimatedDeliveryTime.HasValue)
                {
                    // You might want to add EstimatedDeliveryTime field to Order entity
                }

                await _orderRepository.UpdateAsync(order);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Order/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound($"Order with ID {id} not found");
                }

                // Only allow cancellation if order is still pending or confirmed
                if (order.Status != Core.Entities.OrderStatus.Pending && order.Status != Core.Entities.OrderStatus.Confirmed)
                {
                    return BadRequest("Order cannot be cancelled at this stage");
                }

                order.Status = Core.Entities.OrderStatus.Cancelled;
                await _orderRepository.UpdateAsync(order);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Order/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<OrderHistoryDTO>> GetUserOrders(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userOrders = _orderRepository.FindByCondition(o => o.UserID == userId)
                    .Include(o => o.Resturant)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Meal)
                    .OrderByDescending(o => o.OrderDate);

                var totalOrders = await userOrders.CountAsync();
                var totalSpent = await userOrders.SumAsync(o => o.TotalAmount);

                var pagedOrders = userOrders
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                var orderDTOs = _mapper.Map<List<OrderDTO>>(await pagedOrders.ToListAsync());

                var orderHistoryDTO = new OrderHistoryDTO
                {
                    UserID = userId,
                    Orders = orderDTOs,
                    TotalOrders = totalOrders,
                    TotalSpent = totalSpent
                };

                return Ok(orderHistoryDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Order/restaurant/{restaurantId}
        [HttpGet("restaurant/{restaurantId}")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetRestaurantOrders(int restaurantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var restaurantOrders = _orderRepository.FindByCondition(o => o.ResturantID == restaurantId)
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Meal)
                    .OrderByDescending(o => o.OrderDate);

                var pagedOrders = restaurantOrders
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                var orderDTOs = _mapper.Map<IEnumerable<OrderDTO>>(await pagedOrders.ToListAsync());
                return Ok(orderDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Order/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrdersByStatus(Core.Entities.OrderStatus status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var orders = _orderRepository.FindByCondition(o => o.Status == status)
                    .Include(o => o.User)
                    .Include(o => o.Resturant)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Meal)
                    .OrderByDescending(o => o.OrderDate);

                var pagedOrders = orders
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                var orderDTOs = _mapper.Map<IEnumerable<OrderDTO>>(await pagedOrders.ToListAsync());
                return Ok(orderDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Order/tracking/{id}
        [HttpGet("tracking/{id}")]
        public async Task<ActionResult<OrderTrackingDTO>> TrackOrder(int id)
        {
            try
            {
                var order = _orderRepository.GetAll()
                    .Include(o => o.User)
                    .Include(o => o.Resturant)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Meal)
                    .FirstOrDefault(o => o.ID == id);

                if (order == null)
                {
                    return NotFound($"Order with ID {id} not found");
                }

                var orderTrackingDTO = _mapper.Map<OrderTrackingDTO>(order);

                // Add status updates (you might want to create a separate table for this)
                orderTrackingDTO.StatusUpdates = new List<OrderStatusUpdateDTO>
                {
                    new OrderStatusUpdateDTO
                    {
                        Status = (Core.DTOs.OrderStatus)order.Status,
                        Timestamp = order.OrderDate,
                        Notes = "Order created"
                    }
                };

                return Ok(orderTrackingDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Order/statistics
        [HttpGet("statistics")]
        public async Task<ActionResult<OrderStatisticsDTO>> GetOrderStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _orderRepository.GetAll();

                if (startDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate <= endDate.Value);
                }

                var totalOrders = await query.CountAsync();
                var pendingOrders = await query.CountAsync(o => o.Status == Core.Entities.OrderStatus.Pending);
                var completedOrders = await query.CountAsync(o => o.Status == Core.Entities.OrderStatus.Delivered);
                var cancelledOrders = await query.CountAsync(o => o.Status == Core.Entities.OrderStatus.Cancelled);
                var totalRevenue = await query.Where(o => o.Status == Core.Entities.OrderStatus.Delivered).SumAsync(o => o.TotalAmount);
                var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

                var statistics = new OrderStatisticsDTO
                {
                    TotalOrders = totalOrders,
                    PendingOrders = pendingOrders,
                    CompletedOrders = completedOrders,
                    CancelledOrders = cancelledOrders,
                    TotalRevenue = totalRevenue,
                    AverageOrderValue = averageOrderValue
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Order/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> SearchOrders(
            [FromQuery] string? orderNumber = null,
            [FromQuery] int? userId = null,
            [FromQuery] int? restaurantId = null,
            [FromQuery] Core.Entities.OrderStatus? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _orderRepository.GetAll();

                // Apply filters
                if (!string.IsNullOrEmpty(orderNumber))
                {
                    query = query.Where(o => o.OrderNumber.Contains(orderNumber));
                }

                if (userId.HasValue)
                {
                    query = query.Where(o => o.UserID == userId.Value);
                }

                if (restaurantId.HasValue)
                {
                    query = query.Where(o => o.ResturantID == restaurantId.Value);
                }

                if (status.HasValue)
                {
                    query = query.Where(o => o.Status == status.Value);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate <= endDate.Value);
                }

                // Apply pagination
                var pagedOrders = query
                    .OrderByDescending(o => o.OrderDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                var orderDTOs = _mapper.Map<IEnumerable<OrderDTO>>(await pagedOrders.ToListAsync());
                return Ok(orderDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Helper method to generate unique order number
        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}
