using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Repositories;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Services;

/// <summary>
/// Service implementation for doctor operations.
/// </summary>
public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IBillRepository _billRepository;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(
        IDoctorRepository doctorRepository,
        IAppointmentRepository appointmentRepository,
        IBillRepository billRepository,
        ILogger<DoctorService> logger)
    {
        _doctorRepository = doctorRepository;
        _appointmentRepository = appointmentRepository;
        _billRepository = billRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets doctor information by ID.
    /// </summary>
    public async Task<Doctor?> GetDoctorInfoAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving doctor info for ID: {Id}", doctorId);
            return await _doctorRepository.GetByIdAsync(doctorId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor info for ID: {Id}", doctorId);
            return null;
        }
    }

    /// <summary>
    /// Gets pending appointments for a doctor.
    /// </summary>
    public async Task<IEnumerable<Appointment>> GetPendingAppointmentsAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving pending appointments for doctor ID: {Id}", doctorId);
            return await _appointmentRepository.GetPendingByDoctorAsync(doctorId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending appointments for doctor ID: {Id}", doctorId);
            return Enumerable.Empty<Appointment>();
        }
    }

    /// <summary>
    /// Gets today's appointments for a doctor.
    /// </summary>
    public async Task<IEnumerable<Appointment>> GetTodaysAppointmentsAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving today's appointments for doctor ID: {Id}", doctorId);
            return await _appointmentRepository.GetTodaysByDoctorAsync(doctorId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving today's appointments for doctor ID: {Id}", doctorId);
            return Enumerable.Empty<Appointment>();
        }
    }

    /// <summary>
    /// Gets patient history for a doctor.
    /// </summary>
    public async Task<IEnumerable<Appointment>> GetPatientHistoryAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving patient history for doctor ID: {Id}", doctorId);
            return await _appointmentRepository.GetHistoryByDoctorAsync(doctorId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient history for doctor ID: {Id}", doctorId);
            return Enumerable.Empty<Appointment>();
        }
    }

    /// <summary>
    /// Approves an appointment.
    /// </summary>
    public async Task<bool> ApproveAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Approving appointment ID: {Id}", appointmentId);
            await _appointmentRepository.ApproveAsync(appointmentId, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving appointment ID: {Id}", appointmentId);
            return false;
        }
    }

    /// <summary>
    /// Deletes an appointment.
    /// </summary>
    public async Task<bool> DeleteAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting appointment ID: {Id}", appointmentId);
            await _appointmentRepository.DeleteAsync(appointmentId, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting appointment ID: {Id}", appointmentId);
            return false;
        }
    }

    /// <summary>
    /// Updates prescription for an appointment.
    /// </summary>
    public async Task<bool> UpdatePrescriptionAsync(int doctorId, int appointmentId, string disease, string progress, string prescription, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating prescription for appointment ID: {Id}", appointmentId);
            await _appointmentRepository.UpdatePrescriptionAsync(appointmentId, disease, progress, prescription, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prescription for appointment ID: {Id}", appointmentId);
            return false;
        }
    }

    /// <summary>
    /// Gets pending bills for a doctor.
    /// </summary>
    public async Task<IEnumerable<Bill>> GetPendingBillsAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving pending bills for doctor ID: {Id}", doctorId);
            return await _billRepository.GetPendingByDoctorAsync(doctorId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending bills for doctor ID: {Id}", doctorId);
            return Enumerable.Empty<Bill>();
        }
    }

    /// <summary>
    /// Marks a bill as paid.
    /// </summary>
    public async Task<bool> MarkBillPaidAsync(int doctorId, int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Marking bill paid for appointment ID: {Id}", appointmentId);
            await _appointmentRepository.MarkPaidAsync(doctorId, appointmentId, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking bill paid for appointment ID: {Id}", appointmentId);
            return false;
        }
    }

    /// <summary>
    /// Marks a bill as unpaid.
    /// </summary>
    public async Task<bool> MarkBillUnpaidAsync(int doctorId, int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Marking bill unpaid for appointment ID: {Id}", appointmentId);
            await _appointmentRepository.MarkUnpaidAsync(doctorId, appointmentId, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking bill unpaid for appointment ID: {Id}", appointmentId);
            return false;
        }
    }
}
