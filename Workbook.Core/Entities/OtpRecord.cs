using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Workbook.Core.Entities;

public class OtpRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>SHA-256 hash of the 6-digit OTP code.</summary>
    [BsonElement("CodeHash")]
    public string CodeHash { get; set; } = string.Empty;

    [BsonElement("ExpiresAt")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>Prevents replay attacks — once validated, mark as used.</summary>
    [BsonElement("IsUsed")]
    public bool IsUsed { get; set; } = false;

    [BsonElement("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
