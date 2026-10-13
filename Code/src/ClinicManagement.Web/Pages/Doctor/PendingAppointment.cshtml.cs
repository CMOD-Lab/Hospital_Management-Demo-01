using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Doctor;

/// <summary>
/// Page model for pending appointments management.
/// </summary>
public class PendingAppointmentModel : PageModel
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<PendingAppointmentModel> _logger;

    public IEnumerable<Appointment> PendingAppointments { get; set; } = new List<Appointment>();
    public string Message { get; set; } = string.Empty;

    public PendingAppointmentModel(IDoctorService doctorService, ILogger<PendingAppointmentModel> logger)
    {
        _doctorService = doctorService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 2)
            return RedirectToPage("/SignUp");

        PendingAppointments = await _doctorService.GetPendingAppointmentsAsync(userId.Value);
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int appointmentId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 2)
            return RedirectToPage("/SignUp");

        var success = await _doctorService.ApproveAppointmentAsync(appointmentId);
        Message = success ? "Appointment approved successfully." : "There was an error approving the appointment.";

        PendingAppointments = await _doctorService.GetPendingAppointmentsAsync(userId.Value);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int appointmentId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 2)
            return RedirectToPage("/SignUp");

        var success = await _doctorService.DeleteAppointmentAsync(appointmentId);
        Message = success ? "Appointment deleted successfully." : "There was an error deleting the appointment.";

        PendingAppointments = await _doctorService.GetPendingAppointmentsAsync(userId.Value);
        return Page();
    }
}
