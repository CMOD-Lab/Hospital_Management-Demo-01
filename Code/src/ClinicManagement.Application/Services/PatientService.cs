using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Repositories;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Services;

/// <summary>
/// Service implementation for patient operations.
/// </summary>
public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IBillRepository _billRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<PatientService> _logger;

    public PatientService(
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository,
        IAppointmentRepository appointmentRepository,
        IBillRepository billRepository,
        IDepartmentRepository departmentRepository,
        ILogger<PatientService> logger)
    {
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
        _appointmentRepository = appointmentRepository;
        _billRepository = billRepository;
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets patient information by ID.
    /// </summary>
    public async Task<Patient?> GetPatientInfoAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving patient info for ID: {Id}", patientId);
            return await _patientRepository.GetByIdAsync(patientId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient info for ID: {Id}", patientId);
            return null;
        }
    }

    /// <summary>
    /// Gets bill history for a patient.
    /// </summary>
    public async Task<IEnumerable<Bill>> GetBillHistoryAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving bill history for patient ID: {Id}", patientId);
            return await _billRepository.GetByPatientAsync(patientId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bill history for patient ID: {Id}", patientId);
            return Enumerable.Empty<Bill>();
        }
    }

    /// <summary>
    /// Gets the current appointment for a patient.
    /// </summary>
    public async Task<Appointment?> GetCurrentAppointmentAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving current appointment for patient ID: {Id}", patientId);
            return await _appointmentRepository.GetCurrentByPatientAsync(patientId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current appointment for patient ID: {Id}", patientId);
            return null;
        }
    }

    /// <summary>
    /// Gets treatment history for a patient.
    /// </summary>
    public async Task<IEnumerable<Appointment>> GetTreatmentHistoryAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving treatment history for patient ID: {Id}", patientId);
            // Return completed appointments as treatment history
            return await _appointmentRepository.GetHistoryByPatientAsync(patientId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving treatment history for patient ID: {Id}", patientId);
            return Enumerable.Empty<Appointment>();
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

    /// <summary>
    /// Gets doctors by department name.
    /// </summary>
    public async Task<IEnumerable<Doctor>> GetDoctorsByDepartmentAsync(string deptName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving doctors for department: {Dept}", deptName);
            return await _doctorRepository.GetByDepartmentAsync(deptName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctors for department: {Dept}", deptName);
            return Enumerable.Empty<Doctor>();
        }
    }

    /// <summary>
    /// Gets a doctor's profile by ID.
    /// </summary>
    public async Task<Doctor?> GetDoctorProfileAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _doctorRepository.GetByIdAsync(doctorId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor profile for ID: {Id}", doctorId);
            return null;
        }
    }

    /// <summary>
    /// Gets free appointment slots for a doctor.
    /// </summary>
    public async Task<IEnumerable<Appointment>> GetFreeSlotsAsync(int doctorId, int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving free slots for doctor ID: {DoctorId}, patient ID: {PatientId}", doctorId, patientId);
            return await _appointmentRepository.GetFreeSlotsByDoctorAsync(doctorId, patientId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving free slots for doctor ID: {DoctorId}", doctorId);
            return Enumerable.Empty<Appointment>();
        }
    }

    /// <summary>
    /// Books an appointment for a patient.
    /// </summary>
    public async Task<(bool Success, string Message)> BookAppointmentAsync(int doctorId, int patientId, int freeSlotId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Booking appointment for patient ID: {PatientId} with doctor ID: {DoctorId}", patientId, doctorId);

            await _appointmentRepository.BookFromFreeSlotAsync(doctorId, patientId, freeSlotId, cancellationToken);
            return (true, "Appointment request sent successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error booking appointment for patient ID: {PatientId}", patientId);
            return (false, "There was some error. Try Again!");
        }
    }

    /// <summary>
    /// Gets appointment notification for a patient.
    /// </summary>
    public async Task<Appointment?> GetNotificationAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _appointmentRepository.GetNotificationByPatientAsync(patientId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification for patient ID: {Id}", patientId);
            return null;
        }
    }

    /// <summary>
    /// Gets pending feedback for a patient.
    /// </summary>
    public async Task<Appointment?> GetPendingFeedbackAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _appointmentRepository.GetPendingFeedbackByPatientAsync(patientId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending feedback for patient ID: {Id}", patientId);
            return null;
        }
    }

    /// <summary>
    /// Submits feedback for an appointment.
    /// </summary>
    public async Task<bool> SubmitFeedbackAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Submitting feedback for appointment ID: {Id}", appointmentId);
            await _appointmentRepository.StoreFeedbackAsync(appointmentId, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting feedback for appointment ID: {Id}", appointmentId);
            return false;
        }
    }
}
