namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Represents a department in the clinic.
/// </summary>
public class Department
{
    public int DeptNo { get; set; }
    public string DeptName { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
