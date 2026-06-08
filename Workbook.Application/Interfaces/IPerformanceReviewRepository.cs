using Workbook.Core.Entities;

namespace Workbook.Application.Interfaces;

public interface IPerformanceReviewRepository
{
    Task<PerformanceReview?> GetByDeveloperQuarterAsync(string developerEmail, int quarter, int year);
    Task<List<PerformanceReview>> GetByDeveloperEmailAsync(string developerEmail);
    Task<List<PerformanceReview>> GetByManagerEmailAsync(string managerEmail);
    Task UpsertAsync(PerformanceReview review);
}
