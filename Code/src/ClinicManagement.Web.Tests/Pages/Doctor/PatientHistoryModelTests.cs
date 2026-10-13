using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Services;
using ClinicManagement.Web.Pages.Doctor;
using ClinicManagement.Web.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Web.Tests.Pages.Doctor
{
    public class PatientHistoryModelTests
    {
        private readonly Mock<IDoctorService> _mockDoctorService;
        private readonly Mock<ILogger<PatientHistoryModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public PatientHistoryModelTests()
        {
            _mockDoctorService = new Mock<IDoctorService>();
            _mockLogger = new Mock<ILogger<PatientHistoryModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private PatientHistoryModel CreatePageModel()
        {
            var model = new PatientHistoryModel(_mockDoctorService.Object, _mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new PatientHistoryModel(_mockDoctorService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesEmptyAppointments()
        {
            var model = new PatientHistoryModel(_mockDoctorService.Object, _mockLogger.Object);
            Assert.NotNull(model.Appointments);
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
        public async Task OnGetAsync_WhenUserTypeIsNotDoctor_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 1);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnGetAsync_WhenDoctorLoggedIn_LoadsPatientHistory()
        {
            SessionTestHelper.SetupSession(_mockSession, 5, 2);
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentId = 1, DoctorId = 5, PatientId = 10, IsCompleted = true }
            };
            _mockDoctorService.Setup(s => s.GetPatientHistoryAsync(5, default)).ReturnsAsync(appointments);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            Assert.IsType<PageResult>(result);
            _mockDoctorService.Verify(s => s.GetPatientHistoryAsync(5, default), Times.Once);
        }

        [Fact]
        public async Task OnGetAsync_WhenAdminUserType_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }
    }
}
