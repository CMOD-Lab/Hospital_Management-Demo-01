using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Patient;

/// <summary>
/// Page model for viewing doctors by department.
/// </summary>
public class ViewDoctorsModel : PageModel
{
    private readonly IPatientService _patientService;
    private readonly ILogger<ViewDoctorsModel> _logger;

    public IEnumerable<Department> Departments { get; set; } = new List<Department>();
    public IEnumerable<ClinicManagement.Domain.Entities.Doctor> Doctors { get; set; } = new List<ClinicManagement.Domain.Entities.Doctor>();
    public string SelectedDept { get; set; } = string.Empty;

    public ViewDoctorsModel(IPatientService patientService, ILogger<ViewDoctorsModel> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string deptName = "")
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 1)
            return RedirectToPage("/SignUp");

        Departments = await _patientService.GetDepartmentsAsync();
        SelectedDept = deptName;

        if (!string.IsNullOrEmpty(deptName))
        {
            Doctors = await _patientService.GetDoctorsByDepartmentAsync(deptName);
        }

        return Page();
    }
}
