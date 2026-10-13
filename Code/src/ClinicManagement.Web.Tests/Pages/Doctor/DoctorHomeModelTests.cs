using System;
using System.Threading.Tasks;
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
    public class DoctorHomeModelTests
    {
        private readonly Mock<IDoctorService> _mockDoctorService;
        private readonly Mock<ILogger<DoctorHomeModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public DoctorHomeModelTests()
        {
            _mockDoctorService = new Mock<IDoctorService>();
            _mockLogger = new Mock<ILogger<DoctorHomeModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private DoctorHomeModel CreatePageModel()
        {
            var model = new DoctorHomeModel(_mockDoctorService.Object, _mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new DoctorHomeModel(_mockDoctorService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesNullDoctor()
        {
            var model = new DoctorHomeModel(_mockDoctorService.Object, _mockLogger.Object);
            Assert.Null(model.Doctor);
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
        public async Task OnGetAsync_WhenAdminUserType_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnGetAsync_WhenDoctorLoggedIn_LoadsDoctorInfo()
        {
            SessionTestHelper.SetupSession(_mockSession, 7, 2);
            var doctor = new ClinicManagement.Domain.Entities.Doctor { DoctorId = 7, Name = "Dr. Smith" };
            _mockDoctorService.Setup(s => s.GetDoctorInfoAsync(7, default)).ReturnsAsync(doctor);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            Assert.IsType<PageResult>(result);
            Assert.NotNull(model.Doctor);
            Assert.Equal("Dr. Smith", model.Doctor!.Name);
        }

        [Fact]
        public async Task OnGetAsync_WhenDoctorNotFound_ReturnsPageWithNullDoctor()
        {
            SessionTestHelper.SetupSession(_mockSession, 7, 2);
            _mockDoctorService.Setup(s => s.GetDoctorInfoAsync(7, default))
                .ReturnsAsync((ClinicManagement.Domain.Entities.Doctor?)null);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            Assert.IsType<PageResult>(result);
            Assert.Null(model.Doctor);
        }

        [Fact]
        public async Task OnGetAsync_CallsServiceWithCorrectDoctorId()
        {
            SessionTestHelper.SetupSession(_mockSession, 42, 2);
            _mockDoctorService.Setup(s => s.GetDoctorInfoAsync(42, default))
                .ReturnsAsync(new ClinicManagement.Domain.Entities.Doctor { DoctorId = 42 });
            var model = CreatePageModel();
            await model.OnGetAsync();
            _mockDoctorService.Verify(s => s.GetDoctorInfoAsync(42, default), Times.Once);
        }
    }
}
