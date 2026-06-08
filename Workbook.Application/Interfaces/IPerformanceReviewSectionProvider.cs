using Workbook.Core.Entities;

namespace Workbook.Application.Interfaces;

public interface IPerformanceReviewSectionProvider
{
    Task<List<ReviewSectionConfig>> GetSectionsAsync();
}
