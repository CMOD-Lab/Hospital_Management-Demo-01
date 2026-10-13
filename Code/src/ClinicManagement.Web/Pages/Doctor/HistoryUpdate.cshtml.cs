using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Doctor;

/// <summary>
/// Page model for updating patient history/prescription.
/// </summary>
public class HistoryUpdateModel : PageModel
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<HistoryUpdateModel> _logger;

    public IEnumerable<Appointment> TodaysAppointments { get; set; } = new List<Appointment>();
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }

    public HistoryUpdateModel(IDoctorService doctorService, ILogger<HistoryUpdateModel> logger)
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

        TodaysAppointments = await _doctorService.GetTodaysAppointmentsAsync(userId.Value);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int AppointmentId, string Disease, string Progress, string Prescription)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 2)
            return RedirectToPage("/SignUp");

        var success = await _doctorService.UpdatePrescriptionAsync(userId.Value, AppointmentId, Disease, Progress, Prescription);

        if (success)
        {
            Message = "Prescription updated successfully!";
            IsSuccess = true;
        }
        else
        {
            Message = "There was an error updating the prescription.";
            IsSuccess = false;
        }

        TodaysAppointments = await _doctorService.GetTodaysAppointmentsAsync(userId.Value);
        return Page();
    }
}
