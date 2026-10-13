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
    public class BillModelTests
    {
        private readonly Mock<IDoctorService> _mockDoctorService;
        private readonly Mock<ILogger<BillModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public BillModelTests()
        {
            _mockDoctorService = new Mock<IDoctorService>();
            _mockLogger = new Mock<ILogger<BillModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private BillModel CreatePageModel()
        {
            var model = new BillModel(_mockDoctorService.Object, _mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new BillModel(_mockDoctorService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesDefaultProperties()
        {
            var model = new BillModel(_mockDoctorService.Object, _mockLogger.Object);
            Assert.NotNull(model.TodaysAppointments);
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
        public async Task OnGetAsync_WhenDoctorLoggedIn_LoadsTodaysAppointments()
        {
            SessionTestHelper.SetupSession(_mockSession, 5, 2);
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentId = 1, DoctorId = 5, PatientId = 10 }
            };
            _mockDoctorService.Setup(s => s.GetTodaysAppointmentsAsync(5, default)).ReturnsAsync(appointments);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            Assert.IsType<PageResult>(result);
            _mockDoctorService.Verify(s => s.GetTodaysAppointmentsAsync(5, default), Times.Once);
        }

        [Fact]
        public async Task OnPostPaidAsync_WhenUserNotLoggedIn_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, null, null);
            var model = CreatePageModel();
            var result = await model.OnPostPaidAsync(1);
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostPaidAsync_WhenDoctorLoggedIn_MarksBillPaid()
        {
            SessionTestHelper.SetupSession(_mockSession, 5, 2);
            _mockDoctorService.Setup(s => s.MarkBillPaidAsync(5, 10, default)).ReturnsAsync(true);
            _mockDoctorService.Setup(s => s.GetTodaysAppointmentsAsync(5, default)).ReturnsAsync(new List<Appointment>());
            var model = CreatePageModel();
            var result = await model.OnPostPaidAsync(10);
            Assert.IsType<PageResult>(result);
            Assert.Equal("Bill marked as paid.", model.Message);
            _mockDoctorService.Verify(s => s.MarkBillPaidAsync(5, 10, default), Times.Once);
        }

        [Fact]
        public async Task OnPostUnpaidAsync_WhenUserNotLoggedIn_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, null, null);
            var model = CreatePageModel();
            var result = await model.OnPostUnpaidAsync(1);
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostUnpaidAsync_WhenDoctorLoggedIn_MarksBillUnpaid()
        {
            SessionTestHelper.SetupSession(_mockSession, 5, 2);
            _mockDoctorService.Setup(s => s.MarkBillUnpaidAsync(5, 10, default)).ReturnsAsync(true);
            _mockDoctorService.Setup(s => s.GetTodaysAppointmentsAsync(5, default)).ReturnsAsync(new List<Appointment>());
            var model = CreatePageModel();
            var result = await model.OnPostUnpaidAsync(10);
            Assert.IsType<PageResult>(result);
            Assert.Equal("Bill marked as unpaid.", model.Message);
            _mockDoctorService.Verify(s => s.MarkBillUnpaidAsync(5, 10, default), Times.Once);
        }

        [Fact]
        public async Task OnPostPaidAsync_WhenUserTypeIsNotDoctor_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var model = CreatePageModel();
            var result = await model.OnPostPaidAsync(1);
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostUnpaidAsync_WhenUserTypeIsNotDoctor_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var model = CreatePageModel();
            var result = await model.OnPostUnpaidAsync(1);
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }
    }
}
