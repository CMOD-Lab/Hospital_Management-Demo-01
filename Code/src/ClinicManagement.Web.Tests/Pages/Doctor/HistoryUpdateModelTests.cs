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
    public class HistoryUpdateModelTests
    {
        private readonly Mock<IDoctorService> _mockDoctorService;
        private readonly Mock<ILogger<HistoryUpdateModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public HistoryUpdateModelTests()
        {
            _mockDoctorService = new Mock<IDoctorService>();
            _mockLogger = new Mock<ILogger<HistoryUpdateModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private HistoryUpdateModel CreatePageModel()
        {
            var model = new HistoryUpdateModel(_mockDoctorService.Object, _mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new HistoryUpdateModel(_mockDoctorService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesDefaultProperties()
        {
            var model = new HistoryUpdateModel(_mockDoctorService.Object, _mockLogger.Object);
            Assert.NotNull(model.TodaysAppointments);
            Assert.Equal(string.Empty, model.Message);
            Assert.False(model.IsSuccess);
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
        public async Task OnGetAsync_WhenDoctorLoggedIn_LoadsTodaysAppointments()
        {
            SessionTestHelper.SetupSession(_mockSession, 5, 2);
            var appointments = new List<Appointment> { new Appointment { AppointmentId = 1, DoctorId = 5 } };
            _mockDoctorService.Setup(s => s.GetTodaysAppointmentsAsync(5, default)).ReturnsAsync(appointments);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            Assert.IsType<PageResult>(result);
            _mockDoctorService.Verify(s => s.GetTodaysAppointmentsAsync(5, default), Times.Once);
        }

        [Fact]
        public async Task OnPostAsync_WhenUserNotLoggedIn_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, null, null);
            var model = CreatePageModel();
            var result = await model.OnPostAsync(1, "Flu", "Improving", "Paracetamol");
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostAsync_WhenUserTypeIsNotDoctor_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var model = CreatePageModel();
            var result = await model.OnPostAsync(1, "Flu", "Improving", "Paracetamol");
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostAsync_WhenUpdateSucceeds_SetsSuccessMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 5, 2);
            _mockDoctorService.Setup(s => s.UpdatePrescriptionAsync(5, 10, "Flu", "Improving", "Paracetamol", default)).ReturnsAsync(true);
            _mockDoctorService.Setup(s => s.GetTodaysAppointmentsAsync(5, default)).ReturnsAsync(new List<Appointment>());
            var model = CreatePageModel();
            var result = await model.OnPostAsync(10, "Flu", "Improving", "Paracetamol");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Prescription updated successfully!", model.Message);
            Assert.True(model.IsSuccess);
        }

        [Fact]
        public async Task OnPostAsync_WhenUpdateFails_SetsErrorMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 5, 2);
            _mockDoctorService.Setup(s => s.UpdatePrescriptionAsync(5, 10, "Flu", "Improving", "Paracetamol", default)).ReturnsAsync(false);
            _mockDoctorService.Setup(s => s.GetTodaysAppointmentsAsync(5, default)).ReturnsAsync(new List<Appointment>());
            var model = CreatePageModel();
            var result = await model.OnPostAsync(10, "Flu", "Improving", "Paracetamol");
            Assert.IsType<PageResult>(result);
            Assert.Equal("There was an error updating the prescription.", model.Message);
            Assert.False(model.IsSuccess);
        }

        [Fact]
        public async Task OnPostAsync_AfterUpdate_ReloadsAppointments()
        {
            SessionTestHelper.SetupSession(_mockSession, 5, 2);
            _mockDoctorService.Setup(s => s.UpdatePrescriptionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default)).ReturnsAsync(true);
            _mockDoctorService.Setup(s => s.GetTodaysAppointmentsAsync(5, default)).ReturnsAsync(new List<Appointment>());
            var model = CreatePageModel();
            await model.OnPostAsync(10, "Flu", "Improving", "Paracetamol");
            _mockDoctorService.Verify(s => s.GetTodaysAppointmentsAsync(5, default), Times.Once);
        }
    }
}
