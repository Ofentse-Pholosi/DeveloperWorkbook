namespace Workbook.Core.Entities;

public class WorkbookSection
{
    public string Title { get; set; } = string.Empty;
    public List<string> Questions { get; set; } = new();
}

