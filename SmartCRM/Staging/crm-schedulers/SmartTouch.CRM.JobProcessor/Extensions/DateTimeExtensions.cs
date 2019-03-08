using System;

namespace SmartTouch.CRM.JobProcessor.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToTimezone(this DateTime date, string timezoneId)
        {
            var timezoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            var utc = date.ToUniversalTime();
            return TimeZoneInfo.ConvertTimeFromUtc(utc, timezoneInfo);
        }

        public static DateTime ConvertToTimeZone(this DateTime date, string sourceTimezoneId, string destTimezoneId)
        {
            var sourceTimezone = TimeZoneInfo.FindSystemTimeZoneById(sourceTimezoneId);
            var destTimezone = TimeZoneInfo.FindSystemTimeZoneById(destTimezoneId);
            return TimeZoneInfo.ConvertTime(date, sourceTimezone, destTimezone);
        }

        public static DateTime SetKind(this DateTime date, DateTimeKind kind)
        {
            return DateTime.SpecifyKind(date, kind);
        }

        public static DateTime Yesterday(this DateTime date)
        {
            return date.Date.AddDays(-1);
        }

        public static DateTime NextMinute(this DateTime date)
        {
            return date.Date.AddMinutes(1);
        }

        public static string ToUniversalString(this DateTime date)
        {
            return $"{date:yyyy-MM-ddTHH:mm:ss.FFFZ}";
        }
    }
}
