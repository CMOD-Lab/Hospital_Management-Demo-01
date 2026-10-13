using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Bill data access operations.
/// </summary>
public interface IBillRepository
{
    Task<IEnumerable<Bill>> GetByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Bill>> GetPendingByDoctorAsync(int doctorId, CancellationToken cancellationToken = default);
}
