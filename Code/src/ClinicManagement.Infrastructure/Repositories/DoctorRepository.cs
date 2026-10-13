using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Repositories;
using ClinicManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<DoctorRepository> _logger;

    public DoctorRepository(ClinicDbContext context, ILogger<DoctorRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    private static void AttachLogin(Doctor doctor, LoginAccount login)
    {
        doctor.Email = login.Email;
        doctor.Password = login.Password;
    }

    private static IQueryable<Doctor> ActiveDoctors(IQueryable<Doctor> query) =>
        query.Where(d => d.Status != 0);

    public async Task<Doctor?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var row = await (
                from d in ActiveDoctors(_context.Doctors.AsNoTracking()).Include(d => d.Department)
                join l in _context.LoginAccounts.AsNoTracking() on d.DoctorId equals l.LoginId
                where d.DoctorId == id
                select new { d, l }).FirstOrDefaultAsync(cancellationToken);

            if (row == null) return null;
            AttachLogin(row.d, row.l);
            return row.d;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor by ID: {Id}", id);
            return null;
        }
    }

    public async Task<Doctor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var row = await (
                from d in ActiveDoctors(_context.Doctors.AsNoTracking())
                join l in _context.LoginAccounts.AsNoTracking() on d.DoctorId equals l.LoginId
                where l.Email == email
                select new { d, l }).FirstOrDefaultAsync(cancellationToken);

            if (row == null) return null;
            AttachLogin(row.d, row.l);
            return row.d;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor by email: {Email}", email);
            return null;
        }
    }

    public async Task<IEnumerable<Doctor>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rows = await (
                from d in ActiveDoctors(_context.Doctors.AsNoTracking()).Include(d => d.Department)
                join l in _context.LoginAccounts.AsNoTracking() on d.DoctorId equals l.LoginId
                select new { d, l }).ToListAsync(cancellationToken);

            foreach (var row in rows)
                AttachLogin(row.d, row.l);
            return rows.Select(r => r.d);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all active doctors");
            return Enumerable.Empty<Doctor>();
        }
    }

    public async Task<IEnumerable<Doctor>> SearchAsync(string searchQuery, CancellationToken cancellationToken = default)
    {
        try
        {
            var rows = await (
                from d in ActiveDoctors(_context.Doctors.AsNoTracking()).Include(d => d.Department)
                join l in _context.LoginAccounts.AsNoTracking() on d.DoctorId equals l.LoginId
                where d.Name.Contains(searchQuery)
                select new { d, l }).ToListAsync(cancellationToken);

            foreach (var row in rows)
                AttachLogin(row.d, row.l);
            return rows.Select(r => r.d);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching doctors with query: {Query}", searchQuery);
            return Enumerable.Empty<Doctor>();
        }
    }

    public async Task<IEnumerable<Doctor>> GetByDepartmentAsync(string deptName, CancellationToken cancellationToken = default)
    {
        try
        {
            var rows = await (
                from d in ActiveDoctors(_context.Doctors.AsNoTracking()).Include(d => d.Department)
                join l in _context.LoginAccounts.AsNoTracking() on d.DoctorId equals l.LoginId
                where d.Department != null && d.Department.DeptName == deptName
                select new { d, l }).ToListAsync(cancellationToken);

            foreach (var row in rows)
                AttachLogin(row.d, row.l);
            return rows.Select(r => r.d);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctors for department: {Dept}", deptName);
            return Enumerable.Empty<Doctor>();
        }
    }

    public async Task AddAsync(Doctor doctor, CancellationToken cancellationToken = default)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var login = new LoginAccount
            {
                Email = doctor.Email,
                Password = doctor.Password,
                Type = 2
            };
            _context.LoginAccounts.Add(login);
            await _context.SaveChangesAsync(cancellationToken);

            doctor.DoctorId = login.LoginId;
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error adding doctor: {Name}", doctor.Name);
            throw;
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
            _logger.LogError(ex, "Error checking doctor email existence: {Email}", email);
            return false;
        }
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var doctor = await _context.Doctors.FindAsync(new object[] { id }, cancellationToken);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting doctor ID: {Id}", id);
            throw;
        }
    }
}
