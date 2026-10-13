using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Repositories;
using ClinicManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Repositories;

public class BillRepository : IBillRepository
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<BillRepository> _logger;

    public BillRepository(ClinicDbContext context, ILogger<BillRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Bill>> GetByPatientAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Appointments.AsNoTracking()
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId && a.AppointmentStatus == 3)
                .Select(a => new Bill
                {
                    BillId = a.AppointmentId,
                    AppointmentId = a.AppointmentId,
                    PatientId = a.PatientId,
                    Amount = (decimal)(a.BillAmount ?? 0),
                    BillDate = a.AppointmentDate,
                    IsPaid = a.BillStatus != null && a.BillStatus.ToLower() == "paid",
                    Appointment = a
                })
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bills for patient ID: {Id}", patientId);
            return Enumerable.Empty<Bill>();
        }
    }

    public async Task<IEnumerable<Bill>> GetPendingByDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Appointments.AsNoTracking()
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId
                            && a.AppointmentStatus == 3
                            && (a.BillStatus == null || a.BillStatus.ToLower() != "paid"))
                .Select(a => new Bill
                {
                    BillId = a.AppointmentId,
                    AppointmentId = a.AppointmentId,
                    PatientId = a.PatientId,
                    Amount = (decimal)(a.BillAmount ?? 0),
                    BillDate = a.AppointmentDate,
                    IsPaid = false,
                    Appointment = a
                })
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending bills for doctor ID: {Id}", doctorId);
            return Enumerable.Empty<Bill>();
        }
    }
}

public class StaffRepository : IStaffRepository
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<StaffRepository> _logger;

    public StaffRepository(ClinicDbContext context, ILogger<StaffRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OtherStaff?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.OtherStaff.AsNoTracking()
                .FirstOrDefaultAsync(s => s.StaffId == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff by ID: {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<OtherStaff>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.OtherStaff.AsNoTracking().ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all active staff");
            return Enumerable.Empty<OtherStaff>();
        }
    }

    public async Task<IEnumerable<OtherStaff>> SearchAsync(string searchQuery, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.OtherStaff.AsNoTracking()
                .Where(s => s.Name.Contains(searchQuery))
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching staff with query: {Query}", searchQuery);
            return Enumerable.Empty<OtherStaff>();
        }
    }

    public async Task AddAsync(OtherStaff staff, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.OtherStaff.Add(staff);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding staff member: {Name}", staff.Name);
            throw;
        }
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var staff = await _context.OtherStaff.FindAsync(new object[] { id }, cancellationToken);
            if (staff != null)
            {
                _context.OtherStaff.Remove(staff);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting staff ID: {Id}", id);
            throw;
        }
    }
}

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<DepartmentRepository> _logger;

    public DepartmentRepository(ClinicDbContext context, ILogger<DepartmentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Department>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Departments.AsNoTracking().ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all departments");
            return Enumerable.Empty<Department>();
        }
    }

    public async Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Departments.AsNoTracking()
                .FirstOrDefaultAsync(d => d.DeptNo == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department by ID: {Id}", id);
            return null;
        }
    }

    public async Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Departments.AsNoTracking()
                .FirstOrDefaultAsync(d => d.DeptName == name, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department by name: {Name}", name);
            return null;
        }
    }
}
