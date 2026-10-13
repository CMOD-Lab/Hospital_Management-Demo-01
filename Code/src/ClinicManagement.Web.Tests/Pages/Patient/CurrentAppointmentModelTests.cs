using System;
using System.Threading.Tasks;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Services;
using ClinicManagement.Web.Pages.Patient;
using ClinicManagement.Web.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Web.Tests.Pages.Patient
{
    public class CurrentAppointmentModelTests
    {
        private readonly Mock<IPatientService> _mockPatientService;
        private readonly Mock<ILogger<CurrentAppointmentModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public CurrentAppointmentModelTests()
        {
            _mockPatientService = new Mock<IPatientService>();
            _mockLogger = new Mock<ILogger<CurrentAppointmentModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private CurrentAppointmentModel CreatePageModel()
        {
            var model = new CurrentAppointmentModel(_mockPatientService.Object, _mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new CurrentAppointmentModel(_mockPatientService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesNullAppointment()
        {
            var model = new CurrentAppointmentModel(_mockPatientService.Object, _mockLogger.Object);
            Assert.Null(model.Appointment);
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
        public async Task OnGetAsync_WhenUserTypeIsNotPatient_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 2);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnGetAsync_WhenPatientLoggedIn_LoadsCurrentAppointment()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            var appointment = new Appointment { AppointmentId = 5, PatientId = 10, IsApproved = true };
            _mockPatientService.Setup(s => s.GetCurrentAppointmentAsync(10, default)).ReturnsAsync(appointment);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            Assert.IsType<PageResult>(result);
            _mockPatientService.Verify(s => s.GetCurrentAppointmentAsync(10, default), Times.Once);
        }

        [Fact]
        public async Task OnGetAsync_WhenNoCurrentAppointment_ReturnsPageWithNullAppointment()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            _mockPatientService.Setup(s => s.GetCurrentAppointmentAsync(10, default))
                .ReturnsAsync((Appointment?)null);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            Assert.IsType<PageResult>(result);
            Assert.Null(model.Appointment);
        }
    }
}
