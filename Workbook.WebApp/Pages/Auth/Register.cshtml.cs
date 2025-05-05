using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Workbook.Application.Users.Commands.RegisterUser;

namespace Workbook.WebApp.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly IMediator _mediator;

    public RegisterModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var command = new RegisterUserCommand
        {
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            Email = Input.Email,
            Password = Input.Password,
            PasswordConfirm = Input.PasswordConfirm,
            TeamLeadEmail = Input.TeamLeadEmail,
            DevPosition = Input.PositionName,
            TeamName = Input.TeamName,
            CompanyName = Input.CompanyName,
            DivisionName = Input.DivisionName,
            DateJoinedTeam = Input.DateJoinedTeam
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            ErrorMessage = result.ErrorMessage;
            return Page();
        }

        return RedirectToPage("/Dashboard");
    }

    public class RegisterInput
    {
        [Required]
        public string FirstName { get; set; } = "";
        [Required]
        public string LastName { get; set; } = "";
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
        [Required]
        [DataType(DataType.Password)]
        public string PasswordConfirm { get; internal set; } = "";
        [Required]
        [EmailAddress]
        public string TeamLeadEmail { get; set; } = "";
        [Required]
        public string PositionName { get; set; } = "";
        [Required]
        public string TeamName { get; set; } = "";
        [Required]
        public string CompanyName { get; set; } = "";
        [Required]
        public string DivisionName { get; set; } = "";
        [Required]
        public DateTime DateJoinedTeam { get; set; }
    }
}
