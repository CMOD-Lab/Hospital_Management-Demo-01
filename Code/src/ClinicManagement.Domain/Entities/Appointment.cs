using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Represents an appointment between a patient and a doctor (dbo.appointment).
/// Status codes: 1 = approved, 2 = pending, 3 = completed, 4 = rejected/cancelled.
/// </summary>
public class Appointment
{
    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public int AppointmentStatus { get; set; }
    public double? BillAmount { get; set; }
    public string? BillStatus { get; set; }
    public int? DoctorNotification { get; set; }
    public int? PatientNotification { get; set; }
    public int? FeedbackStatus { get; set; }
    public string? Disease { get; set; }
    public string? Progress { get; set; }
    public string? Prescription { get; set; }

    [NotMapped]
    public string Timings { get; set; } = string.Empty;

    [NotMapped]
    public bool IsApproved
    {
        get => AppointmentStatus == 1;
        set => AppointmentStatus = value ? 1 : 2;
    }

    [NotMapped]
    public bool IsCompleted
    {
        get => AppointmentStatus == 3;
        set { if (value) AppointmentStatus = 3; }
    }

    [NotMapped]
    public bool IsPaid
    {
        get => BillStatus != null && BillStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase);
        set => BillStatus = value ? "Paid" : "Unpaid";
    }

    [NotMapped]
    public bool FeedbackGiven
    {
        get => FeedbackStatus == 1;
        set => FeedbackStatus = value ? 1 : 2;
    }

    public Doctor? Doctor { get; set; }
    public Patient? Patient { get; set; }
}
