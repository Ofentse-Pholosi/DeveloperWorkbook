using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;
using Workbook.Infrastructure.Data;

namespace Workbook.WebApp.Pages;

[Authorize]
public class WorkbookModel : PageModel
{
    private readonly IWorkbookSectionProvider _sectionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly WorkbookAnswerRepository _workbookAnswerRepository;

    public WorkbookModel(
        IWorkbookSectionProvider sectionProvider,
        IHttpContextAccessor httpContextAccessor, 
        WorkbookAnswerRepository workbookAnswerRepository)
    {
        _sectionProvider = sectionProvider;
        _httpContextAccessor = httpContextAccessor;
        _workbookAnswerRepository = workbookAnswerRepository;
    }

    public List<WorkbookSection> WorkbookSections { get; set; } = new();
    public Dictionary<string, WorkbookAnswer> SavedAnswers { get; set; } = new();

    public async Task OnGetAsync()
    {
        WorkbookSections = await _sectionProvider.GetSectionsAsync();

        var email = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(email))
        {
            var answers = await _workbookAnswerRepository.GetWorkbookAnswersByEmailAsync(email);
            // Deduplicate in case old data has duplicates (fallback)
            SavedAnswers = answers
                .GroupBy(a => a.SectionTitle)
                .ToDictionary(g => g.Key, g => g.First());
        }
    }

    public async Task<IActionResult> OnPostAsync(string sectionTitle, string action, Dictionary<string, string> answers)
    {
        var email = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var existingAnswers = await _workbookAnswerRepository.GetWorkbookAnswersByEmailAsync(email);
        var existingSection = existingAnswers.FirstOrDefault(a => a.SectionTitle == sectionTitle);

        var workbookAnswer = existingSection ?? new WorkbookAnswer
        {
            Email = email,
            SectionTitle = sectionTitle
        };

        workbookAnswer.Answers = answers ?? new Dictionary<string, string>();
        
        if (action == "Submit")
        {
            workbookAnswer.Status = "Submitted";
        }
        else
        {
            workbookAnswer.Status = "Draft";
        }

        await _workbookAnswerRepository.SaveWorkbookAnswerAsync(workbookAnswer);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetRecoverAsync()
    {
        var email = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var savedAnswers = await _workbookAnswerRepository.GetWorkbookAnswersByEmailAsync(email);
        var result = savedAnswers
            .GroupBy(a => a.SectionTitle)
            .ToDictionary(
                g => g.Key,
                g => g.First().Answers
            );

        return new JsonResult(result);
    }
}
