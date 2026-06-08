namespace Workbook.Core.Entities;

public class ReviewSection
{
    public string Category { get; set; } = string.Empty;
    public List<RatedItem> Items { get; set; } = new();
    public string GeneralNotes { get; set; } = string.Empty;
}
