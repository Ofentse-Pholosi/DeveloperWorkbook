namespace Workbook.Infrastructure.Settings;

public class MongoDBSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string UsersCollection { get; set; } = "Users";
    public string WorkbookAnswersCollection { get; set; } = "WorkbookAnswers";
}
