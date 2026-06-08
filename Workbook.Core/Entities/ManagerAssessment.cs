namespace Workbook.Core.Entities;

public class ManagerAssessment
{
    public int OverallRating { get; set; } = 0; // 0 = not yet rated, 1–5
    public string Strengths { get; set; } = string.Empty;
    public string ImprovementAreas { get; set; } = string.Empty;
    public string NextQuarterGoals { get; set; } = string.Empty;
    public string PerformanceTrajectory { get; set; } = string.Empty; // "OnTrack" | "NeedsSupport" | "ExceedingExpectations"
    public string AdditionalFeedback { get; set; } = string.Empty;
}
