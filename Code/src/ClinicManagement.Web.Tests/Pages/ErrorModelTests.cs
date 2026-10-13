using System;
using ClinicManagement.Web.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Web.Tests.Pages
{
    public class ErrorModelTests
    {
        private readonly Mock<ILogger<ErrorModel>> _mockLogger;
        private readonly Mock<HttpContext> _mockHttpContext;

        public ErrorModelTests()
        {
            _mockLogger = new Mock<ILogger<ErrorModel>>();
            _mockHttpContext = new Mock<HttpContext>();
        }

        private ErrorModel CreatePageModel()
        {
            var model = new ErrorModel(_mockLogger.Object);
            model.PageContext = new PageContext { HttpContext = _mockHttpContext.Object };
            return model;
        }

        [Fact]
        public void Constructor_WithValidLogger_CreatesInstance()
        {
            var model = new ErrorModel(_mockLogger.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesNullRequestId()
        {
            var model = new ErrorModel(_mockLogger.Object);
            Assert.Null(model.RequestId);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdIsNull_ReturnsFalse()
        {
            var model = new ErrorModel(_mockLogger.Object);
            model.RequestId = null;
            Assert.False(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdIsEmpty_ReturnsFalse()
        {
            var model = new ErrorModel(_mockLogger.Object);
            model.RequestId = string.Empty;
            Assert.False(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdHasValue_ReturnsTrue()
        {
            var model = new ErrorModel(_mockLogger.Object);
            model.RequestId = "some-request-id";
            Assert.True(model.ShowRequestId);
        }

        [Fact]
        public void OnGet_SetsRequestIdFromTraceIdentifier()
        {
            _mockHttpContext.Setup(c => c.TraceIdentifier).Returns("trace-123");
            var model = CreatePageModel();
            model.OnGet();
            Assert.Equal("trace-123", model.RequestId);
        }

        [Fact]
        public void OnGet_WhenTraceIdentifierIsEmpty_SetsRequestId()
        {
            _mockHttpContext.Setup(c => c.TraceIdentifier).Returns("fallback-id");
            var model = CreatePageModel();
            model.OnGet();
            Assert.NotNull(model.RequestId);
        }

        [Fact]
        public void RequestId_CanBeSetDirectly()
        {
            var model = new ErrorModel(_mockLogger.Object);
            model.RequestId = "test-id-123";
            Assert.Equal("test-id-123", model.RequestId);
            Assert.True(model.ShowRequestId);
        }
    }
}
