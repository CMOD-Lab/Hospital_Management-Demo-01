using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        // PostgreSQL snake_case table name
        builder.ToTable("patient");
        builder.HasKey(p => p.PatientId);
        builder.Property(p => p.PatientId).HasColumnName("patient_id");
        builder.Property(p => p.Name).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Phone).HasMaxLength(15);
        builder.Property(p => p.Address).HasMaxLength(40);
        builder.Property(p => p.Gender).HasMaxLength(1);
        builder.Property(p => p.Email).HasMaxLength(30).IsRequired();
        builder.Property(p => p.Password).HasMaxLength(20).IsRequired();
        // PostgreSQL uses timestamptz for DateTime
        builder.Property(p => p.BirthDate).HasColumnType("timestamp with time zone");
    }
}

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        // PostgreSQL snake_case table name
        builder.ToTable("doctor");
        builder.HasKey(d => d.DoctorId);
        builder.Property(d => d.DoctorId).HasColumnName("doctor_id");
        builder.Property(d => d.Name).HasMaxLength(30).IsRequired();
        builder.Property(d => d.Email).HasMaxLength(30).IsRequired();
        builder.Property(d => d.Password).HasMaxLength(30).IsRequired();
        builder.Property(d => d.Gender).HasMaxLength(1);
        builder.Property(d => d.Address).HasMaxLength(30);
        builder.Property(d => d.Phone).HasMaxLength(30);
        builder.Property(d => d.Specialization).HasMaxLength(50);
        builder.Property(d => d.Qualification).HasMaxLength(100);
        // PostgreSQL uses real for float
        builder.Property(d => d.ReputeIndex).HasColumnType("real");
        // PostgreSQL uses timestamptz for DateTime
        builder.Property(d => d.BirthDate).HasColumnType("timestamp with time zone");
        builder.HasOne(d => d.Department)
               .WithMany(dep => dep.Doctors)
               .HasForeignKey(d => d.DeptNo);
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        // PostgreSQL snake_case table name
        builder.ToTable("department");
        builder.HasKey(d => d.DeptNo);
        builder.Property(d => d.DeptName).HasMaxLength(30).IsRequired();
    }
}

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        // PostgreSQL snake_case table name
        builder.ToTable("appointment");
        builder.HasKey(a => a.AppointmentId);
        builder.Property(a => a.AppointmentId).HasColumnName("appointment_id");
        builder.Property(a => a.Timings).HasMaxLength(30);
        builder.Property(a => a.Disease).HasMaxLength(30);
        builder.Property(a => a.Progress).HasMaxLength(50);
        builder.Property(a => a.Prescription).HasMaxLength(60);
        // PostgreSQL uses timestamptz for DateTime
        builder.Property(a => a.AppointmentDate).HasColumnType("timestamp with time zone");
        builder.HasOne(a => a.Doctor)
               .WithMany(d => d.Appointments)
               .HasForeignKey(a => a.DoctorId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Patient)
               .WithMany(p => p.Appointments)
               .HasForeignKey(a => a.PatientId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        // PostgreSQL snake_case table name
        builder.ToTable("bill");
        builder.HasKey(b => b.BillId);
        builder.Property(b => b.BillId).HasColumnName("bill_id");
        // PostgreSQL uses numeric for decimal
        builder.Property(b => b.Amount).HasColumnType("numeric(18,2)");
        // PostgreSQL uses timestamptz for DateTime
        builder.Property(b => b.BillDate).HasColumnType("timestamp with time zone");
        builder.HasOne(b => b.Patient)
               .WithMany(p => p.Bills)
               .HasForeignKey(b => b.PatientId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OtherStaffConfiguration : IEntityTypeConfiguration<OtherStaff>
{
    public void Configure(EntityTypeBuilder<OtherStaff> builder)
    {
        // PostgreSQL snake_case table name
        builder.ToTable("other_staff");
        builder.HasKey(s => s.StaffId);
        builder.Property(s => s.StaffId).HasColumnName("staff_id");
        builder.Property(s => s.Name).HasMaxLength(30).IsRequired();
        builder.Property(s => s.Phone).HasMaxLength(30);
        builder.Property(s => s.Gender).HasMaxLength(1);
        builder.Property(s => s.Address).HasMaxLength(50);
        builder.Property(s => s.Designation).HasMaxLength(30);
        builder.Property(s => s.Qualification).HasMaxLength(1);
    }
}
