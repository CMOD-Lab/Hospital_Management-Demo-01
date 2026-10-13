using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Patient;

/// <summary>
/// Page model for viewing a doctor's profile.
/// </summary>
public class DoctorProfileModel : PageModel
{
    private readonly IPatientService _patientService;
    private readonly ILogger<DoctorProfileModel> _logger;

    public ClinicManagement.Domain.Entities.Doctor? Doctor { get; set; }

    public DoctorProfileModel(IPatientService patientService, ILogger<DoctorProfileModel> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(int doctorId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 1)
            return RedirectToPage("/SignUp");

        Doctor = await _patientService.GetDoctorProfileAsync(doctorId);
        return Page();
    }
}
