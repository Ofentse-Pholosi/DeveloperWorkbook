using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Workbook.Core.Entities;
using Workbook.Infrastructure.Settings;

namespace Workbook.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDBSettings _settings;

    public MongoDbContext(IOptions<MongoDBSettings> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
    }

    public IMongoCollection<DevUser> Users => _database.GetCollection<DevUser>(_settings.UsersCollection);
}
