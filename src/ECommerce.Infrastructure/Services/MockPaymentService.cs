using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class MockPaymentService : IPaymentService
{
    private readonly ILogger<MockPaymentService> _logger;

    public MockPaymentService(ILogger<MockPaymentService> logger)
    {
        _logger = logger;
    }

    public Task<Result<PaymentResultDto>> ProcessPaymentAsync(
        PaymentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing mock payment of {Amount} {Currency} for Order {OrderId} using provider {Provider}.",
            request.Amount, request.Currency, request.OrderId, request.Provider);

        var transactionId = $"TXN-MOCK-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";

        var result = new PaymentResultDto(
            Success: true,
            TransactionId: transactionId,
            Message: "Payment successfully authorized and captured via Mock gateway.");

        return Task.FromResult(Result.Success(result));
    }

    public Task<Result> RefundPaymentAsync(
        string transactionId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing mock refund of {Amount} for Transaction {TransactionId}.",
            amount, transactionId);

        return Task.FromResult(Result.Success());
    }
}
