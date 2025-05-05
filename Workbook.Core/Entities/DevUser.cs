using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Workbook.Core.Entities;

public class DevUser
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordHashConfirm { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TeamLeadEmail { get; set; } = string.Empty;
    public string DevPosition { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string DivisionName { get; set; } = string.Empty;
    public DateTime DateJoinedTeam { get; set; }
}
