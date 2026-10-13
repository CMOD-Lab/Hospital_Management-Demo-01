namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Represents a department in the clinic (dbo.department).
/// </summary>
public class Department
{
    public int DeptNo { get; set; }
    public string DeptName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
