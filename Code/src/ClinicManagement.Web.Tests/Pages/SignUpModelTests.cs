using System;
using System.Threading.Tasks;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Domain.Interfaces.Services;
using ClinicManagement.Web.Pages;
using ClinicManagement.Web.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Web.Tests.Pages
{
    public class SignUpModelTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<ILogger<SignUpModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public SignUpModelTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockLogger = new Mock<ILogger<SignUpModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private SignUpModel CreatePageModel()
        {
            var model = new SignUpModel(_mockAuthService.Object, _mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new SignUpModel(_mockAuthService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesDefaultProperties()
        {
            var model = new SignUpModel(_mockAuthService.Object, _mockLogger.Object);
            Assert.Equal(string.Empty, model.LoginMessage);
            Assert.Equal(string.Empty, model.SignupMessage);
            Assert.False(model.SignupSuccess);
        }

        [Fact]
        public void OnGet_ClearsSession()
        {
            var model = CreatePageModel();
            model.OnGet();
            _mockSession.Verify(s => s.Clear(), Times.Once);
        }

        [Fact]
        public async Task OnPostLoginAsync_WhenEmailIsEmpty_SetsLoginMessage()
        {
            var model = CreatePageModel();
            var result = await model.OnPostLoginAsync("", "password");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Please enter both email and password.", model.LoginMessage);
        }

        [Fact]
        public async Task OnPostLoginAsync_WhenPasswordIsEmpty_SetsLoginMessage()
        {
            var model = CreatePageModel();
            var result = await model.OnPostLoginAsync("email@test.com", "");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Please enter both email and password.", model.LoginMessage);
        }

        [Fact]
        public async Task OnPostLoginAsync_WhenBothEmpty_SetsLoginMessage()
        {
            var model = CreatePageModel();
            var result = await model.OnPostLoginAsync("", "");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Please enter both email and password.", model.LoginMessage);
        }

        [Fact]
        public async Task OnPostLoginAsync_WhenLoginSucceedsAsPatient_RedirectsToPatientHome()
        {
            _mockAuthService.Setup(s => s.LoginAsync("patient@test.com", "pass", default))
                .ReturnsAsync((true, UserType.Patient, 1, "Success"));
            var model = CreatePageModel();
            var result = await model.OnPostLoginAsync("patient@test.com", "pass");
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/Patient/PatientHome", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostLoginAsync_WhenLoginSucceedsAsDoctor_RedirectsToDoctorHome()
        {
            _mockAuthService.Setup(s => s.LoginAsync("doctor@test.com", "pass", default))
                .ReturnsAsync((true, UserType.Doctor, 2, "Success"));
            var model = CreatePageModel();
            var result = await model.OnPostLoginAsync("doctor@test.com", "pass");
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/Doctor/DoctorHome", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostLoginAsync_WhenLoginSucceedsAsAdmin_RedirectsToAdminHome()
        {
            _mockAuthService.Setup(s => s.LoginAsync("admin@test.com", "pass", default))
                .ReturnsAsync((true, UserType.Admin, 3, "Success"));
            var model = CreatePageModel();
            var result = await model.OnPostLoginAsync("admin@test.com", "pass");
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/Admin/AdminHome", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostLoginAsync_WhenLoginFails_SetsLoginMessage()
        {
            _mockAuthService.Setup(s => s.LoginAsync("bad@test.com", "wrong", default))
                .ReturnsAsync((false, UserType.Patient, 0, "Invalid credentials"));
            var model = CreatePageModel();
            var result = await model.OnPostLoginAsync("bad@test.com", "wrong");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Invalid credentials", model.LoginMessage);
        }

        [Fact]
        public async Task OnPostLoginAsync_WhenLoginSucceeds_SetsSessionValues()
        {
            _mockAuthService.Setup(s => s.LoginAsync("patient@test.com", "pass", default))
                .ReturnsAsync((true, UserType.Patient, 42, "Success"));
            var model = CreatePageModel();
            await model.OnPostLoginAsync("patient@test.com", "pass");
            _mockSession.Verify(s => s.Set(It.Is<string>(k => k == "UserId"), It.IsAny<byte[]>()), Times.Once);
            _mockSession.Verify(s => s.Set(It.Is<string>(k => k == "UserType"), It.IsAny<byte[]>()), Times.Once);
        }

        [Fact]
        public async Task OnPostSignupAsync_WhenNameIsEmpty_SetsSignupMessage()
        {
            var model = CreatePageModel();
            var result = await model.OnPostSignupAsync("", "1990-01-01", "email@test.com", "pass",
                "555-1234", "123 St", "Male");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Please fill in all required fields.", model.SignupMessage);
        }

        [Fact]
        public async Task OnPostSignupAsync_WhenEmailIsEmpty_SetsSignupMessage()
        {
            var model = CreatePageModel();
            var result = await model.OnPostSignupAsync("John", "1990-01-01", "", "pass",
                "555-1234", "123 St", "Male");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Please fill in all required fields.", model.SignupMessage);
        }

        [Fact]
        public async Task OnPostSignupAsync_WhenPasswordIsEmpty_SetsSignupMessage()
        {
            var model = CreatePageModel();
            var result = await model.OnPostSignupAsync("John", "1990-01-01", "john@test.com", "",
                "555-1234", "123 St", "Male");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Please fill in all required fields.", model.SignupMessage);
        }

        [Fact]
        public async Task OnPostSignupAsync_WhenRegistrationSucceeds_RedirectsToPatientHome()
        {
            _mockAuthService.Setup(s => s.RegisterPatientAsync(
                "John", "1990-01-01", "john@test.com", "pass", "555-1234", "Male", "123 St", default))
                .ReturnsAsync((true, 10, "Success"));
            var model = CreatePageModel();
            var result = await model.OnPostSignupAsync("John", "1990-01-01", "john@test.com", "pass",
                "555-1234", "123 St", "Male");
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/Patient/PatientHome", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostSignupAsync_WhenRegistrationFails_SetsSignupMessage()
        {
            _mockAuthService.Setup(s => s.RegisterPatientAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
                .ReturnsAsync((false, 0, "Email already exists"));
            var model = CreatePageModel();
            var result = await model.OnPostSignupAsync("John", "1990-01-01", "john@test.com", "pass",
                "555-1234", "123 St", "Male");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Email already exists", model.SignupMessage);
            Assert.False(model.SignupSuccess);
        }

        [Fact]
        public async Task OnPostSignupAsync_WhenRegistrationSucceeds_SetsSessionValues()
        {
            _mockAuthService.Setup(s => s.RegisterPatientAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
                .ReturnsAsync((true, 15, "Success"));
            var model = CreatePageModel();
            await model.OnPostSignupAsync("John", "1990-01-01", "john@test.com", "pass",
                "555-1234", "123 St", "Male");
            _mockSession.Verify(s => s.Set(It.Is<string>(k => k == "UserId"), It.IsAny<byte[]>()), Times.Once);
            _mockSession.Verify(s => s.Set(It.Is<string>(k => k == "UserType"), It.IsAny<byte[]>()), Times.Once);
        }

        [Fact]
        public async Task OnPostLoginAsync_WhenWhitespaceEmail_SetsLoginMessage()
        {
            var model = CreatePageModel();
            var result = await model.OnPostLoginAsync("   ", "password");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Please enter both email and password.", model.LoginMessage);
        }
    }
}
