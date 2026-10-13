using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Services;
using ClinicManagement.Web.Pages.Admin;
using ClinicManagement.Web.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Web.Tests.Pages.Admin
{
    public class DoctorRegistrationFormModelTests
    {
        private readonly Mock<IAdminService> _mockAdminService;
        private readonly Mock<ILogger<DoctorRegistrationFormModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public DoctorRegistrationFormModelTests()
        {
            _mockAdminService = new Mock<IAdminService>();
            _mockLogger = new Mock<ILogger<DoctorRegistrationFormModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private DoctorRegistrationFormModel CreatePageModel()
        {
            var model = new DoctorRegistrationFormModel(_mockAdminService.Object, _mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new DoctorRegistrationFormModel(_mockAdminService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesDefaultProperties()
        {
            var model = new DoctorRegistrationFormModel(_mockAdminService.Object, _mockLogger.Object);
            Assert.Equal(string.Empty, model.Message);
            Assert.False(model.IsSuccess);
            Assert.NotNull(model.Departments);
        }

        [Fact]
        public async Task OnGetAsync_WhenUserNotLoggedIn_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, null, null);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnGetAsync_WhenUserTypeIsNotAdmin_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 2);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnGetAsync_WhenAdminLoggedIn_LoadsDepartments()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var departments = new List<Department>
            {
                new Department { DeptNo = 1, DeptName = "Cardiology" },
                new Department { DeptNo = 2, DeptName = "Neurology" }
            };
            _mockAdminService.Setup(s => s.GetDepartmentsAsync(default)).ReturnsAsync(departments);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            Assert.IsType<PageResult>(result);
            Assert.NotNull(model.Departments);
        }

        [Fact]
        public async Task OnPostAsync_WhenUserNotLoggedIn_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, null, null);
            var model = CreatePageModel();
            var result = await model.OnPostAsync("Dr. Smith", "smith@clinic.com", "pass123",
                "1980-01-01", 1, "555-1234", "123 Main St", "Male", 10, 80000, 500, "Cardiology", "MD");
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostAsync_WhenDepartmentIsZero_SetsErrorMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            _mockAdminService.Setup(s => s.GetDepartmentsAsync(default)).ReturnsAsync(new List<Department>());
            var model = CreatePageModel();
            var result = await model.OnPostAsync("Dr. Smith", "smith@clinic.com", "pass123",
                "1980-01-01", 0, "555-1234", "123 Main St", "Male", 10, 80000, 500, "Cardiology", "MD");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Please select a department.", model.Message);
            Assert.False(model.IsSuccess);
        }

        [Fact]
        public async Task OnPostAsync_WhenEmailAlreadyExists_SetsErrorMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            _mockAdminService.Setup(s => s.GetDepartmentsAsync(default)).ReturnsAsync(new List<Department>());
            _mockAdminService.Setup(s => s.DoctorEmailExistsAsync("smith@clinic.com", default)).ReturnsAsync(true);
            var model = CreatePageModel();
            var result = await model.OnPostAsync("Dr. Smith", "smith@clinic.com", "pass123",
                "1980-01-01", 1, "555-1234", "123 Main St", "Male", 10, 80000, 500, "Cardiology", "MD");
            Assert.IsType<PageResult>(result);
            Assert.Equal("This email already exists. Please choose a different one!", model.Message);
            Assert.False(model.IsSuccess);
        }

        [Fact]
        public async Task OnPostAsync_WhenAddDoctorSucceeds_SetsSuccessMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            _mockAdminService.Setup(s => s.GetDepartmentsAsync(default)).ReturnsAsync(new List<Department>());
            _mockAdminService.Setup(s => s.DoctorEmailExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
            _mockAdminService.Setup(s => s.AddDoctorAsync(It.IsAny<ClinicManagement.Domain.Entities.Doctor>(), default)).ReturnsAsync(true);
            var model = CreatePageModel();
            var result = await model.OnPostAsync("Dr. Smith", "smith@clinic.com", "pass123",
                "1980-01-01", 1, "555-1234", "123 Main St", "Male", 10, 80000, 500, "Cardiology", "MD");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Doctor added successfully!", model.Message);
            Assert.True(model.IsSuccess);
        }

        [Fact]
        public async Task OnPostAsync_WhenAddDoctorFails_SetsErrorMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            _mockAdminService.Setup(s => s.GetDepartmentsAsync(default)).ReturnsAsync(new List<Department>());
            _mockAdminService.Setup(s => s.DoctorEmailExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
            _mockAdminService.Setup(s => s.AddDoctorAsync(It.IsAny<ClinicManagement.Domain.Entities.Doctor>(), default)).ReturnsAsync(false);
            var model = CreatePageModel();
            var result = await model.OnPostAsync("Dr. Smith", "smith@clinic.com", "pass123",
                "1980-01-01", 1, "555-1234", "123 Main St", "Male", 10, 80000, 500, "Cardiology", "MD");
            Assert.IsType<PageResult>(result);
            Assert.Equal("There was an error adding the doctor. Please try again.", model.Message);
            Assert.False(model.IsSuccess);
        }

        [Fact]
        public async Task OnPostAsync_WithInvalidBirthDate_UsesCurrentDate()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            ClinicManagement.Domain.Entities.Doctor? capturedDoctor = null;
            _mockAdminService.Setup(s => s.GetDepartmentsAsync(default)).ReturnsAsync(new List<Department>());
            _mockAdminService.Setup(s => s.DoctorEmailExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
            _mockAdminService.Setup(s => s.AddDoctorAsync(It.IsAny<ClinicManagement.Domain.Entities.Doctor>(), default))
                .Callback<ClinicManagement.Domain.Entities.Doctor, System.Threading.CancellationToken>((d, _) => capturedDoctor = d)
                .ReturnsAsync(true);
            var model = CreatePageModel();
            await model.OnPostAsync("Dr. Smith", "not-a-date", "pass123",
                "not-a-date", 1, "555-1234", "123 Main St", "Male", 10, 80000, 500, "Cardiology", "MD");
            Assert.NotNull(capturedDoctor);
            Assert.True(capturedDoctor!.BirthDate <= DateTime.Now);
        }

        [Fact]
        public async Task OnPostAsync_SetsDoctorPropertiesCorrectly()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            ClinicManagement.Domain.Entities.Doctor? capturedDoctor = null;
            _mockAdminService.Setup(s => s.GetDepartmentsAsync(default)).ReturnsAsync(new List<Department>());
            _mockAdminService.Setup(s => s.DoctorEmailExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
            _mockAdminService.Setup(s => s.AddDoctorAsync(It.IsAny<ClinicManagement.Domain.Entities.Doctor>(), default))
                .Callback<ClinicManagement.Domain.Entities.Doctor, System.Threading.CancellationToken>((d, _) => capturedDoctor = d)
                .ReturnsAsync(true);
            var model = CreatePageModel();
            await model.OnPostAsync("Dr. Jane", "jane@clinic.com", "secret",
                "1975-06-15", 2, "555-9999", "789 Oak Ave", "Female", 15, 90000, 750, "Neurology", "PhD");
            Assert.NotNull(capturedDoctor);
            Assert.Equal("Dr. Jane", capturedDoctor!.Name);
            Assert.Equal("jane@clinic.com", capturedDoctor.Email);
            Assert.Equal(2, capturedDoctor.DeptNo);
            Assert.Equal("Female", capturedDoctor.Gender);
            Assert.Equal(15, capturedDoctor.Experience);
            Assert.Equal(90000, capturedDoctor.Salary);
            Assert.Equal(750, capturedDoctor.ChargesPerVisit);
            Assert.Equal("Neurology", capturedDoctor.Specialization);
            Assert.Equal("PhD", capturedDoctor.Qualification);
            Assert.Equal(1, capturedDoctor.Status);
        }
    }
}
