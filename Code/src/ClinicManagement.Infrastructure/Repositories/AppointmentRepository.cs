using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Repositories;
using ClinicManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private const int StatusApproved = 1;
    private const int StatusPending = 2;
    private const int StatusCompleted = 3;
    private const int StatusRejected = 4;

    private readonly ClinicDbContext _context;
    private readonly ILogger<AppointmentRepository> _logger;

    public AppointmentRepository(ClinicDbContext context, ILogger<AppointmentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Appointment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments.AsNoTracking()
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentId == id, cancellationToken);
            AppointmentFormatting.ApplyTimings(appointment);
            return appointment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointment by ID: {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<Appointment>> GetPendingByDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.Today;
            var list = await _context.Appointments.AsNoTracking()
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId
                            && a.AppointmentStatus == StatusPending
                            && a.AppointmentDate.Date == today)
                .ToListAsync(cancellationToken);
            AppointmentFormatting.ApplyTimings(list);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending appointments for doctor ID: {Id}", doctorId);
            return Enumerable.Empty<Appointment>();
        }
    }

    public async Task<IEnumerable<Appointment>> GetTodaysByDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.Today;
            var list = await _context.Appointments.AsNoTracking()
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId
                            && a.AppointmentStatus == StatusApproved
                            && a.AppointmentDate.Date == today)
                .ToListAsync(cancellationToken);
            AppointmentFormatting.ApplyTimings(list);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving today's appointments for doctor ID: {Id}", doctorId);
            return Enumerable.Empty<Appointment>();
        }
    }

    public async Task<Appointment?> GetCurrentByPatientAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments.AsNoTracking()
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.PatientId == patientId
                    && (a.AppointmentStatus == StatusApproved || a.AppointmentStatus == StatusPending), cancellationToken);
            AppointmentFormatting.ApplyTimings(appointment);
            return appointment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current appointment for patient ID: {Id}", patientId);
            return null;
        }
    }

    public async Task<IEnumerable<Appointment>> GetHistoryByDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _context.Appointments.AsNoTracking()
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId && a.AppointmentStatus == StatusCompleted)
                .ToListAsync(cancellationToken);
            AppointmentFormatting.ApplyTimings(list);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving history for doctor ID: {Id}", doctorId);
            return Enumerable.Empty<Appointment>();
        }
    }

    public async Task<IEnumerable<Appointment>> GetHistoryByPatientAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _context.Appointments.AsNoTracking()
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId && a.AppointmentStatus == StatusCompleted)
                .ToListAsync(cancellationToken);
            AppointmentFormatting.ApplyTimings(list);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving treatment history for patient ID: {Id}", patientId);
            return Enumerable.Empty<Appointment>();
        }
    }

    public async Task<IEnumerable<Appointment>> GetFreeSlotsByDoctorAsync(int doctorId, int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.Today;
            var bookedHours = await _context.Appointments.AsNoTracking()
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == today)
                .Select(a => a.AppointmentDate.Hour)
                .ToListAsync(cancellationToken);

            var freeSlots = Enumerable.Range(9, 9)
                .Where(h => !bookedHours.Contains(h))
                .Select(h => new Appointment
                {
                    AppointmentId = h,
                    DoctorId = doctorId,
                    Timings = AppointmentFormatting.FormatFreeSlotLabel(h)
                })
                .ToList();

            return freeSlots;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving free slots for doctor ID: {Id}", doctorId);
            return Enumerable.Empty<Appointment>();
        }
    }

    public async Task<Appointment?> GetPendingFeedbackByPatientAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments.AsNoTracking()
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.PatientId == patientId
                    && a.AppointmentStatus == StatusCompleted
                    && a.FeedbackStatus == 2, cancellationToken);
            AppointmentFormatting.ApplyTimings(appointment);
            return appointment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending feedback for patient ID: {Id}", patientId);
            return null;
        }
    }

    public async Task<Appointment?> GetNotificationByPatientAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.PatientId == patientId && a.PatientNotification == 2, cancellationToken);

            if (appointment != null)
            {
                appointment.PatientNotification = 1;
                await _context.SaveChangesAsync(cancellationToken);
            }

            AppointmentFormatting.ApplyTimings(appointment);
            return appointment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification for patient ID: {Id}", patientId);
            return null;
        }
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding appointment");
            throw;
        }
    }

    public async Task BookFromFreeSlotAsync(int doctorId, int patientId, int freeSlotHour, CancellationToken cancellationToken = default)
    {
        var slot = freeSlotHour;
        if (slot < 9)
            slot += 12;

        var appointmentDate = DateTime.Today.AddHours(slot);

        var appointment = new Appointment
        {
            DoctorId = doctorId,
            PatientId = patientId,
            AppointmentDate = appointmentDate,
            AppointmentStatus = StatusPending,
            DoctorNotification = 2,
            PatientNotification = 1
        };

        await AddAsync(appointment, cancellationToken);
    }

    public async Task ApproveAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments.FindAsync(new object[] { appointmentId }, cancellationToken);
            if (appointment != null)
            {
                appointment.AppointmentStatus = StatusApproved;
                appointment.PatientNotification = 2;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving appointment ID: {Id}", appointmentId);
            throw;
        }
    }

    public async Task DeleteAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments.FindAsync(new object[] { appointmentId }, cancellationToken);
            if (appointment != null)
            {
                appointment.AppointmentStatus = StatusRejected;
                appointment.PatientNotification = 2;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting appointment ID: {Id}", appointmentId);
            throw;
        }
    }

    public async Task UpdatePrescriptionAsync(int appointmentId, string disease, string progress, string prescription, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments.FindAsync(new object[] { appointmentId }, cancellationToken);
            if (appointment != null)
            {
                appointment.Disease = disease;
                appointment.Progress = progress;
                appointment.Prescription = prescription;
                appointment.AppointmentStatus = StatusCompleted;
                appointment.PatientNotification = 2;
                appointment.FeedbackStatus = 2;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prescription for appointment ID: {Id}", appointmentId);
            throw;
        }
    }

    public async Task MarkPaidAsync(int doctorId, int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments.FindAsync(new object[] { appointmentId }, cancellationToken);
            var doctor = await _context.Doctors.FindAsync(new object[] { doctorId }, cancellationToken);
            if (appointment != null && doctor != null)
            {
                appointment.BillStatus = "Paid";
                appointment.AppointmentStatus = StatusCompleted;
                appointment.PatientNotification = 2;
                appointment.BillAmount = doctor.ChargesPerVisit;
                appointment.FeedbackStatus = 2;
                doctor.PatientsTreated += 1;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking appointment paid ID: {Id}", appointmentId);
            throw;
        }
    }

    public async Task MarkUnpaidAsync(int doctorId, int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments.FindAsync(new object[] { appointmentId }, cancellationToken);
            var doctor = await _context.Doctors.FindAsync(new object[] { doctorId }, cancellationToken);
            if (appointment != null && doctor != null)
            {
                appointment.BillStatus = "Unpaid";
                appointment.AppointmentStatus = StatusCompleted;
                appointment.PatientNotification = 2;
                appointment.BillAmount = doctor.ChargesPerVisit;
                appointment.FeedbackStatus = 2;
                doctor.PatientsTreated += 1;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking appointment unpaid ID: {Id}", appointmentId);
            throw;
        }
    }

    public async Task StoreFeedbackAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments.FindAsync(new object[] { appointmentId }, cancellationToken);
            if (appointment == null)
                return;

            var doctor = await _context.Doctors.FindAsync(new object[] { appointment.DoctorId }, cancellationToken);
            if (doctor != null && doctor.PatientsTreated > 0)
            {
                var avg = doctor.ReputeIndex * (doctor.PatientsTreated - 1) / doctor.PatientsTreated;
                doctor.ReputeIndex = avg;
            }

            appointment.FeedbackStatus = 1;
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing feedback for appointment ID: {Id}", appointmentId);
            throw;
        }
    }
}
