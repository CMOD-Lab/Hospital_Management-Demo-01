using System;
using System.Threading.Tasks;
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
    public class DoctorProfileModelTests
    {
        private readonly Mock<IPatientService> _mockPatientService;
        private readonly Mock<ILogger<DoctorProfileModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public DoctorProfileModelTests()
        {
            _mockPatientService = new Mock<IPatientService>();
            _mockLogger = new Mock<ILogger<DoctorProfileModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private DoctorProfileModel CreatePageModel()
        {
            var model = new DoctorProfileModel(_mockPatientService.Object, _mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new DoctorProfileModel(_mockPatientService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesNullDoctor()
        {
            var model = new DoctorProfileModel(_mockPatientService.Object, _mockLogger.Object);
            Assert.Null(model.Doctor);
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
        public async Task OnGetAsync_WhenPatientLoggedIn_LoadsDoctorProfile()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            var doctor = new ClinicManagement.Domain.Entities.Doctor { DoctorId = 5, Name = "Dr. Smith" };
            _mockPatientService.Setup(s => s.GetDoctorProfileAsync(5, default)).ReturnsAsync(doctor);
            var model = CreatePageModel();
            var result = await model.OnGetAsync(5);
            Assert.IsType<PageResult>(result);
            _mockPatientService.Verify(s => s.GetDoctorProfileAsync(5, default), Times.Once);
        }

        [Fact]
        public async Task OnGetAsync_WhenDoctorNotFound_ReturnsPageWithNullDoctor()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            _mockPatientService.Setup(s => s.GetDoctorProfileAsync(99, default))
                .ReturnsAsync((ClinicManagement.Domain.Entities.Doctor?)null);
            var model = CreatePageModel();
            var result = await model.OnGetAsync(99);
            Assert.IsType<PageResult>(result);
            Assert.Null(model.Doctor);
        }
    }
}
