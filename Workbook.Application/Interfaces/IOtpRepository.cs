using Workbook.Core.Entities;

namespace Workbook.Application.Interfaces;

public interface IOtpRepository
{
    Task SaveOtpAsync(OtpRecord record);

    /// <summary>Returns the most recent unused, unexpired OTP for the given email.</summary>
    Task<OtpRecord?> GetLatestOtpAsync(string email);

    Task MarkOtpUsedAsync(string otpId);
}
