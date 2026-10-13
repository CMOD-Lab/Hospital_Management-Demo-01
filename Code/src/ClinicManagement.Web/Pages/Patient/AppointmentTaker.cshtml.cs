using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Patient;

/// <summary>
/// Page model for booking appointments.
/// </summary>
public class AppointmentTakerModel : PageModel
{
    private readonly IPatientService _patientService;
    private readonly ILogger<AppointmentTakerModel> _logger;

    public ClinicManagement.Domain.Entities.Doctor? Doctor { get; set; }
    public IEnumerable<Appointment> FreeSlots { get; set; } = new List<Appointment>();
    public int DoctorId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }

    public AppointmentTakerModel(IPatientService patientService, ILogger<AppointmentTakerModel> logger)
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

        DoctorId = doctorId;
        Doctor = await _patientService.GetDoctorProfileAsync(doctorId);
        FreeSlots = await _patientService.GetFreeSlotsAsync(doctorId, userId.Value);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int doctorId, int freeSlotId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 1)
            return RedirectToPage("/SignUp");

        DoctorId = doctorId;
        var (success, message) = await _patientService.BookAppointmentAsync(doctorId, userId.Value, freeSlotId);

        if (success)
        {
            return RedirectToPage("/Patient/AppointmentRequestSent");
        }

        Message = message;
        IsSuccess = false;
        Doctor = await _patientService.GetDoctorProfileAsync(doctorId);
        FreeSlots = await _patientService.GetFreeSlotsAsync(doctorId, userId.Value);
        return Page();
    }
}
