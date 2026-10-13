using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ClinicManagement.Web.Tests.Helpers
{
    /// <summary>
    /// Helper class for setting up mock sessions in tests.
    /// ASP.NET Core's ISession.GetInt32 stores integers as 4 bytes in big-endian format.
    /// </summary>
    public static class SessionTestHelper
    {
        /// <summary>
        /// Encodes an integer value as 4 bytes in big-endian format (as ASP.NET Core does).
        /// </summary>
        public static byte[] EncodeInt32(int value)
        {
            return new byte[]
            {
                (byte)(value >> 24),
                (byte)(0xFF & (value >> 16)),
                (byte)(0xFF & (value >> 8)),
                (byte)(0xFF & value)
            };
        }

        /// <summary>
        /// Sets up a mock session with UserId and UserType values.
        /// </summary>
        public static void SetupSession(Mock<ISession> mockSession, int? userId, int? userType)
        {
            if (userId.HasValue)
            {
                var userIdBytes = EncodeInt32(userId.Value);
                mockSession.Setup(s => s.TryGetValue("UserId", out userIdBytes!))
                    .Returns(true);
            }
            else
            {
                byte[]? nullBytes = null;
                mockSession.Setup(s => s.TryGetValue("UserId", out nullBytes!))
                    .Returns(false);
            }

            if (userType.HasValue)
            {
                var userTypeBytes = EncodeInt32(userType.Value);
                mockSession.Setup(s => s.TryGetValue("UserType", out userTypeBytes!))
                    .Returns(true);
            }
            else
            {
                byte[]? nullBytes = null;
                mockSession.Setup(s => s.TryGetValue("UserType", out nullBytes!))
                    .Returns(false);
            }
        }
    }
}
