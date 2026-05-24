using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Workbook.Application.Interfaces;

namespace Workbook.WebApp.Pages.Auth;

public class ManagerLoginModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;

    public ManagerLoginModel(IAuthService authService, IEmailService emailService)
    {
        _authService = authService;
        _emailService = emailService;
    }

    // ── Bound properties ───────────────────────────────────────────────────
    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string OtpCode { get; set; } = string.Empty;

    /// <summary>Controls which step of the form is rendered (1 = email entry, 2 = OTP entry).</summary>
    public int Step { get; set; } = 1;

    public string? ErrorMessage { get; set; }

    // ── GET — always start at Step 1 ───────────────────────────────────────
    public void OnGet() => Step = 1;

    // ── POST Step 1: validate email is a manager, generate & send OTP ──────
    public async Task<IActionResult> OnPostSendOtpAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Please enter your email address.";
            Step = 1;
            return Page();
        }

        var isManager = await _authService.IsManagerAsync(Email);
        if (!isManager)
        {
            // Intentionally vague message to prevent user enumeration
            ErrorMessage = "If this email is registered as a team lead, you will receive an OTP shortly.";
            Step = 1;
            return Page();
        }

        var otpCode = await _authService.GenerateOtpAsync(Email);

        // Fire the OTP email (fails gracefully if SMTP not yet configured)
        await _emailService.SendOtpEmailAsync(Email, otpCode);

        // Move to step 2 — pass email forward via TempData so we can re-bind it
        TempData["OtpEmail"] = Email;
        Step = 2;
        return Page();
    }

    // ── POST Step 2: validate OTP and sign in ──────────────────────────────
    public async Task<IActionResult> OnPostVerifyOtpAsync()
    {
        // Recover email from TempData (persisted across POST redirects)
        Email = TempData["OtpEmail"] as string ?? Email;
        TempData.Keep("OtpEmail");

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(OtpCode))
        {
            ErrorMessage = "Something went wrong. Please start again.";
            Step = 1;
            return Page();
        }

        var valid = await _authService.ValidateOtpAsync(Email, OtpCode.Trim());
        if (!valid)
        {
            ErrorMessage = "The code is invalid or has expired. Please request a new one.";
            Step = 2;
            return Page();
        }

        // OTP is valid — sign the manager in with a Manager role claim
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, Email),
            new(ClaimTypes.Role, "Manager"),
            new(ClaimTypes.Name, Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = false });

        return RedirectToPage("/ManagerDashboard");
    }
}
