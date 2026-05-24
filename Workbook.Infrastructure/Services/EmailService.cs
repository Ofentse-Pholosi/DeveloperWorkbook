using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Workbook.Application.Interfaces;
using Workbook.Infrastructure.Settings;

namespace Workbook.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtp;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> smtp, ILogger<EmailService> logger)
    {
        _smtp = smtp.Value;
        _logger = logger;
    }

    public async Task SendFeedbackNotificationAsync(string managerEmail, string developerName, string sectionTitle)
    {
        var subject = $"📋 Workbook Update: {developerName} submitted a section for review";
        var body = $"""
            <html>
            <body style="font-family: 'Segoe UI', sans-serif; background: #f8f9fa; padding: 32px;">
              <div style="max-width: 560px; margin: 0 auto; background: #fff; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.08); overflow: hidden;">
                <div style="background: linear-gradient(135deg, #0d6efd, #6610f2); padding: 28px 32px;">
                  <h1 style="color: #fff; margin: 0; font-size: 22px;">Dev Onboarding Workbook</h1>
                </div>
                <div style="padding: 32px;">
                  <h2 style="color: #212529; margin-top: 0;">New Section Ready for Review</h2>
                  <p style="color: #495057; line-height: 1.6;">
                    <strong>{developerName}</strong> has submitted the 
                    <strong style="color: #0d6efd;">"{sectionTitle}"</strong> section of their onboarding workbook.
                    It is now awaiting your feedback.
                  </p>
                  <a href="#" style="display: inline-block; margin-top: 16px; padding: 12px 28px; background: #0d6efd; color: #fff; border-radius: 50px; text-decoration: none; font-weight: 600;">
                    Review Workbook →
                  </a>
                  <p style="margin-top: 28px; color: #adb5bd; font-size: 13px;">
                    You received this notification because you are listed as the team lead for {developerName}.
                  </p>
                </div>
              </div>
            </body>
            </html>
            """;

        await SendAsync(managerEmail, subject, body);
    }

    public async Task SendOtpEmailAsync(string managerEmail, string otpCode)
    {
        var subject = "🔐 Your Dev Workbook Login Code";
        var body = $"""
            <html>
            <body style="font-family: 'Segoe UI', sans-serif; background: #f8f9fa; padding: 32px;">
              <div style="max-width: 480px; margin: 0 auto; background: #fff; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.08); overflow: hidden;">
                <div style="background: linear-gradient(135deg, #0d6efd, #6610f2); padding: 28px 32px;">
                  <h1 style="color: #fff; margin: 0; font-size: 22px;">Dev Onboarding Workbook</h1>
                </div>
                <div style="padding: 32px; text-align: center;">
                  <h2 style="color: #212529; margin-top: 0;">Your One-Time Login Code</h2>
                  <p style="color: #495057;">Use the code below to complete your login. It expires in <strong>10 minutes</strong>.</p>
                  <div style="margin: 28px auto; display: inline-block; background: #f1f3f5; border-radius: 12px; padding: 20px 40px;">
                    <span style="font-size: 42px; font-weight: 800; letter-spacing: 12px; color: #0d6efd;">{otpCode}</span>
                  </div>
                  <p style="color: #adb5bd; font-size: 13px; margin-top: 24px;">
                    If you did not request this code, please ignore this email.
                  </p>
                </div>
              </div>
            </body>
            </html>
            """;

        await SendAsync(managerEmail, subject, body);
    }

    // -------------------------------------------------------------------------
    private async Task SendAsync(string toAddress, string subject, string htmlBody)
    {
        try
        {
            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
                EnableSsl = _smtp.EnableSsl
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_smtp.FromAddress, _smtp.FromDisplayName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toAddress);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {Recipient}: {Subject}", toAddress, subject);
        }
        catch (Exception ex)
        {
            // Gracefully degrade — log the failure but don't crash the request.
            // Swap in real SMTP credentials in appsettings.Development.json to activate.
            _logger.LogWarning(ex, "Email delivery failed for {Recipient}. SMTP may not be configured yet.", toAddress);
        }
    }
}
