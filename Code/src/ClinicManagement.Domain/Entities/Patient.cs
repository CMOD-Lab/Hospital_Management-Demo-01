using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Represents a patient (dbo.patient; credentials in dbo.logintable).
/// </summary>
public class Patient
{
    public int PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; } = string.Empty;

    [NotMapped]
    public string Email { get; set; } = string.Empty;

    [NotMapped]
    public string Password { get; set; } = string.Empty;

    [NotMapped]
    public bool IsActive { get; set; } = true;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
