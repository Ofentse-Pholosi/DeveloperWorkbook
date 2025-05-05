using MediatR;

namespace Workbook.Application.Users.Commands.RegisterUser;

public sealed class RegisterUserCommand : IRequest<string>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordConfirm { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TeamLeadEmail { get; set; } = string.Empty;
    public string DevPosition { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string DivisionName { get; set; } = string.Empty;
    public DateTime DateJoinedTeam { get; set; }
}
