using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Infrastructure.Repositories;

internal static class AppointmentFormatting
{
    internal static void ApplyTimings(IEnumerable<Appointment> appointments)
    {
        foreach (var appointment in appointments)
            appointment.Timings = FormatSlot(appointment.AppointmentDate);
    }

    internal static void ApplyTimings(Appointment? appointment)
    {
        if (appointment != null)
            appointment.Timings = FormatSlot(appointment.AppointmentDate);
    }

    internal static string FormatSlot(DateTime date)
    {
        var hour = date.Hour;
        if (hour == 0 && date.Minute == 0 && date.Second == 0)
            return date.ToString("yyyy-MM-dd HH:mm:ss");

        return hour switch
        {
            < 12 => $"{hour}:00 AM",
            12 => "12:00 PM",
            _ => $"{hour - 12}:00 PM"
        };
    }

    internal static string FormatFreeSlotLabel(int hour)
    {
        return hour switch
        {
            < 12 => $"{hour}:00 AM",
            12 => "12:00 PM",
            _ => $"{hour - 12}:00 PM"
        };
    }
}
