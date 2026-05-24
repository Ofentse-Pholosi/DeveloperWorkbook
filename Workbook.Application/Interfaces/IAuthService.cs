using Workbook.Core.Entities;

namespace Workbook.Application.Interfaces;
public interface IAuthService
{
    Task<bool> RegisterAsync(Core.Entities.Users devUser, string password);
    Task<Core.Entities.Users?> ValidateUserAsync(string email, string password);

    /// <summary>Returns true if the given email is the TeamLeadEmail for at least one developer.</summary>
    Task<bool> IsManagerAsync(string email);

    /// <summary>
    /// Generates a 6-digit OTP, stores it hashed in the database, and returns
    /// the plain-text code so the caller can pass it to the email service.
    /// </summary>
    Task<string> GenerateOtpAsync(string email);

    /// <summary>Validates the submitted plain-text code against the stored hash.</summary>
    Task<bool> ValidateOtpAsync(string email, string code);
}
