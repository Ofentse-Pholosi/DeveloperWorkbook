using MongoDB.Driver;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;

namespace Workbook.Infrastructure.Data;

public class PerformanceReviewRepository : IPerformanceReviewRepository
{
    private readonly IMongoCollection<PerformanceReview> _reviews;

    public PerformanceReviewRepository(MongoDbContext dbContext)
    {
        _reviews = dbContext.PerformanceReviews;
    }

    public async Task<PerformanceReview?> GetByDeveloperQuarterAsync(string developerEmail, int quarter, int year)
    {
        return await _reviews
            .Find(r => r.DeveloperEmail == developerEmail && r.Quarter == quarter && r.Year == year)
            .FirstOrDefaultAsync();
    }

    public async Task<List<PerformanceReview>> GetByDeveloperEmailAsync(string developerEmail)
    {
        return await _reviews
            .Find(r => r.DeveloperEmail == developerEmail)
            .SortByDescending(r => r.Year)
            .ThenByDescending(r => r.Quarter)
            .ToListAsync();
    }

    public async Task<List<PerformanceReview>> GetByManagerEmailAsync(string managerEmail)
    {
        return await _reviews
            .Find(r => r.ManagerEmail == managerEmail)
            .SortByDescending(r => r.Year)
            .ThenByDescending(r => r.Quarter)
            .ToListAsync();
    }

    public async Task UpsertAsync(PerformanceReview review)
    {
        var filter = Builders<PerformanceReview>.Filter.And(
            Builders<PerformanceReview>.Filter.Eq(r => r.DeveloperEmail, review.DeveloperEmail),
            Builders<PerformanceReview>.Filter.Eq(r => r.Quarter, review.Quarter),
            Builders<PerformanceReview>.Filter.Eq(r => r.Year, review.Year)
        );
        await _reviews.ReplaceOneAsync(filter, review, new ReplaceOptions { IsUpsert = true });
    }
}
