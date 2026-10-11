using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task<Result> SendOrderConfirmationEmailAsync(
        string toEmail,
        string customerName,
        string orderNumber,
        decimal totalAmount,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("""
            ========================================================================
            [EMAIL NOTIFICATION - ORDER CONFIRMATION]
            To: {ToEmail}
            Dear {CustomerName},
            Thank you for your order! Your order #{OrderNumber} has been received.
            Total Amount: ${TotalAmount:F2}
            ========================================================================
            """, toEmail, customerName, orderNumber, totalAmount);

        return Task.FromResult(Result.Success());
    }

    public Task<Result> SendEmailConfirmationAsync(
        string toEmail,
        string confirmationLink,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("""
            ========================================================================
            [EMAIL NOTIFICATION - CONFIRM EMAIL]
            To: {ToEmail}
            Please confirm your email address by clicking the link below:
            {ConfirmationLink}
            ========================================================================
            """, toEmail, confirmationLink);

        return Task.FromResult(Result.Success());
    }

    public Task<Result> SendPasswordResetEmailAsync(
        string toEmail,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("""
            ========================================================================
            [EMAIL NOTIFICATION - PASSWORD RESET]
            To: {ToEmail}
            Reset your password by following this link:
            {ResetLink}
            ========================================================================
            """, toEmail, resetLink);

        return Task.FromResult(Result.Success());
    }
}
