using Workbook.Core.Entities;

namespace Workbook.Application.Interfaces;
public interface IAuthService
{
    Task<bool> RegisterAsync(DevUser devUser, string password);
    Task<DevUser?> ValidateUserAsync(string email, string password);
}
