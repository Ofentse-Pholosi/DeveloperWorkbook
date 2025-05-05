using Workbook.Core.Entities;

namespace Workbook.Application.Interfaces;
public interface IAuthService
{
    Task<bool> RegisterAsync(Core.Entities.Users devUser, string password);
    Task<Core.Entities.Users?> ValidateUserAsync(string email, string password);
}
