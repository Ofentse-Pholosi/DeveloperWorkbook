using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;

namespace Workbook.WebApp.Pages;

[Authorize]
public class QuarterlyReviewModel : PageModel
{
    private readonly IPerformanceReviewRepository _reviewRepository;
    private readonly IPerformanceReviewSectionProvider _sectionProvider;
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public QuarterlyReviewModel(
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

    public List<ReviewSectionConfig> SectionConfigs { get; set; } = new();
    public PerformanceReview? ExistingReview { get; set; }
    public int Quarter { get; set; }
    public int Year { get; set; }
    public bool IsLocked => ExistingReview?.Status is "Submitted" or "Reviewed";

    public async Task<IActionResult> OnGetAsync(int? quarter, int? year)
    {
        var now = DateTime.UtcNow;
        Quarter = quarter ?? ((now.Month - 1) / 3 + 1);
        Year = year ?? now.Year;

        SectionConfigs = await _sectionProvider.GetSectionsAsync();

        var email = GetEmail();
        if (email == null) return Unauthorized();

        ExistingReview = await _reviewRepository.GetByDeveloperQuarterAsync(email, Quarter, Year);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        string action,
        int quarter, int year,
        Dictionary<string, int> sectionItems,
        Dictionary<string, string> sectionNotes,
        Dictionary<string, string> sectionGeneral)
    {
        var email = GetEmail();
        if (email == null) return Unauthorized();

        var configs = await _sectionProvider.GetSectionsAsync();
        var user = await _userRepository.GetUserEmailAsync(email);

        var existing = await _reviewRepository.GetByDeveloperQuarterAsync(email, quarter, year);

        var review = existing ?? new PerformanceReview
        {
            DeveloperEmail = email,
            ManagerEmail = user?.TeamLeadEmail ?? string.Empty,
            Quarter = quarter,
            Year = year,
            CreatedAt = DateTime.UtcNow
        };

        // Rebuild self-assessment from submitted form indices
        var selfAssessment = new List<ReviewSection>();
        for (int i = 0; i < configs.Count; i++)
        {
            var config = configs[i];
            var section = new ReviewSection
            {
                Category = config.Category,
                GeneralNotes = sectionGeneral.GetValueOrDefault($"{i}", string.Empty),
                Items = new List<RatedItem>()
            };

            for (int j = 0; j < config.Items.Count; j++)
            {
                section.Items.Add(new RatedItem
                {
                    Label = config.Items[j],
                    Rating = config.FreeTextOnly ? 0 : sectionItems.GetValueOrDefault($"{i}_{j}", 0),
                    Notes = sectionNotes.GetValueOrDefault($"{i}_{j}", string.Empty)
                });
            }

            selfAssessment.Add(section);
        }

        review.SelfAssessment = selfAssessment;

        if (action == "Submit")
        {
            review.Status = "Submitted";
            review.SubmittedAt = DateTime.UtcNow;
        }
        else if (review.Status != "Submitted" && review.Status != "Reviewed")
        {
            review.Status = "Draft";
        }

        await _reviewRepository.UpsertAsync(review);
        return RedirectToPage(new { quarter, year });
    }

    public int GetExistingRating(int sectionIdx, int itemIdx)
    {
        if (ExistingReview == null || sectionIdx >= ExistingReview.SelfAssessment.Count) return 0;
        var section = ExistingReview.SelfAssessment[sectionIdx];
        if (itemIdx >= section.Items.Count) return 0;
        return section.Items[itemIdx].Rating;
    }

    public string GetExistingNotes(int sectionIdx, int itemIdx)
    {
        if (ExistingReview == null || sectionIdx >= ExistingReview.SelfAssessment.Count) return string.Empty;
        var section = ExistingReview.SelfAssessment[sectionIdx];
        if (itemIdx >= section.Items.Count) return string.Empty;
        return section.Items[itemIdx].Notes;
    }

    public string GetExistingGeneralNotes(int sectionIdx)
    {
        if (ExistingReview == null || sectionIdx >= ExistingReview.SelfAssessment.Count) return string.Empty;
        return ExistingReview.SelfAssessment[sectionIdx].GeneralNotes;
    }

    private string? GetEmail() =>
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
}
