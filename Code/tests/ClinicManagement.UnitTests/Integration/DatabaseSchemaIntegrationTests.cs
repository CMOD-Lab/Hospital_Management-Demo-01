using ClinicManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ClinicManagement.UnitTests.Integration;

/// <summary>
/// Optional live PostgreSQL checks. Set RUN_DB_INTEGRATION=1 and a valid connection string in appsettings or env.
/// </summary>
public class DatabaseSchemaIntegrationTests
{
    private static ClinicDbContext CreateContext()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_DB_INTEGRATION"), "1", StringComparison.OrdinalIgnoreCase))
            return null!;

        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "ClinicManagement.Web"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");

        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ClinicDbContext(options);
    }

    [Fact]
    public async Task Database_HasExpectedSeedData()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_DB_INTEGRATION"), "1", StringComparison.OrdinalIgnoreCase))
            return;

        await using var context = CreateContext();

        var deptCount = await context.Departments.CountAsync();
        Assert.Equal(5, deptCount);

        var admin = await context.LoginAccounts.FirstOrDefaultAsync(l => l.Email == "admin@clinic.com");
        Assert.NotNull(admin);
        Assert.Equal(3, admin!.Type);

        var patient = await context.Patients.FirstOrDefaultAsync(p => p.Name == "ABC");
        Assert.NotNull(patient);
    }
}
