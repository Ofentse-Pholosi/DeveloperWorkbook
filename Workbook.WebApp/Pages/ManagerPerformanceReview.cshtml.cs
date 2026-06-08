using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;

namespace Workbook.WebApp.Pages;

[Authorize]
public class ManagerPerformanceReviewModel : PageModel
{
    private readonly IPerformanceReviewRepository _reviewRepository;
    private readonly IPerformanceReviewSectionProvider _sectionProvider;
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ManagerPerformanceReviewModel(
        IPerformanceReviewRepository reviewRepository,
        IPerformanceReviewSectionProvider sectionProvider,
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _reviewRepository = reviewRepository;
        _sectionProvider = sectionProvider;
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public Users? Developer { get; set; }
    public PerformanceReview? Review { get; set; }
    public List<ReviewSectionConfig> SectionConfigs { get; set; } = new();
    public string DeveloperEmail { get; set; } = string.Empty;
    public int Quarter { get; set; }
    public int Year { get; set; }

    public async Task<IActionResult> OnGetAsync(string email, int quarter, int year)
    {
        DeveloperEmail = email;
        Quarter = quarter;
        Year = year;

        Developer = await _userRepository.GetUserEmailAsync(email);
        if (Developer == null) return NotFound();

        SectionConfigs = await _sectionProvider.GetSectionsAsync();
        Review = await _reviewRepository.GetByDeveloperQuarterAsync(email, quarter, year);

        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAssessmentAsync(
        string developerEmail, int quarter, int year,
        int overallRating,
        string strengths, string improvementAreas, string nextQuarterGoals,
        string performanceTrajectory, string additionalFeedback)
    {
        var managerEmail = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

        var existing = await _reviewRepository.GetByDeveloperQuarterAsync(developerEmail, quarter, year);

        // If developer hasn't started yet, manager can still create and complete the review
        if (existing == null)
        {
            existing = new PerformanceReview
            {
                DeveloperEmail = developerEmail,
                ManagerEmail = managerEmail ?? string.Empty,
                Quarter = quarter,
                Year = year,
                CreatedAt = DateTime.UtcNow
            };
        }

        existing.ManagerAssessment = new ManagerAssessment
        {
            OverallRating = overallRating,
            Strengths = strengths ?? string.Empty,
            ImprovementAreas = improvementAreas ?? string.Empty,
            NextQuarterGoals = nextQuarterGoals ?? string.Empty,
            PerformanceTrajectory = performanceTrajectory ?? string.Empty,
            AdditionalFeedback = additionalFeedback ?? string.Empty
        };

        existing.Status = "Reviewed";
        existing.ReviewedAt = DateTime.UtcNow;

        await _reviewRepository.UpsertAsync(existing);
        return RedirectToPage(new { email = developerEmail, quarter, year });
    }
}
