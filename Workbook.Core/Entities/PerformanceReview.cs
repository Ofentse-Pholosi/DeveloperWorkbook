using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Workbook.Core.Entities;

public class PerformanceReview
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("DeveloperEmail")]
    public string DeveloperEmail { get; set; } = string.Empty;

    [BsonElement("ManagerEmail")]
    public string ManagerEmail { get; set; } = string.Empty;

    [BsonElement("Quarter")]
    public int Quarter { get; set; } // 1–4

    [BsonElement("Year")]
    public int Year { get; set; }

    [BsonElement("Status")]
    public string Status { get; set; } = "Draft"; // "Draft" | "Submitted" | "Reviewed"

    [BsonElement("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("SubmittedAt")]
    public DateTime? SubmittedAt { get; set; }

    [BsonElement("ReviewedAt")]
    public DateTime? ReviewedAt { get; set; }

    [BsonElement("SelfAssessment")]
    public List<ReviewSection> SelfAssessment { get; set; } = new();

    [BsonElement("ManagerAssessment")]
    public ManagerAssessment? ManagerAssessment { get; set; }
}
