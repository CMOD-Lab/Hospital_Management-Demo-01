using ClinicManagement.Domain.Enums;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages;

/// <summary>
/// Page model for the login and patient registration page.
/// </summary>
public class SignUpModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly ILogger<SignUpModel> _logger;

    public string LoginMessage { get; set; } = string.Empty;
    public string SignupMessage { get; set; } = string.Empty;
    public bool SignupSuccess { get; set; }

    public SignUpModel(IAuthService authService, ILogger<SignUpModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public void OnGet()
    {
        // Clear session on page load (logout behavior)
        HttpContext.Session.Clear();
    }

    /// <summary>
    /// Handles login form submission.
    /// </summary>
    public async Task<IActionResult> OnPostLoginAsync(string loginEmail, string loginPassword)
    {
        if (string.IsNullOrWhiteSpace(loginEmail) || string.IsNullOrWhiteSpace(loginPassword))
        {
            LoginMessage = "Please enter both email and password.";
            return Page();
        }

        var (success, userType, userId, message) = await _authService.LoginAsync(loginEmail, loginPassword);

        if (success)
        {
            HttpContext.Session.SetInt32("UserId", userId);
            HttpContext.Session.SetInt32("UserType", (int)userType);

            return userType switch
            {
                UserType.Patient => RedirectToPage("/Patient/PatientHome"),
                UserType.Doctor => RedirectToPage("/Doctor/DoctorHome"),
                UserType.Admin => RedirectToPage("/Admin/AdminHome"),
                _ => RedirectToPage("/SignUp")
            };
        }

        LoginMessage = message;
        return Page();
    }

    /// <summary>
    /// Handles patient registration form submission.
    /// </summary>
    public async Task<IActionResult> OnPostSignupAsync(
        string sName, string sBirthDate, string sEmail, string sPassword,
        string sPhone, string sAddress, string sGender)
    {
        if (string.IsNullOrWhiteSpace(sName) || string.IsNullOrWhiteSpace(sEmail) ||
            string.IsNullOrWhiteSpace(sPassword))
        {
            SignupMessage = "Please fill in all required fields.";
            return Page();
        }

        var (success, patientId, message) = await _authService.RegisterPatientAsync(
            sName, sBirthDate, sEmail, sPassword, sPhone, sGender, sAddress);

        if (success)
        {
            HttpContext.Session.SetInt32("UserId", patientId);
            HttpContext.Session.SetInt32("UserType", (int)UserType.Patient);
            return RedirectToPage("/Patient/PatientHome");
        }

        SignupMessage = message;
        SignupSuccess = false;
        return Page();
    }
}
