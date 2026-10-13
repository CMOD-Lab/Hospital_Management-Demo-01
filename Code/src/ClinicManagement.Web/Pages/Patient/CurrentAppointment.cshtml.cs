using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Patient;

/// <summary>
/// Page model for viewing current appointment.
/// </summary>
public class CurrentAppointmentModel : PageModel
{
    private readonly IPatientService _patientService;
    private readonly ILogger<CurrentAppointmentModel> _logger;

    public Appointment? Appointment { get; set; }

    public CurrentAppointmentModel(IPatientService patientService, ILogger<CurrentAppointmentModel> logger)
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

        Appointment = await _patientService.GetCurrentAppointmentAsync(userId.Value);
        return Page();
    }
}
