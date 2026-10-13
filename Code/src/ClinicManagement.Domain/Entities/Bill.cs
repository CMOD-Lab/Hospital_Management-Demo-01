namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Bill view model derived from completed appointments (no separate bill table).
/// </summary>
public class Bill
{
    public int BillId { get; set; }
    public int PatientId { get; set; }
    public int AppointmentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime BillDate { get; set; }
    public bool IsPaid { get; set; }

    public Patient? Patient { get; set; }
    public Appointment? Appointment { get; set; }
}
