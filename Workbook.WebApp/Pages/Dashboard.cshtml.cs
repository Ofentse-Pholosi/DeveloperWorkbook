using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Workbook.Application.Interfaces;
using Workbook.Core.Entities;

namespace Workbook.WebApp.Pages;

[Authorize]
public class DashboardModel : PageModel
{
    private readonly IWorkbookSectionProvider _sectionProvider;

    public List<WorkbookSection> WorkbookSections { get; set; } = new();

    public DashboardModel(IWorkbookSectionProvider sectionProvider)
    {
        _sectionProvider = sectionProvider;
    }

    public async Task OnGetAsync()
    {
        WorkbookSections = await _sectionProvider.GetSectionsAsync();
    }
}
