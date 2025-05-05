using System.Text.Json;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;

namespace Workbook.WebApp.Services;

public class WorkbookSectionProvider : IWorkbookSectionProvider
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<WorkbookSectionProvider> _logger;

    public WorkbookSectionProvider(IWebHostEnvironment env, ILogger<WorkbookSectionProvider> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<List<WorkbookSection>> GetSectionsAsync()
    {
        try
        {
            var filePath = Path.Combine(_env.ContentRootPath, "Config", "workbookSections.json");
            var json = await File.ReadAllTextAsync(filePath);

            var sections = JsonSerializer.Deserialize<List<WorkbookSection>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return sections ?? new List<WorkbookSection>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load workbook sections");
            return new List<WorkbookSection>();
        }
    }
}
