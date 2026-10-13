using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Interfaces.Services;

/// <summary>
/// Service interface for admin operations.
/// </summary>
public interface IAdminService
{
    Task<AdminDashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Doctor>> GetDoctorsAsync(string searchQuery = "", CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> GetPatientsAsync(string searchQuery = "", CancellationToken cancellationToken = default);
    Task<IEnumerable<OtherStaff>> GetStaffAsync(string searchQuery = "", CancellationToken cancellationToken = default);
    Task<bool> AddDoctorAsync(Doctor doctor, CancellationToken cancellationToken = default);
    Task<bool> AddStaffAsync(OtherStaff staff, CancellationToken cancellationToken = default);
    Task<bool> DeleteDoctorAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteStaffAsync(int id, CancellationToken cancellationToken = default);
    Task<Doctor?> GetDoctorProfileAsync(int id, CancellationToken cancellationToken = default);
    Task<Patient?> GetPatientProfileAsync(int id, CancellationToken cancellationToken = default);
    Task<OtherStaff?> GetStaffProfileAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DoctorEmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Department>> GetDepartmentsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Data transfer object for admin dashboard statistics.
/// </summary>
public class AdminDashboardData
{
    public int TotalDoctors { get; set; }
    public int TotalPatients { get; set; }
    public decimal TotalIncome { get; set; }
    public IEnumerable<DepartmentStat> DepartmentStats { get; set; } = new List<DepartmentStat>();
    public IEnumerable<AppointmentStat> AppointmentStats { get; set; } = new List<AppointmentStat>();
}

/// <summary>
/// Department statistics for the dashboard.
/// </summary>
public class DepartmentStat
{
    public string DeptName { get; set; } = string.Empty;
    public int DoctorCount { get; set; }
}

/// <summary>
/// Appointment statistics for the dashboard.
/// </summary>
public class AppointmentStat
{
    public int AppointmentId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Timings { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
}
