using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Represents a doctor in the clinic management system (dbo.doctor).
/// </summary>
public class Doctor
{
    public int DoctorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public int DeptNo { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Experience { get; set; }
    public double MonthlySalary { get; set; }
    public double ChargesPerVisit { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public double ReputeIndex { get; set; }
    public int PatientsTreated { get; set; }
    /// <summary>Non-zero = active; -1 = pending approval; 0 = inactive.</summary>
    public int Status { get; set; } = 1;

    [NotMapped]
    public string Email { get; set; } = string.Empty;

    [NotMapped]
    public string Password { get; set; } = string.Empty;

    [NotMapped]
    public int Salary
    {
        get => (int)MonthlySalary;
        set => MonthlySalary = value;
    }

    public Department? Department { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
