using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Appointment data access operations.
/// </summary>
public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetPendingByDoctorAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetTodaysByDoctorAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<Appointment?> GetCurrentByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetHistoryByDoctorAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetFreeSlotsByDoctorAsync(int doctorId, int patientId, CancellationToken cancellationToken = default);
    Task<Appointment?> GetPendingFeedbackByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<Appointment?> GetNotificationByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task ApproveAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task UpdatePrescriptionAsync(int appointmentId, string disease, string progress, string prescription, CancellationToken cancellationToken = default);
    Task MarkPaidAsync(int doctorId, int appointmentId, CancellationToken cancellationToken = default);
    Task MarkUnpaidAsync(int doctorId, int appointmentId, CancellationToken cancellationToken = default);
    Task StoreFeedbackAsync(int appointmentId, CancellationToken cancellationToken = default);
}
