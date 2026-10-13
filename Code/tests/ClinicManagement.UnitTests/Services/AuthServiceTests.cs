using ClinicManagement.Application.Services;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<ILoginRepository> _loginRepositoryMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _loginRepositoryMock = new Mock<ILoginRepository>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _authService = new AuthService(
            _patientRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _loginRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidPatientCredentials_ReturnsSuccess()
    {
        _loginRepositoryMock.Setup(r => r.GetByEmailAsync("patient@test.com", default))
            .ReturnsAsync(new LoginAccount
            {
                LoginId = 1,
                Email = "patient@test.com",
                Password = "password123",
                Type = 1
            });

        var result = await _authService.LoginAsync("patient@test.com", "password123");

        result.Success.Should().BeTrue();
        result.UserId.Should().Be(1);
        result.UserType.Should().Be(Domain.Enums.UserType.Patient);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsFailure()
    {
        _loginRepositoryMock.Setup(r => r.GetByEmailAsync("patient@test.com", default))
            .ReturnsAsync(new LoginAccount
            {
                LoginId = 1,
                Email = "patient@test.com",
                Password = "correctpassword",
                Type = 1
            });

        var result = await _authService.LoginAsync("patient@test.com", "wrongpassword");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Incorrect Password");
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ReturnsFailure()
    {
        _loginRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync((LoginAccount?)null);

        var result = await _authService.LoginAsync("nonexistent@test.com", "password");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Email not found");
    }

    [Fact]
    public async Task RegisterPatientAsync_WithNewEmail_ReturnsSuccess()
    {
        _patientRepositoryMock.Setup(r => r.EmailExistsAsync("new@test.com", default))
            .ReturnsAsync(false);

        var createdPatient = new Patient { PatientId = 5, Email = "new@test.com" };
        _patientRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Patient>(), default))
            .ReturnsAsync(createdPatient);

        var result = await _authService.RegisterPatientAsync(
            "New Patient", "1990-01-01", "new@test.com", "password",
            "1234567890", "M", "123 Main St");

        result.Success.Should().BeTrue();
        result.PatientId.Should().Be(5);
    }

    [Fact]
    public async Task RegisterPatientAsync_WithExistingEmail_ReturnsFailure()
    {
        _patientRepositoryMock.Setup(r => r.EmailExistsAsync("existing@test.com", default))
            .ReturnsAsync(true);

        var result = await _authService.RegisterPatientAsync(
            "Test Patient", "1990-01-01", "existing@test.com", "password",
            "1234567890", "M", "123 Main St");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Email already exists");
    }
}
