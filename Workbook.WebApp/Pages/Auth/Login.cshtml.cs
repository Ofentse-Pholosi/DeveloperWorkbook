using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Workbook.Application.Users.Commands.LoginUser;

namespace Workbook.WebApp.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IMediator _mediator;

    public LoginModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _mediator.Send(new LoginUserCommand(Input.Email, Input.Password));
        if (result)
        {
            return RedirectToPage("/Index");
        }

        ErrorMessage = "Invalid email or password.";
        return Page();
    }

    public class LoginInput
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
