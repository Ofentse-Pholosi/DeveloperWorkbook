using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Workbook.WebApp.Pages;

[Authorize]
public class DashboardModel : PageModel
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    public void OnGet()
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        if (claimsIdentity != null)
        {
            FirstName = claimsIdentity.FindFirst(ClaimTypes.GivenName)?.Value ?? "Fellow Dev";
            LastName = claimsIdentity.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty;
        }
    }
}
