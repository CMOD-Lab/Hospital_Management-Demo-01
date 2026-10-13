using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Patient;

/// <summary>
/// Page model for patient feedback.
/// </summary>
public class PatientFeedbackModel : PageModel
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientFeedbackModel> _logger;

    public Appointment? PendingFeedback { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }

    public PatientFeedbackModel(IPatientService patientService, ILogger<PatientFeedbackModel> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 1)
            return RedirectToPage("/SignUp");

        PendingFeedback = await _patientService.GetPendingFeedbackAsync(userId.Value);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int appointmentId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 1)
            return RedirectToPage("/SignUp");

        var success = await _patientService.SubmitFeedbackAsync(appointmentId);

        if (success)
        {
            Message = "Thank you for your feedback!";
            IsSuccess = true;
        }
        else
        {
            Message = "There was an error submitting your feedback.";
            IsSuccess = false;
        }

        PendingFeedback = await _patientService.GetPendingFeedbackAsync(userId.Value);
        return Page();
    }
}
