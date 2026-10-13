using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class AdminHomeModelTests
    {
        private readonly Mock<IAdminService> _mockAdminService;
        private readonly Mock<ILogger<AdminHomeModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public AdminHomeModelTests()
        {
            _mockAdminService = new Mock<IAdminService>();
            _mockLogger = new Mock<ILogger<AdminHomeModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private AdminHomeModel CreatePageModel()
        {
            var model = new AdminHomeModel(_mockAdminService.Object, _mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new AdminHomeModel(_mockAdminService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesDefaultProperties()
        {
            var model = new AdminHomeModel(_mockAdminService.Object, _mockLogger.Object);
            Assert.Equal(0, model.TotalDoctors);
            Assert.Equal(0, model.TotalPatients);
            Assert.Equal(0m, model.TotalIncome);
            Assert.NotNull(model.DepartmentStats);
            Assert.NotNull(model.AppointmentStats);
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
        public async Task OnGetAsync_WhenAdminLoggedIn_LoadsDashboardData()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var dashboardData = new AdminDashboardData
            {
                TotalDoctors = 10,
                TotalPatients = 50,
                TotalIncome = 25000m,
                DepartmentStats = new List<DepartmentStat>
                {
                    new DepartmentStat { DeptName = "Cardiology", DoctorCount = 3 }
                },
                AppointmentStats = new List<AppointmentStat>
                {
                    new AppointmentStat { AppointmentId = 1, PatientName = "John", DoctorName = "Dr. Smith", Timings = "10:00 AM", IsApproved = true }
                }
            };
            _mockAdminService.Setup(s => s.GetDashboardDataAsync(default)).ReturnsAsync(dashboardData);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            Assert.IsType<PageResult>(result);
            Assert.Equal(10, model.TotalDoctors);
            Assert.Equal(50, model.TotalPatients);
            Assert.Equal(25000m, model.TotalIncome);
        }

        [Fact]
        public async Task OnGetAsync_WhenAdminLoggedIn_MapsDepartmentStats()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var dashboardData = new AdminDashboardData
            {
                DepartmentStats = new List<DepartmentStat>
                {
                    new DepartmentStat { DeptName = "Cardiology", DoctorCount = 3 },
                    new DepartmentStat { DeptName = "Neurology", DoctorCount = 2 }
                },
                AppointmentStats = new List<AppointmentStat>()
            };
            _mockAdminService.Setup(s => s.GetDashboardDataAsync(default)).ReturnsAsync(dashboardData);
            var model = CreatePageModel();
            await model.OnGetAsync();
            var deptStats = model.DepartmentStats.ToList();
            Assert.Equal(2, deptStats.Count);
            Assert.Equal("Cardiology", deptStats[0].DeptName);
            Assert.Equal(3, deptStats[0].DoctorCount);
        }

        [Fact]
        public async Task OnGetAsync_WhenAdminLoggedIn_MapsAppointmentStats()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var dashboardData = new AdminDashboardData
            {
                DepartmentStats = new List<DepartmentStat>(),
                AppointmentStats = new List<AppointmentStat>
                {
                    new AppointmentStat { AppointmentId = 5, PatientName = "Alice", DoctorName = "Dr. Bob", Timings = "2:00 PM", IsApproved = false }
                }
            };
            _mockAdminService.Setup(s => s.GetDashboardDataAsync(default)).ReturnsAsync(dashboardData);
            var model = CreatePageModel();
            await model.OnGetAsync();
            var apptStats = model.AppointmentStats.ToList();
            Assert.Single(apptStats);
            Assert.Equal(5, apptStats[0].AppointmentId);
            Assert.Equal("Alice", apptStats[0].PatientName);
            Assert.Equal("Dr. Bob", apptStats[0].DoctorName);
            Assert.Equal("2:00 PM", apptStats[0].Timings);
            Assert.False(apptStats[0].IsApproved);
        }

        [Fact]
        public async Task OnGetAsync_WhenServiceThrowsException_ReturnsPage()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            _mockAdminService.Setup(s => s.GetDashboardDataAsync(default))
                .ThrowsAsync(new Exception("Service error"));
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public void DepartmentStatViewModel_DefaultProperties()
        {
            var vm = new DepartmentStatViewModel();
            Assert.Equal(string.Empty, vm.DeptName);
            Assert.Equal(0, vm.DoctorCount);
        }

        [Fact]
        public void AppointmentStatViewModel_DefaultProperties()
        {
            var vm = new AppointmentStatViewModel();
            Assert.Equal(0, vm.AppointmentId);
            Assert.Equal(string.Empty, vm.PatientName);
            Assert.Equal(string.Empty, vm.DoctorName);
            Assert.Equal(string.Empty, vm.Timings);
            Assert.False(vm.IsApproved);
        }

        [Fact]
        public void DepartmentStatViewModel_SetProperties()
        {
            var vm = new DepartmentStatViewModel { DeptName = "Cardiology", DoctorCount = 5 };
            Assert.Equal("Cardiology", vm.DeptName);
            Assert.Equal(5, vm.DoctorCount);
        }

        [Fact]
        public void AppointmentStatViewModel_SetProperties()
        {
            var vm = new AppointmentStatViewModel
            {
                AppointmentId = 10, PatientName = "John Doe",
                DoctorName = "Dr. Smith", Timings = "9:00 AM", IsApproved = true
            };
            Assert.Equal(10, vm.AppointmentId);
            Assert.Equal("John Doe", vm.PatientName);
            Assert.Equal("Dr. Smith", vm.DoctorName);
            Assert.Equal("9:00 AM", vm.Timings);
            Assert.True(vm.IsApproved);
        }
    }
}
