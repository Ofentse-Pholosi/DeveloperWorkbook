using System.Security.Cryptography;
using System.Text;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;

namespace Workbook.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
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
    public async Task<Users?> ValidateUserAsync(string email, string password)
    {
        var user = await _userRepository.GetUserEmailAsync(email);
        if (user != null && !VerifyPassword(password, user.PasswordHash))
        {
            return null; // Invalid credentials
        }
        return user;
    }
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
    private bool VerifyPassword(string password, string hashedPassword)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        var inputHashedPassword = Convert.ToBase64String(hash);

        return inputHashedPassword == hashedPassword;
    }
}
