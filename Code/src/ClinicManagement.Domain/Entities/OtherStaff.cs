using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Non-doctor staff (dbo.otherstaff).
/// </summary>
public class OtherStaff
{
    public int StaffId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double? Salary { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string? Qualification { get; set; }

    [NotMapped]
    public bool IsActive { get; set; } = true;
}
