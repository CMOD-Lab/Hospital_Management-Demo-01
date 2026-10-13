using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Doctor;

/// <summary>
/// Page model for viewing previous patient history.
/// </summary>
public class PreviousHistoryModel : PageModel
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<PreviousHistoryModel> _logger;

    public IEnumerable<Appointment> Appointments { get; set; } = new List<Appointment>();

    public PreviousHistoryModel(IDoctorService doctorService, ILogger<PreviousHistoryModel> logger)
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

        Appointments = await _doctorService.GetPatientHistoryAsync(userId.Value);
        return Page();
    }
}
