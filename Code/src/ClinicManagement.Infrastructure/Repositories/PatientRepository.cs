using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Repositories;
using ClinicManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<PatientRepository> _logger;

    public PatientRepository(ClinicDbContext context, ILogger<PatientRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    private static void AttachLogin(Patient patient, LoginAccount login)
    {
        patient.Email = login.Email;
        patient.Password = login.Password;
    }

    public async Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var row = await (
                from p in _context.Patients.AsNoTracking()
                join l in _context.LoginAccounts.AsNoTracking() on p.PatientId equals l.LoginId
                where p.PatientId == id
                select new { p, l }).FirstOrDefaultAsync(cancellationToken);

            if (row == null) return null;
            AttachLogin(row.p, row.l);
            return row.p;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient by ID: {Id}", id);
            return null;
        }
    }

    public async Task<Patient?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var row = await (
                from p in _context.Patients.AsNoTracking()
                join l in _context.LoginAccounts.AsNoTracking() on p.PatientId equals l.LoginId
                where l.Email == email
                select new { p, l }).FirstOrDefaultAsync(cancellationToken);

            if (row == null) return null;
            AttachLogin(row.p, row.l);
            return row.p;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient by email: {Email}", email);
            return null;
        }
    }

    public async Task<IEnumerable<Patient>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rows = await (
                from p in _context.Patients.AsNoTracking()
                join l in _context.LoginAccounts.AsNoTracking() on p.PatientId equals l.LoginId
                select new { p, l }).ToListAsync(cancellationToken);

            foreach (var row in rows)
                AttachLogin(row.p, row.l);
            return rows.Select(r => r.p);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all patients");
            return Enumerable.Empty<Patient>();
        }
    }

    public async Task<IEnumerable<Patient>> SearchAsync(string searchQuery, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Patients.AsNoTracking()
                .Where(p => p.Name.Contains(searchQuery))
                .Select(p => new Patient { PatientId = p.PatientId, Name = p.Name, Phone = p.Phone })
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching patients with query: {Query}", searchQuery);
            return Enumerable.Empty<Patient>();
        }
    }

    public async Task<Patient> AddAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var login = new LoginAccount
            {
                Email = patient.Email,
                Password = patient.Password,
                Type = 1
            };
            _context.LoginAccounts.Add(login);
            await _context.SaveChangesAsync(cancellationToken);

            patient.PatientId = login.LoginId;
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return patient;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error adding patient: {Name}", patient.Name);
            throw;
        }
    }

    public async Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient ID: {Id}", patient.PatientId);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Patients.AnyAsync(p => p.PatientId == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking patient existence for ID: {Id}", id);
            return false;
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
            _logger.LogError(ex, "Error checking patient email existence: {Email}", email);
            return false;
        }
    }
}
