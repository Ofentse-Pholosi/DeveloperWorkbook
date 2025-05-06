using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Workbook.Application.Interfaces;

namespace Workbook.Application.Users.Commands.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, bool>
{
    private readonly IAuthService _authService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoginUserCommandHandler(IAuthService authService, IHttpContextAccessor httpContextAccessor)
    {
        _authService = authService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _authService.ValidateUserAsync(request.Email, request.Password);
        if (user == null)
            return false;

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
            });

        return true;
    }
}