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
    public class PendingAppointmentModelTests
    {
        private readonly Mock<IDoctorService> _mockDoctorService;
        private readonly Mock<ILogger<PendingAppointmentModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public PendingAppointmentModelTests()
        {
            _mockDoctorService = new Mock<IDoctorService>();
            _mockLogger = new Mock<ILogger<PendingAppointmentModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private PendingAppointmentModel CreatePageModel()
        {
            var model = new PendingAppointmentModel(_mockDoctorService.Object, _mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new PendingAppointmentModel(_mockDoctorService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesDefaultProperties()
        {
            var model = new PendingAppointmentModel(_mockDoctorService.Object, _mockLogger.Object);
            Assert.NotNull(model.PendingAppointments);
            Assert.Equal(string.Empty, model.Message);
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
        public async Task OnGetAsync_WhenDoctorLoggedIn_LoadsPendingAppointments()
        {
            SessionTestHelper.SetupSession(_mockSession, 5, 2);
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentId = 1, DoctorId = 5, IsApproved = false }
            };
            _mockDoctorService.Setup(s => s.GetPendingAppointmentsAsync(5, default)).ReturnsAsync(appointments);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            Assert.IsType<PageResult>(result);
            _mockDoctorService.Verify(s => s.GetPendingAppointmentsAsync(5, default), Times.Once);
        }

        [Fact]
        public async Task OnPostApproveAsync_WhenUserNotLoggedIn_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, null, null);
            var model = CreatePageModel();
            var result = await model.OnPostApproveAsync(1);
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostApproveAsync_WhenApproveSucceeds_SetsSuccessMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 5, 2);
            _mockDoctorService.Setup(s => s.ApproveAppointmentAsync(10, default)).ReturnsAsync(true);
            _mockDoctorService.Setup(s => s.GetPendingAppointmentsAsync(5, default)).ReturnsAsync(new List<Appointment>());
            var model = CreatePageModel();
            var result = await model.OnPostApproveAsync(10);
            Assert.IsType<PageResult>(result);
            Assert.Equal("Appointment approved successfully.", model.Message);
        }

        [Fact]
        public async Task OnPostApproveAsync_WhenApproveFails_SetsErrorMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 5, 2);
            _mockDoctorService.Setup(s => s.ApproveAppointmentAsync(10, default)).ReturnsAsync(false);
            _mockDoctorService.Setup(s => s.GetPendingAppointmentsAsync(5, default)).ReturnsAsync(new List<Appointment>());
            var model = CreatePageModel();
            var result = await model.OnPostApproveAsync(10);
            Assert.IsType<PageResult>(result);
            Assert.Equal("There was an error approving the appointment.", model.Message);
        }

        [Fact]
        public async Task OnPostDeleteAsync_WhenUserNotLoggedIn_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, null, null);
            var model = CreatePageModel();
            var result = await model.OnPostDeleteAsync(1);
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostDeleteAsync_WhenDeleteSucceeds_SetsSuccessMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 5, 2);
            _mockDoctorService.Setup(s => s.DeleteAppointmentAsync(10, default)).ReturnsAsync(true);
            _mockDoctorService.Setup(s => s.GetPendingAppointmentsAsync(5, default)).ReturnsAsync(new List<Appointment>());
            var model = CreatePageModel();
            var result = await model.OnPostDeleteAsync(10);
            Assert.IsType<PageResult>(result);
            Assert.Equal("Appointment deleted successfully.", model.Message);
        }

        [Fact]
        public async Task OnPostDeleteAsync_WhenDeleteFails_SetsErrorMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 5, 2);
            _mockDoctorService.Setup(s => s.DeleteAppointmentAsync(10, default)).ReturnsAsync(false);
            _mockDoctorService.Setup(s => s.GetPendingAppointmentsAsync(5, default)).ReturnsAsync(new List<Appointment>());
            var model = CreatePageModel();
            var result = await model.OnPostDeleteAsync(10);
            Assert.IsType<PageResult>(result);
            Assert.Equal("There was an error deleting the appointment.", model.Message);
        }

        [Fact]
        public async Task OnPostApproveAsync_WhenUserTypeIsNotDoctor_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var model = CreatePageModel();
            var result = await model.OnPostApproveAsync(1);
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostDeleteAsync_WhenUserTypeIsNotDoctor_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var model = CreatePageModel();
            var result = await model.OnPostDeleteAsync(1);
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }
    }
}
