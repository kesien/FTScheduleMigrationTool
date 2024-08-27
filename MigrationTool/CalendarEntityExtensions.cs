using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.DataTypes;
using MigrationTool.Persistency;

namespace MigrationTool;
public static class CalendarEntityExtensions
{
    public static string ToSerialized(this CalendarEntity calendar)
    {
        var serializer = new CalendarSerializer();
        return serializer.SerializeToString(calendar.ToIcalendar());
    }

    public static Calendar ToIcalendar(this CalendarEntity calendar)
    {
        var ical = new Calendar();
        calendar.Events.ForEach(e =>
        {
            var calendarEvent = new CalendarEvent
            {
                Uid = e.Uid.ToString(),
                DtStart = new CalDateTime(e.DtStart.UtcDateTime, e.TimezoneId),
                DtEnd = new CalDateTime(e.DtEnd.UtcDateTime, e.TimezoneId),
                Description = e.Description,
                Location = e.Location,
                IsAllDay = e.IsAllDay,
                Summary = e.Summary,
                Properties = { new CalendarProperty("TODOS", e.Todos), new CalendarProperty("SEATID", e.SeatId == Guid.Empty ? "" : e.SeatId.ToString()), new CalendarProperty("LOCATIONID", e.LocationId.ToString()), new CalendarProperty("EVENTTYPE", (int)e.EventType), new CalendarProperty("PHONENUMBER", e.PhoneNumber ?? "") }
            };
            if (e.RecurrenceId is not null)
            {
                calendarEvent.RecurrenceId = new CalDateTime(e.RecurrenceId.Value.UtcDateTime, e.TimezoneId);
            }

            e.Exceptions.ForEach(ex =>
            {
                calendarEvent.ExceptionDates.Add(new PeriodList { new Period(new CalDateTime(ex.Date) { HasTime = true }) });
            });

            e.Recurrences.ForEach(r =>
            {
                calendarEvent.RecurrenceRules.Add(GetRecurrencePattern(r.RecurrencePattern));
            });

            ical.Events.Add(calendarEvent);
        });

        return ical;
    }

    private static RecurrencePattern GetRecurrencePattern(string pattern)
    {
        var serializer = new RecurrencePatternSerializer();
        return (RecurrencePattern)serializer.Deserialize(new StringReader(pattern));
    }
}
