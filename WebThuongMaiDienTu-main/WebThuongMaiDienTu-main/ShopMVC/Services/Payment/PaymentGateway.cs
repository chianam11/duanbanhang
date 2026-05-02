namespace ShopMVC.Services.Payment
{
    /// <summary>
    /// Payment status enum
    /// </summary>
    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Refunded
    }

    /// <summary>
    /// Payment Gateway Interface - supports multiple providers
    /// </summary>
    public interface IPaymentGateway
    {
        Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request);
        Task<PaymentResponse> VerifyPaymentAsync(string transactionId);
        Task<PaymentResponse> RefundPaymentAsync(string transactionId, decimal amount);
    }

    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string Description { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Mock Payment Gateway - for testing/development
    /// </summary>
    public class MockPaymentGateway : IPaymentGateway
    {
        private readonly ILogger<MockPaymentGateway> _logger;

        public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
        {
            _logger = logger;
        }

        public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request)
        {
            _logger.LogInformation(
                "Mock payment created for order {OrderId}: {Amount} {Currency}",
                request.OrderId,
                request.Amount,
                request.Currency);

            await Task.Delay(100);

            var transactionId = $"TXN_{Guid.NewGuid().ToString().Substring(0, 8).ToUpperInvariant()}";

            return new PaymentResponse
            {
                Success = true,
                Message = "Payment created successfully (Mock)",
                TransactionId = transactionId,
                Status = PaymentStatus.Pending,
                PaymentUrl = $"https://payment.mock/checkout?txn={transactionId}"
            };
        }

        public async Task<PaymentResponse> VerifyPaymentAsync(string transactionId)
        {
            await Task.Delay(50);

            return new PaymentResponse
            {
                Success = true,
                Message = "Payment verified",
                TransactionId = transactionId,
                Status = PaymentStatus.Completed
            };
        }

        public async Task<PaymentResponse> RefundPaymentAsync(string transactionId, decimal amount)
        {
            await Task.Delay(100);

            return new PaymentResponse
            {
                Success = true,
                Message = $"Refund {amount} processed",
                TransactionId = transactionId,
                Status = PaymentStatus.Refunded
            };
        }
    }

    /// <summary>
    /// Stripe Payment Gateway (skeleton for implementation)
    /// </summary>
    public class StripePaymentGateway : IPaymentGateway
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripePaymentGateway> _logger;

        public StripePaymentGateway(IConfiguration configuration, ILogger<StripePaymentGateway> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Stripe payment session created for order {OrderId}", request.OrderId);

                await Task.Delay(100);

                return new PaymentResponse
                {
                    Success = true,
                    Message = "Payment session created",
                    Status = PaymentStatus.Pending
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe payment creation failed");
                return new PaymentResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Status = PaymentStatus.Failed
                };
            }
        }

        public async Task<PaymentResponse> VerifyPaymentAsync(string transactionId)
        {
            await Task.Delay(50);

            return new PaymentResponse
            {
                Success = true,
                Message = "Payment verified with Stripe",
                TransactionId = transactionId,
                Status = PaymentStatus.Completed
            };
        }

        public async Task<PaymentResponse> RefundPaymentAsync(string transactionId, decimal amount)
        {
            await Task.Delay(100);

            return new PaymentResponse
            {
                Success = true,
                Message = "Refund processed with Stripe",
                TransactionId = transactionId,
                Status = PaymentStatus.Refunded
            };
        }
    }
}
