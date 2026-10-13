using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Doctor;

/// <summary>
/// Page model for generating and managing bills.
/// </summary>
public class BillModel : PageModel
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<BillModel> _logger;

    public IEnumerable<Appointment> TodaysAppointments { get; set; } = new List<Appointment>();
    public string Message { get; set; } = string.Empty;

    public BillModel(IDoctorService doctorService, ILogger<BillModel> logger)
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

    public async Task<IActionResult> OnPostPaidAsync(int appointmentId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 2)
            return RedirectToPage("/SignUp");

        await _doctorService.MarkBillPaidAsync(userId.Value, appointmentId);
        Message = "Bill marked as paid.";

        TodaysAppointments = await _doctorService.GetTodaysAppointmentsAsync(userId.Value);
        return Page();
    }

    public async Task<IActionResult> OnPostUnpaidAsync(int appointmentId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 2)
            return RedirectToPage("/SignUp");

        await _doctorService.MarkBillUnpaidAsync(userId.Value, appointmentId);
        Message = "Bill marked as unpaid.";

        TodaysAppointments = await _doctorService.GetTodaysAppointmentsAsync(userId.Value);
        return Page();
    }
}
