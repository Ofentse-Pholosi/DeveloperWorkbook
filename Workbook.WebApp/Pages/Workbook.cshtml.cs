using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;
using Workbook.Core.Entities;
using Workbook.Infrastructure.Data;

namespace Workbook.WebApp.Pages;

[Authorize]
public class WorkbookModel : PageModel
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly WorkbookAnswerRepository _workbookAnswerRepository;

    public WorkbookModel(IHttpContextAccessor httpContextAccessor, WorkbookAnswerRepository workbookAnswerRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _workbookAnswerRepository = workbookAnswerRepository;
    }

    public List<WorkbookSection> WorkbookSections { get; set; } = new();

    public async Task OnGetAsync()
    {
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "workbookSections.json");
        var jsonData = await System.IO.File.ReadAllTextAsync(jsonPath);
        WorkbookSections = JsonSerializer.Deserialize<List<WorkbookSection>>(jsonData) ?? new List<WorkbookSection>();

        TempData["WorkbookSections"] = JsonSerializer.Serialize(WorkbookSections);
    }

    public async Task<IActionResult> OnPostAsync(Dictionary<string, WorkbookSectionInput> sections)
    {
        if (TempData["WorkbookSections"] is string serializedSections)
        {
            WorkbookSections = JsonSerializer.Deserialize<List<WorkbookSection>>(serializedSections) ?? new List<WorkbookSection>();
        }

        var email = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        foreach (var section in sections)
        {
            var workbookAnswer = new WorkbookAnswer
            {
                Email = email,
                SectionTitle = section.Value.Title,
                Answers = section.Value.Answers ?? new Dictionary<string, string>()
            };

            foreach (var question in WorkbookSections.First(s => s.Title == section.Value.Title).Questions)
            {
                if (!workbookAnswer.Answers.ContainsKey(question))
                {
                    workbookAnswer.Answers[question] = string.Empty;
                }
            }

            await _workbookAnswerRepository.SaveWorkbookAnswerAsync(workbookAnswer);
        }

        return RedirectToPage("/Workbook");
    }

    public async Task<IActionResult> OnGetRecoverAsync()
    {
        var email = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var savedAnswers = await _workbookAnswerRepository.GetWorkbookAnswersByEmailAsync(email);
        var result = savedAnswers.ToDictionary(
            answer => answer.SectionTitle,
            answer => answer.Answers
        );

        return new JsonResult(result);
    }

    public sealed class WorkbookSectionInput
    {
        public string Title { get; set; } = string.Empty;
        public Dictionary<string, string> Answers { get; set; } = new();
    }
}
