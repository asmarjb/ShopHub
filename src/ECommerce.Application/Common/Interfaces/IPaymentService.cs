using ECommerce.Application.Common.Models;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Common.Interfaces;

public record PaymentRequestDto(
    Guid OrderId,
    decimal Amount,
    string Currency,
    PaymentProvider Provider,
    string? PaymentToken = null);

public record PaymentResultDto(
    bool Success,
    string TransactionId,
    string? Message = null);

public interface IPaymentService
{
    Task<Result<PaymentResultDto>> ProcessPaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken = default);
    Task<Result> RefundPaymentAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default);
}
