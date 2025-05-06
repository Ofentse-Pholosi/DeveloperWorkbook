using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Workbook.Core.Entities
{
    public class WorkbookAnswer
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("Email")]
        public string Email { get; set; } = string.Empty; // Associate answers with the user's email

        [BsonElement("SectionTitle")]
        public string SectionTitle { get; set; } = string.Empty;

        [BsonElement("Answers")]
        public Dictionary<string, string> Answers { get; set; } = new();
    }
}
