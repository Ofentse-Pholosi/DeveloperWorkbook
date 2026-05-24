using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;
using Workbook.Infrastructure.Data;

namespace Workbook.WebApp.Pages;

[Authorize]
public class ManagerReviewModel : PageModel
{
    private readonly IUserRepository _userRepository;
    private readonly WorkbookAnswerRepository _answerRepository;
    private readonly IWorkbookSectionProvider _sectionProvider;

    public ManagerReviewModel(
        IUserRepository userRepository,
        WorkbookAnswerRepository answerRepository,
        IWorkbookSectionProvider sectionProvider)
    {
        _userRepository = userRepository;
        _answerRepository = answerRepository;
        _sectionProvider = sectionProvider;
    }

    [BindProperty(SupportsGet = true)]
    public string Email { get; set; } = string.Empty;

    public Users Developer { get; set; } = null!;
    public List<WorkbookSection> WorkbookSections { get; set; } = new();
    public Dictionary<string, WorkbookAnswer> SavedAnswers { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var managerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(managerEmail))
        {
            return Unauthorized();
        }

        if (string.IsNullOrEmpty(Email))
        {
            return RedirectToPage("/ManagerDashboard");
        }

        var devUser = await _userRepository.GetUserEmailAsync(Email);
        if (devUser == null || devUser.TeamLeadEmail != managerEmail)
        {
            // Unauthorized check - cannot view workbooks of users who are not direct reports
            return Forbid();
        }

        Developer = devUser;
        WorkbookSections = await _sectionProvider.GetSectionsAsync();
        
        var answers = await _answerRepository.GetWorkbookAnswersByEmailAsync(Email);
        SavedAnswers = answers
            .GroupBy(a => a.SectionTitle)
            .ToDictionary(g => g.Key, g => g.First());

        return Page();
    }

    public async Task<IActionResult> OnPostSubmitFeedbackAsync(string sectionTitle, string feedbackText)
    {
        var managerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(managerEmail))
        {
            return Unauthorized();
        }

        var devUser = await _userRepository.GetUserEmailAsync(Email);
        if (devUser == null || devUser.TeamLeadEmail != managerEmail)
        {
            return Forbid();
        }

        var answers = await _answerRepository.GetWorkbookAnswersByEmailAsync(Email);
        var existingAnswer = answers.FirstOrDefault(a => a.SectionTitle == sectionTitle);

        var answerToUpdate = existingAnswer ?? new WorkbookAnswer
        {
            Email = Email,
            SectionTitle = sectionTitle,
            Status = "Draft" // Default fallback
        };

        answerToUpdate.ManagerFeedback = feedbackText ?? string.Empty;
        answerToUpdate.FeedbackDate = DateTime.UtcNow;
        answerToUpdate.Status = "Reviewed";

        await _answerRepository.SaveWorkbookAnswerAsync(answerToUpdate);

        return RedirectToPage(new { email = Email });
    }
}
