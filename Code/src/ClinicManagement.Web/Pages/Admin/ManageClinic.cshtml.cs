using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Admin;

/// <summary>
/// Page model for managing clinic staff, doctors, and patients.
/// </summary>
public class ManageClinicModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly ILogger<ManageClinicModel> _logger;

    public string Category { get; set; } = "DOCTOR";
    public string SearchQuery { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public IEnumerable<ClinicManagement.Domain.Entities.Doctor> Doctors { get; set; } = new List<ClinicManagement.Domain.Entities.Doctor>();
    public IEnumerable<ClinicManagement.Domain.Entities.Patient> Patients { get; set; } = new List<ClinicManagement.Domain.Entities.Patient>();
    public IEnumerable<OtherStaff> Staff { get; set; } = new List<OtherStaff>();

    public ManageClinicModel(IAdminService adminService, ILogger<ManageClinicModel> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string category = "DOCTOR", string searchQuery = "")
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 3)
            return RedirectToPage("/SignUp");

        Category = category;
        SearchQuery = searchQuery;

        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, string category)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 3)
            return RedirectToPage("/SignUp");

        Category = category;

        if (category == "DOCTOR")
        {
            var success = await _adminService.DeleteDoctorAsync(id);
            Message = success ? $"Doctor No: {id} deleted successfully." : "There was some error.";
        }
        else if (category == "PATIENT")
        {
            Message = "You are not authorized to delete a patient.";
        }
        else
        {
            var success = await _adminService.DeleteStaffAsync(id);
            Message = success ? $"Staff No: {id} deleted successfully." : "There was some error.";
        }

        await LoadDataAsync();
        return Page();
    }

    private async Task LoadDataAsync()
    {
        if (Category == "DOCTOR")
        {
            Doctors = await _adminService.GetDoctorsAsync(SearchQuery);
        }
        else if (Category == "PATIENT")
        {
            Patients = await _adminService.GetPatientsAsync(SearchQuery);
        }
        else
        {
            Staff = await _adminService.GetStaffAsync(SearchQuery);
        }
    }
}
