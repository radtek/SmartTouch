using Microsoft.SqlServer.Server;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

public static class UserDefinedFunctions
{
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlDateTime GetNextTime(byte timerType, int delayPeriod, byte delayUnit, byte runOn, SqlDateTime runAt, byte runType, SqlDateTime runOnDate
        , SqlDateTime startDate, SqlDateTime endDate, string runOnDay, string daysOfWeek, SqlDateTime previousActionTime)
    {
        var dateTime = GetNextTime_(timerType
            , delayPeriod
            , delayUnit
            , runOn
            , runAt.Value
            , runType
            , runOnDate.Value
            , startDate.Value
            , endDate.Value
            , runOnDay
            , daysOfWeek
            , previousActionTime.Value);

        return new SqlDateTime(dateTime);
    }

    [SqlFunction(
        DataAccess = DataAccessKind.Read,
        TableDefinition = "ContactId int",
        FillRowMethodName = "ContactUDF_FillRow")]
    public static IEnumerable GetContactsBySearchDefinition(SqlString url)
    {
        var contactsList = new string[0];

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync(new Uri(url.Value)).Result;

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;

                if (result.IndexOf(@"],""Exception"":null}") > -1) //success
                {
                    contactsList = result.Replace(@"],""Exception"":null}", string.Empty).Replace(@"{""ContactIds"":[", string.Empty).Split(',');
                }
                else
                {
                    var startIndex = result.IndexOf(@"],""Exception"":");
                    var exceptionMessage = result.Substring(startIndex + 14);
                    throw new ApplicationException(exceptionMessage);
                }
            }
            else
            {
                throw new ApplicationException("Error in consuming GetContactsBySearchDefinition service");
            }
        }
        var tableResult = new ArrayList();
        foreach (var item in contactsList)
        {
            var value = default(int);
            if (int.TryParse(item, out value)) { tableResult.Add(new ContactInfo { ContactId = int.Parse(item) }); }
        }

        return tableResult;
    }

    [SqlFunction(
        DataAccess = DataAccessKind.Read,
        TableDefinition = "ContactId int",
        FillRowMethodName = "ContactUDF_FillRow")]
    public static IEnumerable GetCampaignRecipientsById(SqlString url)
    {
        var contactsList = new string[0];

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync(new Uri(url.Value)).Result;

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;

                if (result.IndexOf(@"],""Exception"":null}") > -1) //success
                {
                    contactsList = result.Replace(@"],""Exception"":null}", string.Empty).Replace(@"{""ContactIds"":[", string.Empty).Split(',');
                }
                else
                {
                    var startIndex = result.IndexOf(@"],""Exception"":");
                    var exceptionMessage = result.Substring(startIndex + 14);
                    throw new ApplicationException(exceptionMessage);
                }
            }
            else
            {
                throw new ApplicationException("Error in consuming GetcampaignrecipientsbyId service");
            }
        }
        var tableResult = new ArrayList();
        foreach (var item in contactsList)
        {
            var value = default(int);
            if (int.TryParse(item, out value)) { tableResult.Add(new ContactInfo { ContactId = int.Parse(item) }); }
        }

        return tableResult;
    }

    [SqlFunction]
    public static SqlString InsertCampaignRecipients(SqlString url)
    {
        var rowData = string.Empty;
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync(new Uri(url.Value)).Result;

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;

                if (result.IndexOf(@"""Exception"":null}") > 0) //success
                {
                    rowData = "success";
                }
                else
                {
                    rowData = string.Empty;
                }
            }
            else
            {
                rowData = string.Empty;
            }
        }
        return rowData;
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString GetImportRowData(SqlString url)
    {
        var rowData = string.Empty;
        using (var client = new HttpClient())
        {       
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync(new Uri(url.Value)).Result;
            
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;

                if (result.IndexOf(@"""Exception"":null}") > 0) //success
                {
                    rowData = result.Replace(@"}"",""Exception"":null}", string.Empty).Replace(@"{""RowData"":""{", "{").Replace(@"\""", "\"");
                }
                else
                {
                    rowData = string.Empty;
                }
            }
            else
            {
                rowData = string.Empty;
            }
        }
        return rowData;
    }

   

    public static void ContactUDF_FillRow(object tableTypeObject, out SqlInt32 contactId)
    {
        var tableType = (ContactInfo)tableTypeObject;
        contactId = tableType.ContactId;
    }
    private static DateTime GetNextTime_(byte timerType, int delayPeriod, byte delayUnit, byte runOn, DateTime runAt, byte runType, DateTime runOnDate, DateTime startDate, DateTime endDate, string runOnDay, string daysOfWeek, DateTime previousActionTime)
    {
        DateTime date = previousActionTime;
        if (timerType == (byte)TimerType.TimeDelay)
        {
            switch ((DateInterval)delayUnit)
            {
                case DateInterval.Years:
                    date = date.AddYears(delayPeriod);
                    break;
                case DateInterval.Months:
                    date = date.AddMonths(delayPeriod);
                    break;
                case DateInterval.Weeks:
                    date = date.AddDays(delayPeriod * 7);
                    break;
                case DateInterval.Days:
                    date = date.AddDays(delayPeriod);
                    break;
                case DateInterval.Minutes:
                    date = date.AddMinutes(delayPeriod);
                    break;
                case DateInterval.Seconds:
                    date = date.AddSeconds(delayPeriod);
                    break;
                case DateInterval.Hours:
                default:
                    date = date.AddHours(delayPeriod);
                    break;
            }

            if (runOn == (byte)RunOn.Weekday)
                date = date.AddDays(DaysToWeekday(date.DayOfWeek));
            return RunAt(runAt, date);
        }
        else if (timerType == (byte)TimerType.Date)
        {
            if (runType == (byte)RunType.OnADate)
            {
                runOnDate = RunAt(runAt, runOnDate);
                if (runOnDate < date)
                    return date;
                else if (runOnDate > date)
                    return runOnDate;
            }
            else
            {
                if ((endDate < date) || (date > startDate && date < endDate) || date < startDate) return date;
            }
        }
        else if (timerType == (byte)TimerType.Week)
        {
            var runOnDays = new List<int>();

            if (!string.IsNullOrWhiteSpace(daysOfWeek))
            {
                var daysList = daysOfWeek.Split(',');
                foreach (var item in daysList)
                {
                    var value = default(int);
                    if (int.TryParse(item, out value)) 
                    runOnDays.Add(value);
                }

            }

            if (runOnDays.Any(d => d == (int)date.DayOfWeek))
                return date;
            else
            {
                IDictionary<DayOfWeek, int> days = new Dictionary<DayOfWeek, int>();
                foreach (DayOfWeek day in runOnDays)
                {
                    int daysUntilDayOfWeek = ((int)day - (int)date.DayOfWeek + 7) % 7;
                    days.Add(day, daysUntilDayOfWeek);
                }
                var minDays = days.Min(d => d.Value);
                date.AddDays(minDays);
                return date;
            }
        }
        return date;
    }
    private static int DaysToWeekday(DayOfWeek dayOfWeek)
    {
        if (dayOfWeek == DayOfWeek.Sunday)
            return 1;
        else if (dayOfWeek == DayOfWeek.Saturday)
            return 2;
        else
            return 0;
    }
    private static DateTime RunAt(DateTime runAt, DateTime nextTime)
    {
        if (runAt.TimeOfDay > nextTime.TimeOfDay)
            return nextTime.AddMilliseconds(runAt.Millisecond - nextTime.Millisecond);
        return nextTime;
    }
}

public class ContactInfo
{
    public SqlInt32 ContactId { get; set; }
}
public enum RunOn : byte
{
    AnyDay = 1,
    Weekday = 2
}
public enum RunType : byte
{
    OnADate = 1,
    BetweenDates = 2
}
public enum TimerType : byte
{
    TimeDelay = 1,
    Date = 2,
    Week = 3
}
public enum DateInterval : byte
{
    Years = 1,
    Months = 2,
    Weeks = 3,
    Days = 4,
    Hours = 5,
    Minutes = 6,
    Seconds = 7
}