using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Repositories;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Services;

/// <summary>
/// Service implementation for admin operations.
/// </summary>
public class AdminService : IAdminService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IDoctorRepository doctorRepository,
        IPatientRepository patientRepository,
        IStaffRepository staffRepository,
        IDepartmentRepository departmentRepository,
        IAppointmentRepository appointmentRepository,
        ILogger<AdminService> logger)
    {
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
        _staffRepository = staffRepository;
        _departmentRepository = departmentRepository;
        _appointmentRepository = appointmentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets dashboard statistics for the admin home page.
    /// </summary>
    public async Task<AdminDashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving admin dashboard data");

            var doctors = await _doctorRepository.GetAllActiveAsync(cancellationToken);
            var patients = await _patientRepository.GetAllAsync(cancellationToken);
            var departments = await _departmentRepository.GetAllAsync(cancellationToken);

            var deptStats = departments.Select(d => new DepartmentStat
            {
                DeptName = d.DeptName,
                DoctorCount = doctors.Count(doc => doc.DeptNo == d.DeptNo)
            }).ToList();

            return new AdminDashboardData
            {
                TotalDoctors = doctors.Count(),
                TotalPatients = patients.Count(),
                TotalIncome = 0,
                DepartmentStats = deptStats,
                AppointmentStats = new List<AppointmentStat>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin dashboard data");
            return new AdminDashboardData();
        }
    }

    /// <summary>
    /// Gets all doctors, optionally filtered by search query.
    /// </summary>
    public async Task<IEnumerable<Doctor>> GetDoctorsAsync(string searchQuery = "", CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return await _doctorRepository.GetAllActiveAsync(cancellationToken);
            return await _doctorRepository.SearchAsync(searchQuery, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctors with query: {Query}", searchQuery);
            return Enumerable.Empty<Doctor>();
        }
    }

    /// <summary>
    /// Gets all patients, optionally filtered by search query.
    /// </summary>
    public async Task<IEnumerable<Patient>> GetPatientsAsync(string searchQuery = "", CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return await _patientRepository.GetAllAsync(cancellationToken);
            return await _patientRepository.SearchAsync(searchQuery, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patients with query: {Query}", searchQuery);
            return Enumerable.Empty<Patient>();
        }
    }

    /// <summary>
    /// Gets all staff members, optionally filtered by search query.
    /// </summary>
    public async Task<IEnumerable<OtherStaff>> GetStaffAsync(string searchQuery = "", CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return await _staffRepository.GetAllActiveAsync(cancellationToken);
            return await _staffRepository.SearchAsync(searchQuery, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff with query: {Query}", searchQuery);
            return Enumerable.Empty<OtherStaff>();
        }
    }

    /// <summary>
    /// Adds a new doctor to the system.
    /// </summary>
    public async Task<bool> AddDoctorAsync(Doctor doctor, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Adding new doctor: {Name}", doctor.Name);
            await _doctorRepository.AddAsync(doctor, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding doctor: {Name}", doctor.Name);
            return false;
        }
    }

    /// <summary>
    /// Adds a new staff member to the system.
    /// </summary>
    public async Task<bool> AddStaffAsync(OtherStaff staff, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Adding new staff member: {Name}", staff.Name);
            await _staffRepository.AddAsync(staff, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding staff member: {Name}", staff.Name);
            return false;
        }
    }

    /// <summary>
    /// Deactivates a doctor (soft delete).
    /// </summary>
    public async Task<bool> DeleteDoctorAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deactivating doctor with ID: {Id}", id);
            await _doctorRepository.DeactivateAsync(id, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating doctor with ID: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// Deactivates a staff member (soft delete).
    /// </summary>
    public async Task<bool> DeleteStaffAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deactivating staff member with ID: {Id}", id);
            await _staffRepository.DeactivateAsync(id, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating staff member with ID: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// Gets a doctor's profile by ID.
    /// </summary>
    public async Task<Doctor?> GetDoctorProfileAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _doctorRepository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor profile for ID: {Id}", id);
            return null;
        }
    }

    /// <summary>
    /// Gets a patient's profile by ID.
    /// </summary>
    public async Task<Patient?> GetPatientProfileAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _patientRepository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient profile for ID: {Id}", id);
            return null;
        }
    }

    /// <summary>
    /// Gets a staff member's profile by ID.
    /// </summary>
    public async Task<OtherStaff?> GetStaffProfileAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _staffRepository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff profile for ID: {Id}", id);
            return null;
        }
    }

    /// <summary>
    /// Checks if a doctor email already exists.
    /// </summary>
    public async Task<bool> DoctorEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _doctorRepository.EmailExistsAsync(email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking doctor email: {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Gets all departments.
    /// </summary>
    public async Task<IEnumerable<Department>> GetDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _departmentRepository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departments");
            return Enumerable.Empty<Department>();
        }
    }
}
