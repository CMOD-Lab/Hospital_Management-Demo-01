using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Admin;

/// <summary>
/// Page model for doctor registration.
/// </summary>
public class DoctorRegistrationFormModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly ILogger<DoctorRegistrationFormModel> _logger;

    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public IEnumerable<Department> Departments { get; set; } = new List<Department>();

    public DoctorRegistrationFormModel(IAdminService adminService, ILogger<DoctorRegistrationFormModel> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 3)
            return RedirectToPage("/SignUp");

        Departments = await _adminService.GetDepartmentsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        string Name, string Email, string Password, string BirthDate,
        int Department, string Phone, string Address, string Gender,
        int Experience, int Salary, int ChargesPerVisit,
        string Specialization, string Qualification)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 3)
            return RedirectToPage("/SignUp");

        Departments = await _adminService.GetDepartmentsAsync();

        // Validate department selection
        if (Department == 0)
        {
            Message = "Please select a department.";
            IsSuccess = false;
            return Page();
        }

        // Check if email already exists
        if (await _adminService.DoctorEmailExistsAsync(Email))
        {
            Message = "This email already exists. Please choose a different one!";
            IsSuccess = false;
            return Page();
        }

        var doctor = new ClinicManagement.Domain.Entities.Doctor
        {
            Name = Name,
            Email = Email,
            Password = Password,
            BirthDate = DateTime.TryParse(BirthDate, out var bd) ? bd : DateTime.Now,
            DeptNo = Department,
            Phone = Phone,
            Address = Address,
            Gender = Gender,
            Experience = Experience,
            Salary = Salary,
            ChargesPerVisit = ChargesPerVisit,
            Specialization = Specialization,
            Qualification = Qualification,
            Status = 1
        };

        var success = await _adminService.AddDoctorAsync(doctor);

        if (success)
        {
            Message = "Doctor added successfully!";
            IsSuccess = true;
        }
        else
        {
            Message = "There was an error adding the doctor. Please try again.";
            IsSuccess = false;
        }

        return Page();
    }
}
