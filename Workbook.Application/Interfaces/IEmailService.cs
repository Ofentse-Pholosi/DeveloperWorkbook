namespace Workbook.Application.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Sends an email to the manager notifying them that a developer has
    /// submitted a workbook section for review.
    /// </summary>
    Task SendFeedbackNotificationAsync(string managerEmail, string developerName, string sectionTitle);

    /// <summary>
    /// Sends a One-Time Passcode to the manager's email address so they can
    /// complete the two-step login process.
    /// </summary>
    Task SendOtpEmailAsync(string managerEmail, string otpCode);

    /// <summary>
    /// Sends an email to a newly-named team lead letting them know a developer
    /// has listed them and that the relationship is awaiting their confirmation.
    /// </summary>
    Task SendTeamLeadRequestNotificationAsync(string managerEmail, string developerName);
}
