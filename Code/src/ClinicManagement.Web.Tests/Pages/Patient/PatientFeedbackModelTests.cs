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
    public class PatientFeedbackModelTests
    {
        private readonly Mock<IPatientService> _mockPatientService;
        private readonly Mock<ILogger<PatientFeedbackModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public PatientFeedbackModelTests()
        {
            _mockPatientService = new Mock<IPatientService>();
            _mockLogger = new Mock<ILogger<PatientFeedbackModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private PatientFeedbackModel CreatePageModel()
        {
            var model = new PatientFeedbackModel(_mockPatientService.Object, _mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new PatientFeedbackModel(_mockPatientService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesDefaultProperties()
        {
            var model = new PatientFeedbackModel(_mockPatientService.Object, _mockLogger.Object);
            Assert.Null(model.PendingFeedback);
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
        public async Task OnGetAsync_WhenUserTypeIsNotPatient_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 2);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnGetAsync_WhenPatientLoggedIn_LoadsPendingFeedback()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            var appointment = new Appointment { AppointmentId = 5, PatientId = 10, FeedbackGiven = false };
            _mockPatientService.Setup(s => s.GetPendingFeedbackAsync(10, default)).ReturnsAsync(appointment);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            Assert.IsType<PageResult>(result);
            _mockPatientService.Verify(s => s.GetPendingFeedbackAsync(10, default), Times.Once);
        }

        [Fact]
        public async Task OnPostAsync_WhenUserNotLoggedIn_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, null, null);
            var model = CreatePageModel();
            var result = await model.OnPostAsync(1);
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostAsync_WhenFeedbackSubmitSucceeds_SetsSuccessMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            _mockPatientService.Setup(s => s.SubmitFeedbackAsync(5, default)).ReturnsAsync(true);
            _mockPatientService.Setup(s => s.GetPendingFeedbackAsync(10, default)).ReturnsAsync((Appointment?)null);
            var model = CreatePageModel();
            var result = await model.OnPostAsync(5);
            Assert.IsType<PageResult>(result);
            Assert.Equal("Thank you for your feedback!", model.Message);
            Assert.True(model.IsSuccess);
        }

        [Fact]
        public async Task OnPostAsync_WhenFeedbackSubmitFails_SetsErrorMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            _mockPatientService.Setup(s => s.SubmitFeedbackAsync(5, default)).ReturnsAsync(false);
            _mockPatientService.Setup(s => s.GetPendingFeedbackAsync(10, default)).ReturnsAsync((Appointment?)null);
            var model = CreatePageModel();
            var result = await model.OnPostAsync(5);
            Assert.IsType<PageResult>(result);
            Assert.Equal("There was an error submitting your feedback.", model.Message);
            Assert.False(model.IsSuccess);
        }

        [Fact]
        public async Task OnPostAsync_WhenUserTypeIsNotPatient_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var model = CreatePageModel();
            var result = await model.OnPostAsync(5);
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostAsync_AfterSubmit_ReloadsPendingFeedback()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            _mockPatientService.Setup(s => s.SubmitFeedbackAsync(It.IsAny<int>(), default)).ReturnsAsync(true);
            _mockPatientService.Setup(s => s.GetPendingFeedbackAsync(10, default)).ReturnsAsync((Appointment?)null);
            var model = CreatePageModel();
            await model.OnPostAsync(5);
            _mockPatientService.Verify(s => s.GetPendingFeedbackAsync(10, default), Times.Once);
        }
    }
}
