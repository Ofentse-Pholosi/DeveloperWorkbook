using System.Text.Json.Serialization;

namespace Workbook.Core.Entities;

public class ReviewSectionConfig
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<string> Items { get; set; } = new();

    [JsonPropertyName("freeTextOnly")]
    public bool FreeTextOnly { get; set; } = false;
}
