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

    public ManagerDashboardModel(
        IUserRepository userRepository,
        WorkbookAnswerRepository answerRepository,
        IWorkbookSectionProvider sectionProvider)
    {
        _userRepository = userRepository;
        _answerRepository = answerRepository;
        _sectionProvider = sectionProvider;
    }

    public List<DeveloperProgressInfo> DirectReports { get; set; } = new();
    public int TotalSectionsCount { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var reports = await _userRepository.GetUsersByTeamLeadEmailAsync(email);
        TotalSectionsCount = (await _sectionProvider.GetSectionsAsync()).Count;

        foreach (var report in reports)
        {
            var answers = await _answerRepository.GetWorkbookAnswersByEmailAsync(report.Email);

            DirectReports.Add(new DeveloperProgressInfo
            {
                Developer = report,
                SavedSectionsCount = answers.Count,
                SubmittedSectionsCount = answers.Count(a => a.Status == "Submitted"),
                ReviewedSectionsCount = answers.Count(a => a.Status == "Reviewed")
            });
        }

        return Page();
    }

    public class DeveloperProgressInfo
    {
        public Users Developer { get; set; } = null!;
        public int SavedSectionsCount { get; set; }
        public int SubmittedSectionsCount { get; set; }
        public int ReviewedSectionsCount { get; set; }
    }
}
