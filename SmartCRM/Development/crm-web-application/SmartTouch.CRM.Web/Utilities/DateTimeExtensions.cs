//using LandmarkIT.Enterprise.Utilities.Caching;
//using NodaTime;
//using NodaTime.TimeZones;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Web;

//namespace SmartTouch.CRM.Web.Utilities
//{
//    public static class DateTimeExtensions
//    {
//        /// <summary>
//        /// Converts given time to Utz DateTime with DateTime.Kind as Unspecified
//        /// </summary>
//        /// <param name="date"></param>
//        /// <returns></returns>
//        public static DateTime ToUserDateTime(this DateTime date)
//        {
//            var timeZoneInfo = DateTimeZoneProviders.Tzdb[ianaTimeZone];
//            date = (date.Kind == DateTimeKind.Utc) ? date : date.ToUniversalTime();
//            return Instant.FromDateTimeUtc(date)
//                .InZone(timeZoneInfo)
//                .ToDateTimeUnspecified();
//        }

//        /// <summary>
//        /// Dirty timezone fix - Converts date coming from javascript to Utz DateTime
//        /// on client side, user timezone is masked with browser timezone.
//        /// after serialization time will be converted to utc.
//        /// convert time to local time -> utc -> subtract utz offset -> convert to utz date
//        /// Kind property will be set to Unspecified.
//        /// </summary>
//        /// <param name="dateFromJS"></param>
//        /// <returns></returns>
//        public static DateTime GetCorrectUtzDateTime(this DateTime dateFromJS)
//        {
//            //actual utz time
//            var localTime = dateFromJS.ToLocalTime();
//            var utzOffset = GetUtzOffset();
//            var convertedDate = localTime.ToUtc();
//            return convertedDate
//                .Subtract(utzOffset)
//                .ToUserDateTime();
//        }

//        /// <summary>
//        /// Converts server date to Newtonsoft.Json Serialized Date
//        /// </summary>
//        /// <param name="d"></param>
//        /// <returns></returns>
//        public static DateTime ToJsonSerailizedDate(this DateTime d)
//        {
//            d = DateTime.SpecifyKind(d.ToUniversalTime(), DateTimeKind.Unspecified);
//            return d.ToUserDateTime();
//        }
//        /// <summary>
//        /// Converts the given time to UTZ -> Utc
//        /// </summary>
//        /// <param name="d"></param>
//        /// <returns></returns>
//        public static DateTime ToUserUtcDateTime(this DateTime d)
//        {
//            var utzUtc = d.GetCorrectUtzDateTime().ToUtc();
//            return utzUtc.Subtract(GetUtzOffset());
//        }
//        /// <summary>
//        /// Converts date to javascript user compatible date
//        /// Subtracts locale timezone offset and adds user timezone offset
//        /// </summary>
//        /// <param name="d"></param>
//        /// <returns></returns>
//        public static DateTime ToJSDate(this DateTime d)
//        {
//            var date = d.ToUtc();
//            var offset = GetUtzOffset();
//            return date.ToUserDateTime();//.Subtract(TimeZone.CurrentTimeZone.GetUtcOffset(d));//.AddHours(offset.TotalHours);
//        }
//        /// <summary>
//        /// Round up date to nearest x minutes.
//        /// Usage: date.RoundUp(TimeSpan.FromMinutes(x)) or date.RoundUp()
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <param name="d"></param>
//        /// <returns></returns>
//        public static DateTime RoundUp(this DateTime dt, TimeSpan d = default(TimeSpan))
//        {
//            if(d == default(TimeSpan))
//                d = TimeSpan.FromMinutes(15);
//            return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
//        }
//        #region Private
//        /// <summary>
//        /// Gets user's timezone
//        /// </summary>
//        private static string timezone
//        {
//            get
//            {
//                return HttpContext.Current.User.Identity.ToTimeZone();
//            }
//        }
//        private static string ianaTimeZone
//        {
//            get
//            {
//                return HttpContext.Current.User.Identity.ToIanaTimeZone(); 
//            }
//        }
//        /// <summary>
//        /// Changes the given time's Kind to UTC. 
//        /// </summary>
//        /// <param name="d"></param>
//        /// <returns></returns>
//        private static DateTime ToUtc(this DateTime d)
//        {
//            return DateTime.SpecifyKind(d, DateTimeKind.Utc);
//        }
//        /// <summary>
//        /// Gets the timezone name/code of user's timezone
//        /// </summary>
//        /// <param name="timezone"></param>
//        /// <returns></returns>
//        private static TimeSpan GetUtzOffset()
//        {
//            return TimeZoneInfo.FindSystemTimeZoneById(timezone).GetUtcOffset(DateTime.Now);
//        } 
//        #endregion

//    }
//}