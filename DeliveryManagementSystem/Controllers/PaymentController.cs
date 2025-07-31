using AutoMapper;
using DeliveryManagementSystem.Core.DTOs;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IGenericRepository<Payment> _paymentRepository;
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Restaurant> _resturantRepository;
        //  private readonly IPayment _paymentGatewayService;

        public PaymentController(IGenericRepository<Payment> paymentRepository,
            IMapper mapper, IGenericRepository<Order> orderRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<Restaurant> resturantRepository)
        {
            _paymentRepository = paymentRepository;
            _mapper = mapper;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _resturantRepository = resturantRepository;
        }
        // 1) Payment Processing
        #region 1) Payment Processing
        [Authorize]
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment
            ([FromBody] ProcessPaymentDTO processPaymentDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                // Validate Order exists and is in correct state
                var order = await _orderRepository.GetByIdAsync(processPaymentDTO.OrderID);
                if (order == null)
                    return BadRequest($"Order {processPaymentDTO.OrderID} not found");

                if (order.Status == Core.Entities.OrderStatus.Cancelled || order.Status == Core.Entities.OrderStatus.Delivered)
                    return BadRequest($"Cannot process payment for order with status: {order.Status}");

                // Check for existing payment
                var existingPayment = await _paymentRepository
                    .FindByCondition(p => p.OrderID == processPaymentDTO.OrderID &&
                                   p.Status != Core.Entities.PaymentStatus.Failed &&
                                   p.Status != Core.Entities.PaymentStatus.Cancelled)
                    .FirstOrDefaultAsync();

                if (existingPayment != null)
                    return BadRequest($"Active payment already exists for order {processPaymentDTO.OrderID}");

                //  Validate amount matches order total
                if (processPaymentDTO.Amount != order.TotalAmount)
                    return BadRequest($"Payment amount {processPaymentDTO.Amount} does not match order total {order.TotalAmount}");

                //  Validate payment method specific requirements
                var validationResult = ValidatePaymentMethod(processPaymentDTO);
                if (!validationResult.IsValid)
                    return BadRequest(validationResult.ErrorMessage);

                // Create payment record with pending status
                var payment = new Payment
                {
                    OrderID = processPaymentDTO.OrderID,
                    PaymentMethod = (Core.Entities.PaymentMethod)processPaymentDTO.Method,
                    Status = Core.Entities.PaymentStatus.Pending,
                    TransactionID = GenerateTransactionID(), // Generate unique ID
                    Amount = processPaymentDTO.Amount,
                    PaymentToken = processPaymentDTO.PaymentToken,
                    BillingAddress = processPaymentDTO.BillingAddress,
                    PaymentDate = DateTime.UtcNow,
                    CreatedBy = GetCurrentUserId().Result // Get from JWT token
                };

                await _paymentRepository.AddAsync(payment);
                //Process payment with external payment gateway
                var paymentResult = await ProcessWithPaymentGateway(payment, processPaymentDTO);
                // 7. Update payment status based on result
                payment.Status = paymentResult.IsSuccess ? Core.Entities.PaymentStatus.Confirmed : Core.Entities.PaymentStatus.Failed;
                payment.GatewayResponse = paymentResult.ResponseMessage;
                payment.GatewayTransactionId = paymentResult.GatewayTransactionId;
                payment.ProcessedAt = DateTime.UtcNow;

                await _paymentRepository.UpdateAsync(payment);

                // 8. Update order status if payment successful
                if (paymentResult.IsSuccess)
                {
                    order.Status = Core.Entities.OrderStatus.Confirmed;
                    await _orderRepository.UpdateAsync(order);

                    // Send confirmation email/notification
                    // await _notificationService.SendPaymentConfirmationAsync(payment);
                }

                var paymentDTO = _mapper.Map<PaymentResponseDTO>(payment);

                return paymentResult.IsSuccess
                    ? Ok(new { Success = true, Payment = paymentDTO, Message = "Payment processed successfully" })
                    : BadRequest(new { Success = false, Message = paymentResult.ErrorMessage });
            }
            //catch (PaymentGatewayException ex)
            //{
            //   // _logger.LogError(ex, "Payment gateway error for order {OrderID}", processPaymentDTO.OrderID);
            //    return StatusCode(500, new { Success = false, Message = "Payment service temporarily unavailable" });
            //}
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error processing payment for order {OrderID}", processPaymentDTO.OrderID);
                return StatusCode(500, new { Success = false, Message = "An error occurred while processing the payment" });
            }

        }

        #region Private Healper Methods
        private async Task<int> GetCurrentUserId()
        {
            // Get user ID from JWT token claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                //  _logger.LogWarning("User ID claim not found in token");
                return -1;
            }
            if (!int.TryParse(userIdClaim, out int userId))
            {
                // _logger.LogWarning("Invalid user ID format in token: {UserIdClaim}", userIdClaim);
                return -1;
            }

            // Get user from database
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                //_logger.LogWarning("User not found in database with ID: {UserId}", userId);
                return -1;
            }

            // Check if user is active
            if (!user.IsActive)
            {
                // _logger.LogWarning("Inactive user attempted to access profile: {UserId}", userId);
                return -1;
            }

            // _logger.LogInformation("Current user profile retrieved successfully for user: {UserId}", userId);
            return user.Id;
        }

        private string GenerateTransactionID()
        {
            return $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
        private PaymentValidationResult ValidatePaymentMethod(ProcessPaymentDTO dto)
        {
            switch (dto.Method)
            {
                case Core.DTOs.PaymentMethod.CreditCard:
                case Core.DTOs.PaymentMethod.DebitCard:
                    if (string.IsNullOrEmpty(dto.PaymentToken))
                        return new PaymentValidationResult { IsValid = false, ErrorMessage = "Payment token is required for card payments" };
                    break;

                case Core.DTOs.PaymentMethod.PayPal:
                    if (string.IsNullOrEmpty(dto.PaymentToken))
                        return new PaymentValidationResult { IsValid = false, ErrorMessage = "PayPal token is required" };
                    break;

                case Core.DTOs.PaymentMethod.Cash:
                    // Cash payments might need special handling
                    break;
            }
            return new PaymentValidationResult { IsValid = true };
        }

        private async Task<PaymentGatewayResult> ProcessWithPaymentGateway(Payment payment, ProcessPaymentDTO dto)
        {
            // This would integrate with actual payment gateway (Stripe, PayPal, etc.)
            //return await _paymentGatewayService.ProcessPaymentAsync(new PaymentGatewayRequest
            //{
            //    Amount = payment.Amount,
            //    PaymentMethod = payment.PaymentMethod,
            //    PaymentToken = dto.PaymentToken,
            //    TransactionId = payment.TransactionID,
            //    BillingAddress = dto.BillingAddress
            //});
            return new PaymentGatewayResult
            {
                IsSuccess = true, // Simulate success for now
                GatewayTransactionId = payment.TransactionID,
                ResponseMessage = "Payment processed successfully"
            };
        }

        #endregion


        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("confirm/{paymentId}")]
        public async Task<IActionResult> ConfirmPayment(int paymentId)
        {
            if (paymentId <= 0)
                return BadRequest("Invalid payment ID");
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(paymentId);
                if (payment == null)
                    return NotFound($"Payment with ID {paymentId} not found");

                if (payment.Status == Core.Entities.PaymentStatus.Confirmed)
                {
                    return BadRequest($"Payment with ID {paymentId} is already confirmed");
                }
                if (payment.Status != Core.Entities.PaymentStatus.Pending)
                {
                    return BadRequest($"Payment with ID {paymentId} cannot be confirmed in its current state: {payment.Status}");
                }

                payment.Status = Core.Entities.PaymentStatus.Confirmed;
                payment.ProcessedAt = DateTime.UtcNow;

                await _paymentRepository.UpdateAsync(payment);

                var paymentDTO = _mapper.Map<PaymentDTO>(payment);

                var paymentResponseDTO = new PaymentResponseDTO
                {
                    Message = $"Payment {paymentId} has been confirmed successfully",
                    Payment = paymentDTO,
                    IsSuccess = true,
                    TransactionID = GenerateTransactionID()
                };

                return Ok(paymentResponseDTO);
            }
            catch (Exception ex)
            {
                // Log the exception
                // _logger.LogError(ex, "Error confirming payment with ID {PaymentId}", paymentId);
                return StatusCode(500, "An error occurred while confirming the payment");
            }

        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid payment ID");
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
                return NotFound($"Payment with ID {id} not found");
            var paymentDTO = _mapper.Map<PaymentDTO>(payment);
            return Ok(paymentDTO);
        }
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetPaymentByOrderId(int orderId)
        {
            if (orderId <= 0)
                return BadRequest("Invalid payment ID");
            // Find payment by OrderID

            var payment = await _paymentRepository
               .FindByCondition(p => p.OrderID == orderId)
               .Include(p => p.Order)
               .Include(p => p.CreatedByUser)
               .FirstOrDefaultAsync();

            if (payment == null)
                return NotFound($"Payment for order {orderId} not found");
            var paymentDTO = _mapper.Map<PaymentDTO>(payment);
            return Ok(paymentDTO);
        }

        #endregion

        // 2) Payment History & Management
        #region Payment History & Management
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("by-status/{status}")]
        public async Task<IActionResult> GetPaymentsByStatus(string status)
        {
            try
            {
                if (!Enum.TryParse<Core.Entities.PaymentStatus>(status, true, out var parsedStatus))
                {
                    return BadRequest("Invalid payment status.");
                }
                var payments = await _paymentRepository
                   .FindByCondition(p => p.Status == parsedStatus)
                   .Include(p => p.Order)
                   .ToListAsync();

                if (!payments.Any())
                {
                    return NotFound($"No payments found with status: {status}");
                }
                var responseList = payments.Select(payment => new PaymentResponseDTO
                {
                    IsSuccess = true,
                    TransactionID = payment.TransactionID,
                    Message = "Payment retrieved successfully",
                    Payment = _mapper.Map<PaymentDTO>(payment)
                }).ToList();
                return Ok(responseList);
            }
            catch
            {
                return StatusCode(500, "An error occurred while retrieving payments by status");
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("search")]
        public async Task<IActionResult> SearchPayments([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                    return BadRequest("Search query cannot be empty");

                var payments = await _paymentRepository
                    .FindByCondition(p =>
                        p.TransactionID.Contains(query) ||
                        (p.GatewayTransactionId != null && p.GatewayTransactionId.Contains(query)) ||
                        (p.Order != null && p.Order.ID.ToString().Contains(query)))
                    .Include(p => p.Order)
                    .Include(p => p.CreatedByUser)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                var paymentDTOs = _mapper.Map<List<PaymentResponseDTO>>(payments);
                return Ok(new { Query = query, Count = paymentDTOs.Count, Payments = paymentDTOs });
            }
            catch
            {
                // _logger.LogError(ex, "Error searching payments with query {Query}", query);
                return StatusCode(500, "An error occurred while searching payments");
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("user/{userId}/payments")]
        public async Task<IActionResult> GetAllPaymentsforUser(int userId)
        {
            try
            {
                var userExists = await _userRepository
                    .FindByCondition(b => b.Id == userId)
                    .AnyAsync();

                if (!userExists)
                    return NotFound($"No Payments found for user with ID {userId}");

                var payments = await _paymentRepository
                    .FindByCondition(p => p.CreatedBy == userId)
                    .Include(p => p.Order)
                    .ToListAsync();

                var paymentDTOs = _mapper.Map<List<PaymentDTO>>(payments);
                return Ok(paymentDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving payments for the user");
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("Resturasnt/{resturasntId}/")]
        public async Task<IActionResult> GetAllPaymentsForResturasnt(int resturasntId)
        {
            try
            {
                var ResturasntExists = await _resturantRepository
                    .FindByCondition(b => b.ID == resturasntId)
                    .AnyAsync();
                if (!ResturasntExists)
                    return NotFound($"No restaurant found with ID {resturasntId}");

                var payments = await _paymentRepository
                    .FindByCondition(p => p.Order.ResturantID == resturasntId)
                    .Include(p => p.Order)
                    .ToListAsync();

                if (!payments.Any())
                    return NotFound($"No payments found for restaurant with ID {resturasntId}");
                var paymentDTOs = _mapper.Map<List<PaymentDTO>>(payments);
                return Ok(paymentDTOs);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving payments for restaurant {ResturasntId}", resturasntId);
                return StatusCode(500, "An error occurred while retrieving payments for the restaurant");
            }
        }

        #endregion

        // 3) Payment Status Updates
        #region Payment Status Updates
        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{paymentId}/status")]
        public async Task<IActionResult> UpdatePayment(int paymentId, string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                return BadRequest("New status is required.");

            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment is null)
                return NotFound($"there is no payments with id {paymentId}");

            if (!Enum.TryParse<Core.Entities.PaymentStatus>(newStatus, true, out var parsedStatus))
            {
                return BadRequest("Invalid payment status.");
            }

            if (payment.Status == Core.Entities.PaymentStatus.Cancelled
                || payment.Status == Core.Entities.PaymentStatus.Failed
                || payment.Status == Core.Entities.PaymentStatus.Refunded)
            {
                return BadRequest($"Cannot update status of a {payment.Status} payment.");
            }
            

            payment.Status = parsedStatus;

            try
            {
                await _paymentRepository.UpdateAsync(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "internal Server Error");
            }
            var paymentDTO = _mapper.Map<PaymentDTO>(payment);
            return Ok(new
            {
                Success = true,
                Message = $"Payment status updated to {newStatus}",
                Payment = paymentDTO
            });
        }
        #endregion

        // 4) Payment Methods & Configuration
        #region Payment Methods & Configuration
        [HttpGet("available-methods")]
        public IActionResult GetAvailablePaymentMethods()
        {
            try
            {
                var paymentMethods = new List<string>();
                foreach (var method in Enum.GetValues(typeof(Core.DTOs.PaymentMethod)))
                {
                    paymentMethods.Add(method.ToString());
                }
                return Ok(paymentMethods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving payment methods");
            }
        }

        #endregion

        // 6) Admin/Reporting
        #region Admin/Reporting
        // Get monthly payment reports 
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("reports/monthly")]
        public async Task<IActionResult> GetMonthlyPaymentReports()
        {
            var monthlyReports = await _paymentRepository
                .GetAll()
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalAmount = g.Sum(p => p.Amount),
                    PaymentCount = g.Count()
                })
                .OrderBy(r => r.Year)
                .ThenBy(r => r.Month)
                .ToListAsync();

            return Ok(monthlyReports);
        }

        // Get payment analytics and statistics
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("analytics")]
        public async Task<IActionResult> GetPaymentAnalytics()
        {
            var payments = await _paymentRepository.GetAll().ToListAsync();

            var analytics = new
            {
                TotalPayments = payments.Count,
                TotalAmount = payments.Sum(p => p.Amount),
                AveragePayment = payments.Any() ? payments.Average(p => p.Amount) : 0,
                StatusCounts = payments
                    .GroupBy(p => p.Status)
                    .Select(g => new
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count()
                    }),
                    MethodUsage = payments
                    .GroupBy(p => p.PaymentMethod)
                    .Select(g => new
                    {
                        Method = g.Key.ToString(),
                        Count = g.Count()
                    })
            };

            return Ok(analytics);
        }
        #endregion


        // 7) Security & Compliance
        #region Security & Compliance
        // Implement security measures like input validation, logging, etc.
        
        #endregion


    }
}
