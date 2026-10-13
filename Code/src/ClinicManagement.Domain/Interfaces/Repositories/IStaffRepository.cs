using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for OtherStaff data access operations.
/// </summary>
public interface IStaffRepository
{
    Task<OtherStaff?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OtherStaff>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<OtherStaff>> SearchAsync(string searchQuery, CancellationToken cancellationToken = default);
    Task AddAsync(OtherStaff staff, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int id, CancellationToken cancellationToken = default);
}
