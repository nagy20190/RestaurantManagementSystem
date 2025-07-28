using AutoMapper;
using DeliveryManagementSystem.Core.DTOs;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
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
      //  private readonly IPayment _paymentGatewayService;

        public PaymentController(IGenericRepository<Payment> paymentRepository,
            IMapper mapper, IGenericRepository<Order> orderRepository)
        {
            _paymentRepository = paymentRepository;
            _mapper = mapper;
            _orderRepository = orderRepository;

        }

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
                   // CreatedBy = _userRepository.GetCurrentUser() // Get from JWT token
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

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
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

        [HttpPost("confirm/{paymentId}")]
        public async  Task<IActionResult> ConfirmPayment(int paymentId)
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


    }
}
