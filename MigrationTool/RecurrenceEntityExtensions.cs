using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.DataTypes;
using MigrationTool.Persistency;

namespace MigrationTool;
public static class RecurrenceEntityExtensions
{
    public static string GetRecurrencePattern(this RecurrenceEntity entity)
    {
        var pattern = new RecurrencePattern
        {
            ByDay = entity.ByDay.Select(day => new WeekDay((DayOfWeek)day)).ToList(),
            ByHour = entity.ByHour,
            ByMinute = entity.ByMinute,
            BySecond = entity.BySecond,
            ByMonth = entity.ByMonth,
            ByMonthDay = entity.ByMonthDay,
            ByWeekNo = entity.ByWeekNo,
            ByYearDay = entity.ByYearDay,
            Count = entity.Count,
            BySetPosition = entity.BySetPosition,
            FirstDayOfWeek = entity.FirstDayOfWeek,
            Frequency = (FrequencyType)Enum.Parse(typeof(FrequencyType), entity.Frequency, true),
            Interval = entity.Interval,
            Until = entity.Until,
        };

        var serializer = new RecurrencePatternSerializer();
        return serializer.SerializeToString(pattern);
    }
}
