using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;

namespace Workbook.WebApp.Pages;

[Authorize]
public class DashboardModel : PageModel
{
    private readonly IWorkbookSectionProvider _sectionProvider;

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public List<WorkbookSection> WorkbookSections { get; set; } = new();

    public DashboardModel(IWorkbookSectionProvider sectionProvider)
    {
        _sectionProvider = sectionProvider;
    }

    public async Task OnGetAsync()
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        if (claimsIdentity != null)
        {
            FirstName = claimsIdentity.FindFirst(ClaimTypes.GivenName)?.Value ?? "User";
            LastName = claimsIdentity.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty;
        }

        WorkbookSections = await _sectionProvider.GetSectionsAsync();
    }
}
