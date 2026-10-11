using ECommerce.Application.Common.Models;

namespace ECommerce.Application.Common.Interfaces;

public interface IEmailService
{
    Task<Result> SendOrderConfirmationEmailAsync(string toEmail, string customerName, string orderNumber, decimal totalAmount, CancellationToken cancellationToken = default);
    Task<Result> SendEmailConfirmationAsync(string toEmail, string confirmationLink, CancellationToken cancellationToken = default);
    Task<Result> SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default);
}
