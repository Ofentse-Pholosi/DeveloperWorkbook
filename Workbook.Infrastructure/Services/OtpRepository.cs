using MongoDB.Driver;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;
using Workbook.Infrastructure.Data;

namespace Workbook.Infrastructure.Services;

public class OtpRepository : IOtpRepository
{
    private readonly IMongoCollection<OtpRecord> _otps;

    public OtpRepository(MongoDbContext context)
    {
        _otps = context.Otps;
    }

    public async Task SaveOtpAsync(OtpRecord record)
    {
        await _otps.InsertOneAsync(record);
    }

    public async Task<OtpRecord?> GetLatestOtpAsync(string email)
    {
        var now = DateTime.UtcNow;
        return await _otps
            .Find(r => r.Email == email && !r.IsUsed && r.ExpiresAt > now)
            .SortByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task MarkOtpUsedAsync(string otpId)
    {
        var filter = Builders<OtpRecord>.Filter.Eq(r => r.Id, otpId);
        var update = Builders<OtpRecord>.Update.Set(r => r.IsUsed, true);
        await _otps.UpdateOneAsync(filter, update);
    }
}
