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

    public async Task<List<Users>> GetUsersByTeamLeadEmailAsync(string teamLeadEmail)
    {
        return await _context.Users.Find(u => u.TeamLeadEmail == teamLeadEmail).ToListAsync();
    }

    public async Task UpdatePasswordHashAsync(string userId, string newHash, int hashVersion)
    {
        var filter = Builders<Users>.Filter.Eq(u => u.Id, userId);
        var update = Builders<Users>.Update
            .Set(u => u.PasswordHash, newHash)
            .Set(u => u.PasswordHashVersion, hashVersion);
        await _context.Users.UpdateOneAsync(filter, update);
    }

    public async Task UpdateTeamLeadApprovalStatusAsync(string userId, string status)
    {
        var filter = Builders<Users>.Filter.Eq(u => u.Id, userId);
        var update = Builders<Users>.Update.Set(u => u.TeamLeadApprovalStatus, status);
        await _context.Users.UpdateOneAsync(filter, update);
    }
}
