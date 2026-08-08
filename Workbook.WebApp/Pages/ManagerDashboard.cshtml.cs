using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;
using Workbook.Infrastructure.Data;

namespace Workbook.WebApp.Pages;

[Authorize]
public class ManagerDashboardModel : PageModel
{
    private readonly IUserRepository _userRepository;
    private readonly WorkbookAnswerRepository _answerRepository;
    private readonly IWorkbookSectionProvider _sectionProvider;
    private readonly IPerformanceReviewRepository _reviewRepository;

    public ManagerDashboardModel(
        IUserRepository userRepository,
        WorkbookAnswerRepository answerRepository,
        IWorkbookSectionProvider sectionProvider,
        IPerformanceReviewRepository reviewRepository)
    {
        _userRepository = userRepository;
        _answerRepository = answerRepository;
        _sectionProvider = sectionProvider;
        _reviewRepository = reviewRepository;
    }

    public List<DeveloperProgressInfo> DirectReports { get; set; } = new();
    public List<Users> PendingApprovals { get; set; } = new();
    public int TotalSectionsCount { get; set; }
    public int CurrentQuarter { get; set; }
    public int CurrentYear { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var now = DateTime.UtcNow;
        CurrentQuarter = (now.Month - 1) / 3 + 1;
        CurrentYear = now.Year;

        var reports = await _userRepository.GetUsersByTeamLeadEmailAsync(email);
        TotalSectionsCount = (await _sectionProvider.GetSectionsAsync()).Count;

        foreach (var report in reports)
        {
            if (report.TeamLeadApprovalStatus == "Rejected")
            {
                continue;
            }

            if (report.TeamLeadApprovalStatus == "Pending")
            {
                // Not yet confirmed — show identity only, never their submitted data.
                PendingApprovals.Add(report);
                continue;
            }

            var answers = await _answerRepository.GetWorkbookAnswersByEmailAsync(report.Email);
            var currentQReview = await _reviewRepository.GetByDeveloperQuarterAsync(report.Email, CurrentQuarter, CurrentYear);

            DirectReports.Add(new DeveloperProgressInfo
            {
                Developer = report,
                SavedSectionsCount = answers.Count,
                SubmittedSectionsCount = answers.Count(a => a.Status == "Submitted"),
                ReviewedSectionsCount = answers.Count(a => a.Status == "Reviewed"),
                CurrentQuarterReview = currentQReview
            });
        }

        return Page();
    }

    // ── Approve/Reject a pending team-lead relationship ─────────────────────
    // Re-verifies TeamLeadEmail server-side against the signed-in manager's own
    // claim rather than trusting the posted developerEmail alone, so a manager
    // can't approve/reject an arbitrary developer by tampering with the form.
    public async Task<IActionResult> OnPostApproveAsync(string developerEmail)
        => await SetApprovalStatusAsync(developerEmail, "Approved");

    public async Task<IActionResult> OnPostRejectAsync(string developerEmail)
        => await SetApprovalStatusAsync(developerEmail, "Rejected");

    private async Task<IActionResult> SetApprovalStatusAsync(string developerEmail, string status)
    {
        var managerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(managerEmail))
        {
            return Unauthorized();
        }

        var developer = await _userRepository.GetUserEmailAsync(developerEmail);
        if (developer == null || !string.Equals(developer.TeamLeadEmail, managerEmail, StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        await _userRepository.UpdateTeamLeadApprovalStatusAsync(developer.Id, status);
        return RedirectToPage();
    }

    public class DeveloperProgressInfo
    {
        public Users Developer { get; set; } = null!;
        public int SavedSectionsCount { get; set; }
        public int SubmittedSectionsCount { get; set; }
        public int ReviewedSectionsCount { get; set; }
        public PerformanceReview? CurrentQuarterReview { get; set; }
    }
}
