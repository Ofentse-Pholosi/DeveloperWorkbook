using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;

namespace Workbook.Application.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IAuthService authService,
        IEmailService emailService,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _userRepository = userRepository;
        _authService = authService;
        _emailService = emailService;
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
            PasswordHash = _authService.HashPassword(request.Password),
            PasswordHashVersion = 1,
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

        if (!string.IsNullOrWhiteSpace(user.TeamLeadEmail))
        {
            // Best-effort — the team lead relationship stays "Pending" regardless,
            // so a failed send here shouldn't block registration.
            await _emailService.SendTeamLeadRequestNotificationAsync(
                user.TeamLeadEmail, $"{user.FirstName} {user.LastName}".Trim());
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await _httpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10)
            }
        );

        return new Core.Entities.RegisterUserResult
        {
            Success = true,
            UserId = user.Id
        };
    }
}