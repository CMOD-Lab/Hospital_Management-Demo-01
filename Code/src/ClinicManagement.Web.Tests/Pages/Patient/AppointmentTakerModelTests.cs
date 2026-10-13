using System;
using System.Collections.Generic;
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
    public class AppointmentTakerModelTests
    {
        private readonly Mock<IPatientService> _mockPatientService;
        private readonly Mock<ILogger<AppointmentTakerModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public AppointmentTakerModelTests()
        {
            _mockPatientService = new Mock<IPatientService>();
            _mockLogger = new Mock<ILogger<AppointmentTakerModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private AppointmentTakerModel CreatePageModel()
        {
            var model = new AppointmentTakerModel(_mockPatientService.Object, _mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new AppointmentTakerModel(_mockPatientService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesDefaultProperties()
        {
            var model = new AppointmentTakerModel(_mockPatientService.Object, _mockLogger.Object);
            Assert.Null(model.Doctor);
            Assert.NotNull(model.FreeSlots);
            Assert.Equal(0, model.DoctorId);
            Assert.Equal(string.Empty, model.Message);
            Assert.False(model.IsSuccess);
        }

        [Fact]
        public async Task OnGetAsync_WhenUserNotLoggedIn_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, null, null);
            var model = CreatePageModel();
            var result = await model.OnGetAsync(1);
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnGetAsync_WhenUserTypeIsNotPatient_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 2);
            var model = CreatePageModel();
            var result = await model.OnGetAsync(1);
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnGetAsync_WhenPatientLoggedIn_LoadsDoctorAndSlots()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            var doctor = new ClinicManagement.Domain.Entities.Doctor { DoctorId = 5, Name = "Dr. Smith" };
            var slots = new List<Appointment> { new Appointment { AppointmentId = 1 } };
            _mockPatientService.Setup(s => s.GetDoctorProfileAsync(5, default)).ReturnsAsync(doctor);
            _mockPatientService.Setup(s => s.GetFreeSlotsAsync(5, 10, default)).ReturnsAsync(slots);
            var model = CreatePageModel();
            var result = await model.OnGetAsync(5);
            Assert.IsType<PageResult>(result);
            Assert.Equal(5, model.DoctorId);
            _mockPatientService.Verify(s => s.GetDoctorProfileAsync(5, default), Times.Once);
            _mockPatientService.Verify(s => s.GetFreeSlotsAsync(5, 10, default), Times.Once);
        }

        [Fact]
        public async Task OnPostAsync_WhenUserNotLoggedIn_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, null, null);
            var model = CreatePageModel();
            var result = await model.OnPostAsync(5, 1);
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostAsync_WhenBookingSucceeds_RedirectsToConfirmation()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            _mockPatientService.Setup(s => s.BookAppointmentAsync(5, 10, 1, default))
                .ReturnsAsync((true, "Booked successfully"));
            var model = CreatePageModel();
            var result = await model.OnPostAsync(5, 1);
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/Patient/AppointmentRequestSent", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostAsync_WhenBookingFails_SetsErrorMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            _mockPatientService.Setup(s => s.BookAppointmentAsync(5, 10, 1, default))
                .ReturnsAsync((false, "Slot already taken"));
            _mockPatientService.Setup(s => s.GetDoctorProfileAsync(5, default))
                .ReturnsAsync(new ClinicManagement.Domain.Entities.Doctor { DoctorId = 5 });
            _mockPatientService.Setup(s => s.GetFreeSlotsAsync(5, 10, default))
                .ReturnsAsync(new List<Appointment>());
            var model = CreatePageModel();
            var result = await model.OnPostAsync(5, 1);
            Assert.IsType<PageResult>(result);
            Assert.Equal("Slot already taken", model.Message);
            Assert.False(model.IsSuccess);
        }

        [Fact]
        public async Task OnPostAsync_WhenUserTypeIsNotPatient_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 2);
            var model = CreatePageModel();
            var result = await model.OnPostAsync(5, 1);
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }
    }
}
