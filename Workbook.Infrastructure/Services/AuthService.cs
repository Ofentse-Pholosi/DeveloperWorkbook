using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public async Task<bool> RegisterAsync(DevUser devUser, string password)
    {
        var existingUser = await _userRepository.GetUserEmailAsync(devUser.Email);
        if (existingUser != null)
        {
            return false; // User already exists
        }
        var newUser = new DevUser
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
    public async Task<DevUser?> ValidateUserAsync(string email, string password)
    {
        var user = await _userRepository.GetUserEmailAsync(email);
        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            return null; // Invalid credentials
        }
        return user;
    }
    private string HashPassword(string password)
    {
        // Implement your password hashing logic here
        return password; // Placeholder
    }
    private bool VerifyPassword(string password, string hashedPassword)
    {
        // Implement your password verification logic here
        return password == hashedPassword; // Placeholder
    }
}
