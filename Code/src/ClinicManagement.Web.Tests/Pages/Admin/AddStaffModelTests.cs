using System;
using System.Threading.Tasks;
using ClinicManagement.Domain.Entities;
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
    public class AddStaffModelTests
    {
        private readonly Mock<IAdminService> _mockAdminService;
        private readonly Mock<ILogger<AddStaffModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public AddStaffModelTests()
        {
            _mockAdminService = new Mock<IAdminService>();
            _mockLogger = new Mock<ILogger<AddStaffModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private AddStaffModel CreatePageModel()
        {
            var model = new AddStaffModel(_mockAdminService.Object, _mockLogger.Object);
            model.PageContext = new PageContext
            {
                HttpContext = _mockHttpContext.Object
            };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new AddStaffModel(_mockAdminService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesDefaultProperties()
        {
            var model = new AddStaffModel(_mockAdminService.Object, _mockLogger.Object);
            Assert.Equal(string.Empty, model.Message);
            Assert.False(model.IsSuccess);
        }

        [Fact]
        public void OnGet_WhenUserNotLoggedIn_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, null, null);
            var model = CreatePageModel();
            var result = model.OnGet();
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public void OnGet_WhenUserTypeIsNotAdmin_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 1);
            var model = CreatePageModel();
            var result = model.OnGet();
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public void OnGet_WhenAdminLoggedIn_ReturnsPage()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            var model = CreatePageModel();
            var result = model.OnGet();
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task OnPostAsync_WhenUserNotLoggedIn_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, null, null);
            var model = CreatePageModel();
            var result = await model.OnPostAsync("John", "1990-01-01", "1234567890", "Male",
                "123 Street", 50000, "Nurse", "BSc Nursing");
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostAsync_WhenUserTypeIsNotAdmin_RedirectsToSignUp()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 2);
            var model = CreatePageModel();
            var result = await model.OnPostAsync("John", "1990-01-01", "1234567890", "Male",
                "123 Street", 50000, "Nurse", "BSc Nursing");
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/SignUp", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostAsync_WhenAddStaffSucceeds_SetsSuccessMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            _mockAdminService.Setup(s => s.AddStaffAsync(It.IsAny<OtherStaff>(), default))
                .ReturnsAsync(true);
            var model = CreatePageModel();
            var result = await model.OnPostAsync("John Doe", "1990-01-01", "1234567890", "Male",
                "123 Street", 50000, "Nurse", "BSc Nursing");
            Assert.IsType<PageResult>(result);
            Assert.Equal("Staff member added successfully!", model.Message);
            Assert.True(model.IsSuccess);
        }

        [Fact]
        public async Task OnPostAsync_WhenAddStaffFails_SetsErrorMessage()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            _mockAdminService.Setup(s => s.AddStaffAsync(It.IsAny<OtherStaff>(), default))
                .ReturnsAsync(false);
            var model = CreatePageModel();
            var result = await model.OnPostAsync("John Doe", "1990-01-01", "1234567890", "Male",
                "123 Street", 50000, "Nurse", "BSc Nursing");
            Assert.IsType<PageResult>(result);
            Assert.Equal("There was an error adding the staff member. Please try again.", model.Message);
            Assert.False(model.IsSuccess);
        }

        [Fact]
        public async Task OnPostAsync_WithInvalidBirthDate_UsesCurrentDate()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            OtherStaff? capturedStaff = null;
            _mockAdminService.Setup(s => s.AddStaffAsync(It.IsAny<OtherStaff>(), default))
                .Callback<OtherStaff, System.Threading.CancellationToken>((staff, _) => capturedStaff = staff)
                .ReturnsAsync(true);
            var model = CreatePageModel();
            await model.OnPostAsync("John Doe", "invalid-date", "1234567890", "Male",
                "123 Street", 50000, "Nurse", "BSc Nursing");
            Assert.NotNull(capturedStaff);
            Assert.True(capturedStaff!.BirthDate <= DateTime.Now);
        }

        [Fact]
        public async Task OnPostAsync_WithValidBirthDate_ParsesCorrectly()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            OtherStaff? capturedStaff = null;
            _mockAdminService.Setup(s => s.AddStaffAsync(It.IsAny<OtherStaff>(), default))
                .Callback<OtherStaff, System.Threading.CancellationToken>((staff, _) => capturedStaff = staff)
                .ReturnsAsync(true);
            var model = CreatePageModel();
            await model.OnPostAsync("John Doe", "1990-05-15", "1234567890", "Male",
                "123 Street", 50000, "Nurse", "BSc Nursing");
            Assert.NotNull(capturedStaff);
            Assert.Equal(new DateTime(1990, 5, 15), capturedStaff!.BirthDate);
        }

        [Fact]
        public async Task OnPostAsync_SetsStaffPropertiesCorrectly()
        {
            SessionTestHelper.SetupSession(_mockSession, 1, 3);
            OtherStaff? capturedStaff = null;
            _mockAdminService.Setup(s => s.AddStaffAsync(It.IsAny<OtherStaff>(), default))
                .Callback<OtherStaff, System.Threading.CancellationToken>((staff, _) => capturedStaff = staff)
                .ReturnsAsync(true);
            var model = CreatePageModel();
            await model.OnPostAsync("Jane Smith", "1985-03-20", "9876543210", "Female",
                "456 Avenue", 60000, "Receptionist", "BA");
            Assert.NotNull(capturedStaff);
            Assert.Equal("Jane Smith", capturedStaff!.Name);
            Assert.Equal("9876543210", capturedStaff.Phone);
            Assert.Equal("Female", capturedStaff.Gender);
            Assert.Equal("456 Avenue", capturedStaff.Address);
            Assert.Equal(60000, capturedStaff.Salary);
            Assert.Equal("Receptionist", capturedStaff.Designation);
            Assert.Equal("BA", capturedStaff.Qualification);
            Assert.True(capturedStaff.IsActive);
        }
    }
}
