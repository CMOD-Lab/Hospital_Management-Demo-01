using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Domain.Interfaces.Repositories;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly ILoginRepository _loginRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository,
        ILoginRepository loginRepository,
        ILogger<AuthService> logger)
    {
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
        _loginRepository = loginRepository;
        _logger = logger;
    }

    public async Task<(bool Success, UserType UserType, int UserId, string Message)> LoginAsync(
        string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", email);

            var login = await _loginRepository.GetByEmailAsync(email, cancellationToken);
            if (login == null)
                return (false, UserType.Patient, 0, "Email not found. Try Again!");

            if (login.Password != password)
                return (false, UserType.Patient, 0, "Incorrect Password. Try Again!");

            var userType = (UserType)login.Type;
            _logger.LogInformation("Login successful for {Type} ID: {Id}", userType, login.LoginId);
            return (true, userType, login.LoginId, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", email);
            return (false, UserType.Patient, 0, "There was some error. Try Again!");
        }
    }

    public async Task<(bool Success, int PatientId, string Message)> RegisterPatientAsync(
        string name, string birthDate, string email, string password,
        string phone, string gender, string address, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Patient registration attempt for email: {Email}", email);

            if (await _patientRepository.EmailExistsAsync(email, cancellationToken))
                return (false, 0, "Email already exists. Please choose a different one.");

            var patient = new Patient
            {
                Name = name,
                Email = email,
                Password = password,
                Phone = phone,
                Gender = gender,
                Address = address,
                BirthDate = DateTime.TryParse(birthDate, out var bd) ? bd : DateTime.Now
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
