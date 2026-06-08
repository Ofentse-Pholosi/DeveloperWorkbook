namespace Workbook.Core.Entities;

public class RatedItem
{
    public string Label { get; set; } = string.Empty;
    public int Rating { get; set; } = 0; // 0 = not yet rated, 1–5 = score
    public string Notes { get; set; } = string.Empty;
}
