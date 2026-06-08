using System.Text.Json;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;

namespace Workbook.WebApp.Services;

public class PerformanceReviewSectionProvider : IPerformanceReviewSectionProvider
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<PerformanceReviewSectionProvider> _logger;

    public PerformanceReviewSectionProvider(IWebHostEnvironment env, ILogger<PerformanceReviewSectionProvider> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<List<ReviewSectionConfig>> GetSectionsAsync()
    {
        try
        {
            var filePath = Path.Combine(_env.ContentRootPath, "performanceReviewSections.json");
            var json = await File.ReadAllTextAsync(filePath);
            var sections = JsonSerializer.Deserialize<List<ReviewSectionConfig>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return sections ?? new List<ReviewSectionConfig>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load performance review sections");
            return new List<ReviewSectionConfig>();
        }
    }
}
