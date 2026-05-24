using System.Security.Cryptography;
using System.Text;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;

namespace Workbook.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;

    public AuthService(IUserRepository userRepository, IOtpRepository otpRepository)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
    }

    // ── Standard registration ──────────────────────────────────────────────
    public async Task<bool> RegisterAsync(Users devUser, string password)
    {
        var existingUser = await _userRepository.GetUserEmailAsync(devUser.Email);
        if (existingUser != null)
        {
            return false; // User already exists
        }
        var newUser = new Users
        {
            Email = devUser.Email,
            PasswordHash = HashPassword(password),
            TeamLeadEmail = devUser.TeamLeadEmail,
            DevPosition = devUser.DevPosition,
            TeamName = devUser.TeamName,
            CompanyName = devUser.CompanyName,
            DivisionName = devUser.DivisionName,
            DateJoinedTeam = devUser.DateJoinedTeam
        };

        await _userRepository.CreateAsync(newUser);
        return true;
    }

    // ── Standard login validation ──────────────────────────────────────────
    public async Task<Users?> ValidateUserAsync(string email, string password)
    {
        var user = await _userRepository.GetUserEmailAsync(email);
        if (user != null && !VerifyPassword(password, user.PasswordHash))
        {
            return null; // Invalid credentials
        }
        return user;
    }

    // ── Manager check ──────────────────────────────────────────────────────
    public async Task<bool> IsManagerAsync(string email)
    {
        var reports = await _userRepository.GetUsersByTeamLeadEmailAsync(email);
        return reports.Any();
    }

    // ── OTP generation ─────────────────────────────────────────────────────
    public async Task<string> GenerateOtpAsync(string email)
    {
        // Generate a cryptographically random 6-digit code
        var code = RandomNumberGenerator.GetInt32(100_000, 999_999).ToString();

        var record = new OtpRecord
        {
            Email = email,
            CodeHash = HashPassword(code),           // reuse same SHA-256 helper
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _otpRepository.SaveOtpAsync(record);
        return code; // return plain-text so caller can email it
    }

    // ── OTP validation ─────────────────────────────────────────────────────
    public async Task<bool> ValidateOtpAsync(string email, string code)
    {
        var record = await _otpRepository.GetLatestOtpAsync(email);
        if (record == null)
            return false;

        var inputHash = HashPassword(code);
        if (!string.Equals(record.CodeHash, inputHash, StringComparison.Ordinal))
            return false;

        // Mark as used to prevent replay attacks
        await _otpRepository.MarkOtpUsedAsync(record.Id);
        return true;
    }

    // ── Private helpers ───────────────────────── ───────────────────────────
    private static string HashPassword(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string hashedPassword)
    {
        return string.Equals(HashPassword(password), hashedPassword, StringComparison.Ordinal);
    }
}
