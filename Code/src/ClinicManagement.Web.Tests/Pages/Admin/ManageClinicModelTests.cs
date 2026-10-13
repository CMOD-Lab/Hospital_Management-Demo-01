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
    public class ManageClinicModelTests
    {
        private readonly Mock<IAdminService> _mockAdminService;
        private readonly Mock<ILogger<ManageClinicModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public ManageClinicModelTests()
        {
            _mockAdminService = new Mock<IAdminService>();
            _mockLogger = new Mock<ILogger<ManageClinicModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private ManageClinicModel CreatePageModel()
        {
            var model = new ManageClinicModel(_mockAdminService.Object, _mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new ManageClinicModel(_mockAdminService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesDefaultProperties()
        {
            var model = new ManageClinicModel(_mockAdminService.Object, _mockLogger.Object);
            Assert.Equal("DOCTOR", model.Category);
            Assert.Equal(string.Empty, model.SearchQuery);
            Assert.Equal(string.Empty, model.Message);
            Assert.NotNull(model.Doctors);
            Assert.NotNull(model.Patients);
            Assert.NotNull(model.Staff);
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
            SessionTestHelper.SetupSession(_mockSession, 1, 1);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnGetAsync_WhenAdminLoggedIn_WithDoctorCategory_LoadsDoctors()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var doctors = new List<ClinicManagement.Domain.Entities.Doctor>
            {
                new ClinicManagement.Domain.Entities.Doctor { DoctorId = 1, Name = "Dr. Smith" }
            };
            _mockAdminService.Setup(s => s.GetDoctorsAsync("", default)).ReturnsAsync(doctors);
            var model = CreatePageModel();
            var result = await model.OnGetAsync("DOCTOR", "");
            Assert.IsType<PageResult>(result);
            Assert.Equal("DOCTOR", model.Category);
            _mockAdminService.Verify(s => s.GetDoctorsAsync("", default), Times.Once);
        }

        [Fact]
        public async Task OnGetAsync_WhenAdminLoggedIn_WithPatientCategory_LoadsPatients()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var patients = new List<ClinicManagement.Domain.Entities.Patient>
            {
                new ClinicManagement.Domain.Entities.Patient { PatientId = 1, Name = "John Doe" }
            };
            _mockAdminService.Setup(s => s.GetPatientsAsync("", default)).ReturnsAsync(patients);
            var model = CreatePageModel();
            var result = await model.OnGetAsync("PATIENT", "");
            Assert.IsType<PageResult>(result);
            Assert.Equal("PATIENT", model.Category);
            _mockAdminService.Verify(s => s.GetPatientsAsync("", default), Times.Once);
        }

        [Fact]
        public async Task OnGetAsync_WhenAdminLoggedIn_WithStaffCategory_LoadsStaff()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var staff = new List<OtherStaff>
            {
                new OtherStaff { StaffId = 1, Name = "Jane Nurse" }
            };
            _mockAdminService.Setup(s => s.GetStaffAsync("", default)).ReturnsAsync(staff);
            var model = CreatePageModel();
            var result = await model.OnGetAsync("STAFF", "");
            Assert.IsType<PageResult>(result);
            Assert.Equal("STAFF", model.Category);
            _mockAdminService.Verify(s => s.GetStaffAsync("", default), Times.Once);
        }

        [Fact]
        public async Task OnGetAsync_WithSearchQuery_PassesQueryToService()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            _mockAdminService.Setup(s => s.GetDoctorsAsync("Smith", default))
                .ReturnsAsync(new List<ClinicManagement.Domain.Entities.Doctor>());
            var model = CreatePageModel();
            await model.OnGetAsync("DOCTOR", "Smith");
            Assert.Equal("Smith", model.SearchQuery);
            _mockAdminService.Verify(s => s.GetDoctorsAsync("Smith", default), Times.Once);
        }

        [Fact]
        public async Task OnPostDeleteAsync_WhenUserNotLoggedIn_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, null, null);
            var model = CreatePageModel();
            var result = await model.OnPostDeleteAsync(1, "DOCTOR");
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostDeleteAsync_WhenDoctorCategory_DeletesDoctor()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            _mockAdminService.Setup(s => s.DeleteDoctorAsync(5, default)).ReturnsAsync(true);
            _mockAdminService.Setup(s => s.GetDoctorsAsync("", default))
                .ReturnsAsync(new List<ClinicManagement.Domain.Entities.Doctor>());
            var model = CreatePageModel();
            var result = await model.OnPostDeleteAsync(5, "DOCTOR");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Doctor No: 5 deleted successfully.", model.Message);
        }

        [Fact]
        public async Task OnPostDeleteAsync_WhenDoctorDeleteFails_SetsErrorMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            _mockAdminService.Setup(s => s.DeleteDoctorAsync(5, default)).ReturnsAsync(false);
            _mockAdminService.Setup(s => s.GetDoctorsAsync("", default))
                .ReturnsAsync(new List<ClinicManagement.Domain.Entities.Doctor>());
            var model = CreatePageModel();
            var result = await model.OnPostDeleteAsync(5, "DOCTOR");
            Assert.IsType<PageResult>(result);
            Assert.Equal("There was some error.", model.Message);
        }

        [Fact]
        public async Task OnPostDeleteAsync_WhenPatientCategory_SetsUnauthorizedMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            _mockAdminService.Setup(s => s.GetPatientsAsync("", default))
                .ReturnsAsync(new List<ClinicManagement.Domain.Entities.Patient>());
            var model = CreatePageModel();
            var result = await model.OnPostDeleteAsync(1, "PATIENT");
            Assert.IsType<PageResult>(result);
            Assert.Equal("You are not authorized to delete a patient.", model.Message);
        }

        [Fact]
        public async Task OnPostDeleteAsync_WhenStaffCategory_DeletesStaff()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            _mockAdminService.Setup(s => s.DeleteStaffAsync(3, default)).ReturnsAsync(true);
            _mockAdminService.Setup(s => s.GetStaffAsync("", default)).ReturnsAsync(new List<OtherStaff>());
            var model = CreatePageModel();
            var result = await model.OnPostDeleteAsync(3, "STAFF");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Staff No: 3 deleted successfully.", model.Message);
        }

        [Fact]
        public async Task OnPostDeleteAsync_WhenStaffDeleteFails_SetsErrorMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            _mockAdminService.Setup(s => s.DeleteStaffAsync(3, default)).ReturnsAsync(false);
            _mockAdminService.Setup(s => s.GetStaffAsync("", default)).ReturnsAsync(new List<OtherStaff>());
            var model = CreatePageModel();
            var result = await model.OnPostDeleteAsync(3, "STAFF");
            Assert.IsType<PageResult>(result);
            Assert.Equal("There was some error.", model.Message);
        }
    }
}
