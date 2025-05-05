using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;

namespace Workbook.Application.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RegisterUserCommandHandler(
        IUserRepository userRepository, 
        IHttpContextAccessor httpContextAccessor
    )
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Core.Entities.RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetUserEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new Core.Entities.RegisterUserResult
            {
                Success = false,
                ErrorMessage = "The email address is already in use. Please try again with a unique email address."
            };
        }

        var user = new Core.Entities.Users
        {
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            PasswordHashConfirm = HashPassword(request.PasswordConfirm),
            FirstName = request.FirstName,
            LastName = request.LastName,
            TeamLeadEmail = request.TeamLeadEmail,
            DevPosition = request.DevPosition,
            TeamName = request.TeamName,
            CompanyName = request.CompanyName,
            DivisionName = request.DivisionName,
            DateJoinedTeam = request.DateJoinedTeam
        };

        await _userRepository.CreateAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await _httpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10)
            }
        );

        return new Core.Entities.RegisterUserResult
        {
            Success = true,
            UserId = user.Id
        };
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}