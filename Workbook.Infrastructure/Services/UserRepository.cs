using MongoDB.Driver;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;
using Workbook.Infrastructure.Data;

namespace Workbook.Infrastructure.Services;

public class UserRepository : IUserRepository
{
    private readonly MongoDbContext _context;

    public UserRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Users?> GetUserEmailAsync(string email)
    {
        return await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Users user)
    {
        await _context.Users.InsertOneAsync(user);
    }
}
