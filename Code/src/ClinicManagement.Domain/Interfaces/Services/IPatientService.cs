using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Interfaces.Services;

/// <summary>
/// Service interface for patient operations.
/// </summary>
public interface IPatientService
{
    Task<Patient?> GetPatientInfoAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Bill>> GetBillHistoryAsync(int patientId, CancellationToken cancellationToken = default);
    Task<Appointment?> GetCurrentAppointmentAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetTreatmentHistoryAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Department>> GetDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Doctor>> GetDoctorsByDepartmentAsync(string deptName, CancellationToken cancellationToken = default);
    Task<Doctor?> GetDoctorProfileAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetFreeSlotsAsync(int doctorId, int patientId, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> BookAppointmentAsync(int doctorId, int patientId, int freeSlotId, CancellationToken cancellationToken = default);
    Task<Appointment?> GetNotificationAsync(int patientId, CancellationToken cancellationToken = default);
    Task<Appointment?> GetPendingFeedbackAsync(int patientId, CancellationToken cancellationToken = default);
    Task<bool> SubmitFeedbackAsync(int appointmentId, CancellationToken cancellationToken = default);
}
