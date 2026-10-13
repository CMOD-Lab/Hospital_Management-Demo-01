using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Admin;

/// <summary>
/// Page model for the admin dashboard.
/// </summary>
public class AdminHomeModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminHomeModel> _logger;

    public int TotalDoctors { get; set; }
    public int TotalPatients { get; set; }
    public decimal TotalIncome { get; set; }
    public IEnumerable<DepartmentStatViewModel> DepartmentStats { get; set; } = new List<DepartmentStatViewModel>();
    public IEnumerable<AppointmentStatViewModel> AppointmentStats { get; set; } = new List<AppointmentStatViewModel>();

    public AdminHomeModel(IAdminService adminService, ILogger<AdminHomeModel> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Verify admin session
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 3)
        {
            return RedirectToPage("/SignUp");
        }

        try
        {
            var dashboardData = await _adminService.GetDashboardDataAsync();
            TotalDoctors = dashboardData.TotalDoctors;
            TotalPatients = dashboardData.TotalPatients;
            TotalIncome = dashboardData.TotalIncome;

            DepartmentStats = dashboardData.DepartmentStats.Select(d => new DepartmentStatViewModel
            {
                DeptName = d.DeptName,
                DoctorCount = d.DoctorCount
            }).ToList();

            AppointmentStats = dashboardData.AppointmentStats.Select(a => new AppointmentStatViewModel
            {
                AppointmentId = a.AppointmentId,
                PatientName = a.PatientName,
                DoctorName = a.DoctorName,
                Timings = a.Timings,
                IsApproved = a.IsApproved
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admin dashboard");
        }

        return Page();
    }
}

public class DepartmentStatViewModel
{
    public string DeptName { get; set; } = string.Empty;
    public int DoctorCount { get; set; }
}

public class AppointmentStatViewModel
{
    public int AppointmentId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Timings { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
}
