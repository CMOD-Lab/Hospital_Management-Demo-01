using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Repositories;
using ClinicManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Repositories;

public class LoginRepository : ILoginRepository
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<LoginRepository> _logger;

    public LoginRepository(ClinicDbContext context, ILogger<LoginRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<LoginAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.LoginAccounts.AsNoTracking()
                .FirstOrDefaultAsync(l => l.Email == email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving login by email: {Email}", email);
            return null;
        }
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.LoginAccounts.AnyAsync(l => l.Email == email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking login email: {Email}", email);
            return false;
        }
    }

    public async Task<LoginAccount> AddAsync(LoginAccount account, CancellationToken cancellationToken = default)
    {
        _context.LoginAccounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        return account;
    }
}
