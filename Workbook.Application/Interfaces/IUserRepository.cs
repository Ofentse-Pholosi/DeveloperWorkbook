using Workbook.Core.Entities;

namespace Workbook.Application.Interfaces;
public interface IUserRepository
{
    Task<DevUser?> GetUserEmailAsync(string email);
    Task CreateAsync(DevUser user);
}
