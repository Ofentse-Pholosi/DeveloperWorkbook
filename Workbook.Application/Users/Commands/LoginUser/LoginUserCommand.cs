using MediatR;

namespace Workbook.Application.Users.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<bool>;

