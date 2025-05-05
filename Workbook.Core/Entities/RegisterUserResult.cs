namespace Workbook.Core.Entities;

public class RegisterUserResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? UserId { get; set; }
}
