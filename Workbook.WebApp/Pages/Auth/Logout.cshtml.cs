using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Workbook.WebApp.Pages.Auth
{
    [Authorize]
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToPage("/Account/Login");
        }
    }
}
