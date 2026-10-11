namespace ECommerce.Infrastructure.Security;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = "SuperSecretKeyForECommerceApp2026!MustBeVeryLongAndSecure123456789";
    public string Issuer { get; set; } = "ECommerceAPI";
    public string Audience { get; set; } = "ECommerceClient";
    public int AccessTokenExpirationMinutes { get; set; } = 60; // 60 minutes
    public int RefreshTokenExpirationDays { get; set; } = 7;     // 7 days
}
