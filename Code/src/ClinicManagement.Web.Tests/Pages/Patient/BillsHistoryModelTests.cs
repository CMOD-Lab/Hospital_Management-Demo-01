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
    public class BillsHistoryModelTests
    {
        private readonly Mock<IPatientService> _mockPatientService;
        private readonly Mock<ILogger<BillsHistoryModel>> _mockLogger;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public BillsHistoryModelTests()
        {
            _mockPatientService = new Mock<IPatientService>();
            _mockLogger = new Mock<ILogger<BillsHistoryModel>>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        private BillsHistoryModel CreatePageModel()
        {
            var model = new BillsHistoryModel(_mockPatientService.Object, _mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var model = new BillsHistoryModel(_mockPatientService.Object, _mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesEmptyBills()
        {
            var model = new BillsHistoryModel(_mockPatientService.Object, _mockLogger.Object);
            Assert.NotNull(model.Bills);
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
        public async Task OnGetAsync_WhenPatientLoggedIn_LoadsBillHistory()
        {
            SessionTestHelper.SetupSession(_mockSession, 10, 1);
            var bills = new List<Bill>
            {
                new Bill { BillId = 1, PatientId = 10, Amount = 500m, IsPaid = true }
            };
            _mockPatientService.Setup(s => s.GetBillHistoryAsync(10, default)).ReturnsAsync(bills);
            var model = CreatePageModel();
            var result = await model.OnGetAsync();
            Assert.IsType<PageResult>(result);
            _mockPatientService.Verify(s => s.GetBillHistoryAsync(10, default), Times.Once);
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
    }
}
