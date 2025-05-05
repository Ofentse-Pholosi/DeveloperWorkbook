using Workbook.Core.Entities;

namespace Workbook.Application.Interfaces;
public interface IUserRepository
{
    Task<Core.Entities.Users?> GetUserEmailAsync(string email);
    Task CreateAsync(Core.Entities.Users user);
}
