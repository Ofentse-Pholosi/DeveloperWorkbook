using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;

namespace Workbook.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;

    // PBKDF2-HMAC-SHA256, salted, 100k iterations by default. The `Users` type
    // parameter is unused by the default implementation — it's only there for
    // API extensibility, so a throwaway instance is fine when hashing standalone.
    private static readonly PasswordHasher<Users> _hasher = new();

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
            PasswordHashVersion = 1,
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

    /// <summary>Hashes a new plain-text password using the current (strong) algorithm.</summary>
    public string HashPassword(string password) => _hasher.HashPassword(new Users(), password);

    // ── Standard login validation ────────────────────────────────────────────
    // Supports two hash generations so existing users aren't locked out:
    //   v1 (PasswordHashVersion == 1) → verified with PasswordHasher (PBKDF2).
    //   v0 (legacy, no version stored) → verified with the old unsalted SHA-256
    //       check; on success, silently rehashed to v1 and persisted so the
    //       account is upgraded the moment its owner next logs in.
    public async Task<Users?> ValidateUserAsync(string email, string password)
    {
        var user = await _userRepository.GetUserEmailAsync(email);
        if (user == null)
            return null;

        if (user.PasswordHashVersion == 1)
        {
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
                return null;

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = HashPassword(password);
                await _userRepository.UpdatePasswordHashAsync(user.Id, user.PasswordHash, 1);
            }

            return user;
        }

        // Legacy (v0) path.
        if (!VerifyLegacyPassword(password, user.PasswordHash))
            return null;

        user.PasswordHash = HashPassword(password);
        user.PasswordHashVersion = 1;
        await _userRepository.UpdatePasswordHashAsync(user.Id, user.PasswordHash, 1);
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
            CodeHash = LegacyHash(code),              // short-lived, single-use — SHA-256 is fine here
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

        var inputHash = LegacyHash(code);
        if (!string.Equals(record.CodeHash, inputHash, StringComparison.Ordinal))
            return false;

        // Mark as used to prevent replay attacks
        await _otpRepository.MarkOtpUsedAsync(record.Id);
        return true;
    }

    // ── Private helpers ───────────────────────── ───────────────────────────
    // Unsalted single-round SHA-256. No longer used for new password hashes
    // (see HashPassword/_hasher above) — kept only to (a) verify a v0/legacy
    // user's password on their first post-migration login, so it can be
    // transparently upgraded, and (b) hash short-lived OTP codes, where this
    // algorithm's weaknesses don't meaningfully apply.
    private static string LegacyHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyLegacyPassword(string password, string hashedPassword)
    {
        return string.Equals(LegacyHash(password), hashedPassword, StringComparison.Ordinal);
    }
}
