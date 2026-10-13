# Clinic Management System - .NET 8 Migration

## Overview
This is a Clinic Management System migrated from ASP.NET Web Forms 4.5.2 to .NET 8 using clean architecture principles.

## Architecture
The solution follows Clean Architecture with four layers:

```
src/
├── ClinicManagement.Domain/          # Domain entities, interfaces, enums
├── ClinicManagement.Application/     # Business logic, services
├── ClinicManagement.Infrastructure/  # EF Core, repositories, data access
└── ClinicManagement.Web/             # Razor Pages UI layer

tests/
└── ClinicManagement.UnitTests/       # Unit tests
```

## Features
- **Patient Portal**: Registration, appointment booking, treatment history, bills
- **Doctor Portal**: Appointment management, prescription updates, billing
- **Admin Portal**: Doctor/staff management, clinic overview dashboard

## Technology Stack
- **.NET 8** with Razor Pages
- **Entity Framework Core 8.0** for data access
- **SQL Server** database
- **Serilog** for structured logging
- **Bootstrap 5** for UI
- **xUnit + Moq + FluentAssertions** for testing

## Setup Instructions

### Prerequisites
- .NET 8 SDK
- SQL Server (or SQL Server Express)

### Configuration
1. Update the connection string in `src/ClinicManagement.Web/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=.\\SQLEXPRESS;Initial Catalog=DBProject;Integrated Security=True;TrustServerCertificate=True"
  }
}
```

2. Run database migrations (if needed):
```bash
cd src/ClinicManagement.Web
dotnet ef database update
```

### Running the Application
```bash
cd src/ClinicManagement.Web
dotnet run
```

### Running Tests
```bash
dotnet test tests/ClinicManagement.UnitTests/
```

## Migration Notes
- Web Forms pages (.aspx) migrated to Razor Pages (.cshtml)
- ADO.NET replaced with Entity Framework Core 8
- Web.config replaced with appsettings.json
- Global.asax replaced with Program.cs
- Session state preserved using ASP.NET Core session middleware
- Master pages replaced with Razor layout pages
