using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class LoginAccountConfiguration : IEntityTypeConfiguration<LoginAccount>
{
    public void Configure(EntityTypeBuilder<LoginAccount> builder)
    {
        builder.ToTable("logintable", "dbo");
        builder.HasKey(l => l.LoginId);
        builder.Property(l => l.LoginId).HasColumnName("loginid").UseIdentityColumn();
        builder.Property(l => l.Password).HasColumnName("password").HasMaxLength(20).IsRequired();
        builder.Property(l => l.Email).HasColumnName("email").HasMaxLength(30).IsRequired();
        builder.Property(l => l.Type).HasColumnName("type").IsRequired();
    }
}

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patient", "dbo");
        builder.HasKey(p => p.PatientId);
        builder.Property(p => p.PatientId).HasColumnName("patientid");
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(30).IsRequired();
        builder.Property(p => p.Phone).HasColumnName("phone").HasMaxLength(11);
        builder.Property(p => p.Address).HasColumnName("address").HasMaxLength(40);
        builder.Property(p => p.BirthDate).HasColumnName("birthdate").HasColumnType("date");
        builder.Property(p => p.Gender).HasColumnName("gender").HasMaxLength(1).IsRequired();
        builder.Ignore(p => p.Email);
        builder.Ignore(p => p.Password);
        builder.Ignore(p => p.IsActive);
    }
}

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("doctor", "dbo");
        builder.HasKey(d => d.DoctorId);
        builder.Property(d => d.DoctorId).HasColumnName("doctorid");
        builder.Property(d => d.Name).HasColumnName("name").HasMaxLength(30).IsRequired();
        builder.Property(d => d.Phone).HasColumnName("phone").HasMaxLength(11);
        builder.Property(d => d.Address).HasColumnName("address").HasMaxLength(40);
        builder.Property(d => d.BirthDate).HasColumnName("birthdate").HasColumnType("date");
        builder.Property(d => d.Gender).HasColumnName("gender").HasMaxLength(1).IsRequired();
        builder.Property(d => d.DeptNo).HasColumnName("deptno");
        builder.Property(d => d.ChargesPerVisit).HasColumnName("charges_per_visit");
        builder.Property(d => d.MonthlySalary).HasColumnName("monthlysalary");
        builder.Property(d => d.ReputeIndex).HasColumnName("reputeindex");
        builder.Property(d => d.PatientsTreated).HasColumnName("patients_treated");
        builder.Property(d => d.Qualification).HasColumnName("qualification").HasMaxLength(100).IsRequired();
        builder.Property(d => d.Specialization).HasColumnName("specialization").HasMaxLength(100);
        builder.Property(d => d.Experience).HasColumnName("work_experience");
        builder.Property(d => d.Status).HasColumnName("status");
        builder.Ignore(d => d.Email);
        builder.Ignore(d => d.Password);
        builder.Ignore(d => d.Salary);
        builder.HasOne(d => d.Department)
            .WithMany(dep => dep.Doctors)
            .HasForeignKey(d => d.DeptNo);
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("department", "dbo");
        builder.HasKey(d => d.DeptNo);
        builder.Property(d => d.DeptNo).HasColumnName("deptno");
        builder.Property(d => d.DeptName).HasColumnName("deptname").HasMaxLength(30).IsRequired();
        builder.Property(d => d.Description).HasColumnName("description").HasMaxLength(1000);
    }
}

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointment", "dbo");
        builder.HasKey(a => a.AppointmentId);
        builder.Property(a => a.AppointmentId).HasColumnName("appointid").UseIdentityColumn();
        builder.Property(a => a.DoctorId).HasColumnName("doctorid");
        builder.Property(a => a.PatientId).HasColumnName("patientid");
        builder.Property(a => a.AppointmentDate).HasColumnName("date").HasColumnType("timestamp without time zone");
        builder.Property(a => a.AppointmentStatus).HasColumnName("appointment_status");
        builder.Property(a => a.BillAmount).HasColumnName("bill_amount");
        builder.Property(a => a.BillStatus).HasColumnName("bill_status").HasMaxLength(10);
        builder.Property(a => a.DoctorNotification).HasColumnName("doctornotification");
        builder.Property(a => a.PatientNotification).HasColumnName("patientnotification");
        builder.Property(a => a.FeedbackStatus).HasColumnName("feedbackstatus");
        builder.Property(a => a.Disease).HasColumnName("disease").HasMaxLength(100);
        builder.Property(a => a.Progress).HasColumnName("progress").HasMaxLength(100);
        builder.Property(a => a.Prescription).HasColumnName("prescription").HasMaxLength(100);
        builder.Ignore(a => a.Timings);
        builder.Ignore(a => a.IsApproved);
        builder.Ignore(a => a.IsCompleted);
        builder.Ignore(a => a.IsPaid);
        builder.Ignore(a => a.FeedbackGiven);
        builder.HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OtherStaffConfiguration : IEntityTypeConfiguration<OtherStaff>
{
    public void Configure(EntityTypeBuilder<OtherStaff> builder)
    {
        builder.ToTable("otherstaff", "dbo");
        builder.HasKey(s => s.StaffId);
        builder.Property(s => s.StaffId).HasColumnName("staffid").UseIdentityColumn();
        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(30).IsRequired();
        builder.Property(s => s.Phone).HasColumnName("phone").HasMaxLength(11);
        builder.Property(s => s.Address).HasColumnName("address").HasMaxLength(30);
        builder.Property(s => s.Designation).HasColumnName("designation").HasMaxLength(15).IsRequired();
        builder.Property(s => s.Gender).HasColumnName("gender").HasMaxLength(1).IsRequired();
        builder.Property(s => s.BirthDate).HasColumnName("birthdate").HasColumnType("date");
        builder.Property(s => s.Qualification).HasColumnName("highest_qualification").HasMaxLength(50);
        builder.Property(s => s.Salary).HasColumnName("salary");
        builder.Ignore(s => s.IsActive);
    }
}
