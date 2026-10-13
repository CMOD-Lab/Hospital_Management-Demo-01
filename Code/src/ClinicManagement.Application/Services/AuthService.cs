using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Domain.Interfaces.Repositories;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Services;

/// <summary>
/// Service implementation for authentication operations.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository,
        ILogger<AuthService> logger)
    {
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
        _logger = logger;
    }

    /// <summary>
    /// Validates user credentials and returns user type and ID.
    /// </summary>
    public async Task<(bool Success, UserType UserType, int UserId, string Message)> LoginAsync(
        string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", email);

            // Check patient
            var patient = await _patientRepository.GetByEmailAsync(email, cancellationToken);
            if (patient != null)
            {
                if (patient.Password == password)
                {
                    _logger.LogInformation("Patient login successful for ID: {Id}", patient.PatientId);
                    return (true, UserType.Patient, patient.PatientId, "Login successful");
                }
                return (false, UserType.Patient, 0, "Incorrect Password. Try Again!");
            }

            // Check doctor
            var doctor = await _doctorRepository.GetByEmailAsync(email, cancellationToken);
            if (doctor != null)
            {
                if (doctor.Password == password)
                {
                    _logger.LogInformation("Doctor login successful for ID: {Id}", doctor.DoctorId);
                    return (true, UserType.Doctor, doctor.DoctorId, "Login successful");
                }
                return (false, UserType.Doctor, 0, "Incorrect Password. Try Again!");
            }

            // Check admin (hardcoded admin or from a separate admin table)
            if (email == "admin@clinic.com" && password == "admin123")
            {
                _logger.LogInformation("Admin login successful");
                return (true, UserType.Admin, 1, "Login successful");
            }

            return (false, UserType.Patient, 0, "Email not found. Try Again!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", email);
            return (false, UserType.Patient, 0, "There was some error. Try Again!");
        }
    }

    /// <summary>
    /// Registers a new patient in the system.
    /// </summary>
    public async Task<(bool Success, int PatientId, string Message)> RegisterPatientAsync(
        string name, string birthDate, string email, string password,
        string phone, string gender, string address, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Patient registration attempt for email: {Email}", email);

            // Check if email already exists
            if (await _patientRepository.EmailExistsAsync(email, cancellationToken))
            {
                return (false, 0, "Email already exists. Please choose a different one.");
            }

            var patient = new Patient
            {
                Name = name,
                Email = email,
                Password = password,
                Phone = phone,
                Gender = gender,
                Address = address,
                BirthDate = DateTime.TryParse(birthDate, out var bd) ? bd : DateTime.Now,
                IsActive = true
            };

            var created = await _patientRepository.AddAsync(patient, cancellationToken);
            _logger.LogInformation("Patient registered successfully with ID: {Id}", created.PatientId);
            return (true, created.PatientId, "Registration successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during patient registration for email: {Email}", email);
            return (false, 0, "There was some error. Try again!");
        }
    }
}
