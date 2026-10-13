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
    public class ViewDoctorsModelTests
    {
        private readonly Mock<IPatientService> _mockPatientService;
        private readonly Mock<ILogger<ViewDoctorsModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public ViewDoctorsModelTests()
        {
            _mockPatientService = new Mock<IPatientService>();
            _mockLogger = new Mock<ILogger<ViewDoctorsModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private ViewDoctorsModel CreatePageModel()
        {
            var model = new ViewDoctorsModel(_mockPatientService.Object, _mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new ViewDoctorsModel(_mockPatientService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesDefaultProperties()
        {
            var model = new ViewDoctorsModel(_mockPatientService.Object, _mockLogger.Object);
            Assert.NotNull(model.Departments);
            Assert.NotNull(model.Doctors);
            Assert.Equal(string.Empty, model.SelectedDept);
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
        public async Task OnGetAsync_WhenPatientLoggedIn_LoadsDepartments()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            var departments = new List<Department>
            {
                new Department { DeptNo = 1, DeptName = "Cardiology" },
                new Department { DeptNo = 2, DeptName = "Neurology" }
            };
            _mockPatientService.Setup(s => s.GetDepartmentsAsync(default)).ReturnsAsync(departments);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            Assert.IsType<PageResult>(result);
            _mockPatientService.Verify(s => s.GetDepartmentsAsync(default), Times.Once);
        }

        [Fact]
        public async Task OnGetAsync_WhenDeptNameProvided_LoadsDoctors()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            _mockPatientService.Setup(s => s.GetDepartmentsAsync(default)).ReturnsAsync(new List<Department>());
            var doctors = new List<ClinicManagement.Domain.Entities.Doctor>
            {
                new ClinicManagement.Domain.Entities.Doctor { DoctorId = 1, Name = "Dr. Smith" }
            };
            _mockPatientService.Setup(s => s.GetDoctorsByDepartmentAsync("Cardiology", default)).ReturnsAsync(doctors);
            var model = CreatePageModel();
            var result = await model.OnGetAsync("Cardiology");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Cardiology", model.SelectedDept);
            _mockPatientService.Verify(s => s.GetDoctorsByDepartmentAsync("Cardiology", default), Times.Once);
        }

        [Fact]
        public async Task OnGetAsync_WhenDeptNameIsEmpty_DoesNotLoadDoctors()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            _mockPatientService.Setup(s => s.GetDepartmentsAsync(default)).ReturnsAsync(new List<Department>());
            var model = CreatePageModel();
            await model.OnGetAsync("");
            _mockPatientService.Verify(s => s.GetDoctorsByDepartmentAsync(It.IsAny<string>(), default), Times.Never);
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
        public async Task OnGetAsync_WithDeptName_SetsSelectedDept()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            _mockPatientService.Setup(s => s.GetDepartmentsAsync(default)).ReturnsAsync(new List<Department>());
            _mockPatientService.Setup(s => s.GetDoctorsByDepartmentAsync("Neurology", default))
                .ReturnsAsync(new List<ClinicManagement.Domain.Entities.Doctor>());
            var model = CreatePageModel();
            await model.OnGetAsync("Neurology");
            Assert.Equal("Neurology", model.SelectedDept);
        }
    }
}
