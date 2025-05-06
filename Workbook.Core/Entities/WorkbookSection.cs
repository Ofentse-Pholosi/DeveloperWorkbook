using System.Text.Json.Serialization;

namespace Workbook.Core.Entities;

public class WorkbookSection
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    [JsonPropertyName("questions")]
    public List<string>? Questions { get; set; }
}

