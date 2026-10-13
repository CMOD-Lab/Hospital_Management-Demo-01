using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Admin;

/// <summary>
/// Page model for adding staff members.
/// </summary>
public class AddStaffModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AddStaffModel> _logger;

    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }

    public AddStaffModel(IAdminService adminService, ILogger<AddStaffModel> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 3)
            return RedirectToPage("/SignUp");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        string Name, string BirthDate, string Phone, string Gender,
        string Address, int Salary, string Designation, string Qualification)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userId == null || userType != 3)
            return RedirectToPage("/SignUp");

        var staff = new OtherStaff
        {
            Name = Name,
            BirthDate = DateTime.TryParse(BirthDate, out var bd) ? bd : DateTime.Now,
            Phone = Phone,
            Gender = Gender,
            Address = Address,
            Salary = Salary,
            Designation = Designation,
            Qualification = Qualification,
            IsActive = true
        };

        var success = await _adminService.AddStaffAsync(staff);

        if (success)
        {
            Message = "Staff member added successfully!";
            IsSuccess = true;
        }
        else
        {
            Message = "There was an error adding the staff member. Please try again.";
            IsSuccess = false;
        }

        return Page();
    }
}
