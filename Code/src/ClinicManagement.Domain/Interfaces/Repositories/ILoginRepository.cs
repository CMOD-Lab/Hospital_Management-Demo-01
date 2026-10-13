using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Interfaces.Repositories;

public interface ILoginRepository
{
    Task<LoginAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<LoginAccount> AddAsync(LoginAccount account, CancellationToken cancellationToken = default);
}
