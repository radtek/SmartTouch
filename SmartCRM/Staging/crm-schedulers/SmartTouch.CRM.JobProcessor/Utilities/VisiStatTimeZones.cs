using System;
using System.Collections.Generic;

namespace SmartTouch.CRM.JobProcessor.Utilities
{
    public static class VisiStatTimeZones
    {
        // Data compiled from the following sources.
        // Reference: http://www.xiirus.net/articles/article-_net-convert-datetime-from-one-timezone-to-another-7e44y.aspx
        // Reference: http://www.visistat.com/admin/partners/api-doc.php

        public static IList<VisiStatTimeZone> TimeZones()
        {
            IList<VisiStatTimeZone> timeZones = new List<VisiStatTimeZone>();
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 1, GlobalTimeZoneId = "Dateline Standard Time", TimeSpan = new TimeSpan(-12, 0, 0), Location = "International Date Line West " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 2, GlobalTimeZoneId = "Samoa Standard Time", TimeSpan = new TimeSpan(-11, 0, 0), Location = "Midway Island Samoa " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 3, GlobalTimeZoneId = "Hawaiian Standard Time", TimeSpan = new TimeSpan(-10, 0, 0), Location = "Hawaii " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 4, GlobalTimeZoneId = "Alaskan Standard Time", TimeSpan = new TimeSpan(-9, 0, 0), Location = "Alaska " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 5, GlobalTimeZoneId = "Pacific Standard Time", TimeSpan = new TimeSpan(-8, 0, 0), Location = "PACIFIC TIME US & Canada" });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 6, GlobalTimeZoneId = "US Mountain Standard Time", TimeSpan = new TimeSpan(-7, 0, 0), Location = "Arizona " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 7, GlobalTimeZoneId = "Mountain Standard Time (Mexico)", TimeSpan = new TimeSpan(-7, 0, 0), Location = "Chihuahua, La Paz, Mazatlan " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 8, GlobalTimeZoneId = "Mountain Standard Time", TimeSpan = new TimeSpan(-7, 0, 0), Location = "MOUNTAIN TIME US & Canada" });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 9, GlobalTimeZoneId = "Central America Standard Time", TimeSpan = new TimeSpan(-6, 0, 0), Location = "Central America " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 10, GlobalTimeZoneId = "Central Standard Time", TimeSpan = new TimeSpan(-6, 0, 0), Location = "CENTRAL TIME US & Canada" });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 11, GlobalTimeZoneId = "Central Standard Time (Mexico)", TimeSpan = new TimeSpan(-6, 0, 0), Location = "Guadalajara, Mexico City, Monterrey " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 12, GlobalTimeZoneId = "Canada Central Standard Time", TimeSpan = new TimeSpan(-6, 0, 0), Location = "Saskatchewan " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 13, GlobalTimeZoneId = "SA Pacific Standard Time", TimeSpan = new TimeSpan(-5, 0, 0), Location = "Bogota, Lime, Quito " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 14, GlobalTimeZoneId = "Eastern Standard Time", TimeSpan = new TimeSpan(-5, 0, 0), Location = "EASTERN TIME US & Canada" });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 15, GlobalTimeZoneId = "US Eastern Standard Time", TimeSpan = new TimeSpan(-5, 0, 0), Location = "Indiana East" });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 16, GlobalTimeZoneId = "Atlantic Standard Time", TimeSpan = new TimeSpan(-4, 0, 0), Location = "Atlantic Time Canada" });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 17, GlobalTimeZoneId = "Venezuela Standard Time", TimeSpan = new TimeSpan(-4, 0, 0), Location = "Caracas, La Paz " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 18, GlobalTimeZoneId = "Pacific SA Standard Time", TimeSpan = new TimeSpan(-4, 0, 0), Location = "Santiago " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 19, GlobalTimeZoneId = "Newfoundland Standard Time", TimeSpan = new TimeSpan(-3, -30, 0), Location = "Newfoundland " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 20, GlobalTimeZoneId = "E. South America Standard Time", TimeSpan = new TimeSpan(-3, 0, 0), Location = "Brasilia " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 21, GlobalTimeZoneId = "Argentina Standard Time", TimeSpan = new TimeSpan(-3, 0, 0), Location = "Buenos Aires, Georgetown " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 22, GlobalTimeZoneId = "Greenland Standard Time", TimeSpan = new TimeSpan(-3, 0, 0), Location = "Greenland " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 23, GlobalTimeZoneId = "Mid-Atlantic Standard Time", TimeSpan = new TimeSpan(-2, 0, 0), Location = "Mid-Atlantic " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 24, GlobalTimeZoneId = "Azores Standard Time", TimeSpan = new TimeSpan(-1, 0, 0), Location = "Azores " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 25, GlobalTimeZoneId = "Cape Verde Standard Time", TimeSpan = new TimeSpan(-1, 0, 0), Location = "Cape Verde Island " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 26, GlobalTimeZoneId = "Morocco Standard Time", TimeSpan = new TimeSpan(0, 0, 0), Location = "Casablanca, Monrovia " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 27, GlobalTimeZoneId = "GMT Standard Time", TimeSpan = new TimeSpan(0, 0, 0), Location = "Greenwich Mean Time: Dublin, Edinburgh, Lisbon, London " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 28, GlobalTimeZoneId = "W. Europe Standard Time", TimeSpan = new TimeSpan(1, 0, 0), Location = "Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 29, GlobalTimeZoneId = "Central Europe Standard Time", TimeSpan = new TimeSpan(1, 0, 0), Location = "Belgrade, Bratislava, Budapest, Ljubljana, Prague " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 30, GlobalTimeZoneId = "Romance Standard Time", TimeSpan = new TimeSpan(1, 0, 0), Location = "Brussels, Copenhagen, Madrid, Paris " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 31, GlobalTimeZoneId = "Central European Standard Time", TimeSpan = new TimeSpan(1, 0, 0), Location = "Sarajevo, Skopje, Warsaw, Zagreb " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 32, GlobalTimeZoneId = "W. Central Africa Standard Time", TimeSpan = new TimeSpan(1, 0, 0), Location = "West Central Africa " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 33, GlobalTimeZoneId = "GTB Standard Time", TimeSpan = new TimeSpan(2, 0, 0), Location = "Athens, Istanbul, Minsk " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 34, GlobalTimeZoneId = "Middle East Standard Time", TimeSpan = new TimeSpan(2, 0, 0), Location = "Bucharest " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 35, GlobalTimeZoneId = "Egypt Standard Time", TimeSpan = new TimeSpan(2, 0, 0), Location = "Cairo " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 36, GlobalTimeZoneId = "South Africa Standard Time", TimeSpan = new TimeSpan(2, 0, 0), Location = "Harare, Pretoria " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 37, GlobalTimeZoneId = "FLE Standard Time", TimeSpan = new TimeSpan(2, 0, 0), Location = "Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 38, GlobalTimeZoneId = "Israel Standard Time", TimeSpan = new TimeSpan(2, 0, 0), Location = "Jerusalem" });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 39, GlobalTimeZoneId = "Arabic Standard Time", TimeSpan = new TimeSpan(3, 0, 0), Location = "Baghdad " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 40, GlobalTimeZoneId = "Arab Standard Time", TimeSpan = new TimeSpan(3, 0, 0), Location = "Kuwait, Riyadh " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 41, GlobalTimeZoneId = "Russian Standard Time", TimeSpan = new TimeSpan(3, 0, 0), Location = "Moscow, St. Petersburg, Volgograd " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 42, GlobalTimeZoneId = "E. Africa Standard Time", TimeSpan = new TimeSpan(3, 0, 0), Location = "Nairobi " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 43, GlobalTimeZoneId = "Iran Standard Time", TimeSpan = new TimeSpan(3, 30, 0), Location = "Tehran " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 44, GlobalTimeZoneId = "Arabian Standard Time", TimeSpan = new TimeSpan(4, 0, 0), Location = "Abu Dhabi, Muscat " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 45, GlobalTimeZoneId = "Azerbaijan Standard Time", TimeSpan = new TimeSpan(4, 0, 0), Location = "Baku, Tbilisi, Yerevan " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 46, GlobalTimeZoneId = "Afghanistan Standard Time", TimeSpan = new TimeSpan(4, 30, 0), Location = "Kabul " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 47, GlobalTimeZoneId = "Ekaterinburg Standard Time", TimeSpan = new TimeSpan(5, 0, 0), Location = "Ekaterinburg " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 48, GlobalTimeZoneId = "Pakistan Standard Time", TimeSpan = new TimeSpan(5, 0, 0), Location = "Islamabad, Karachi, Tashkent " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 49, GlobalTimeZoneId = "India Standard Time", TimeSpan = new TimeSpan(5, 30, 0), Location = "Chennai, Kolkata, Mumbai, New Delhi " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 50, GlobalTimeZoneId = "Nepal Standard Time", TimeSpan = new TimeSpan(5, 45, 0), Location = "Kathmandu " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 51, GlobalTimeZoneId = "N. Central Asia Standard Time", TimeSpan = new TimeSpan(6, 0, 0), Location = "Almaty, Novosibirsk " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 52, GlobalTimeZoneId = "Central Asia Standard Time", TimeSpan = new TimeSpan(6, 0, 0), Location = "Astana, Dhaka " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 53, GlobalTimeZoneId = "Sri Lanka Standard Time", TimeSpan = new TimeSpan(6, 0, 0), Location = "Sri Jayawardenepura " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 54, GlobalTimeZoneId = "Myanmar Standard Time", TimeSpan = new TimeSpan(6, 30, 0), Location = "Rangoon " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 55, GlobalTimeZoneId = "SE Asia Standard Time", TimeSpan = new TimeSpan(7, 0, 0), Location = "Bangkok, Hanoi, Jakarta " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 56, GlobalTimeZoneId = "North Asia Standard Time", TimeSpan = new TimeSpan(7, 0, 0), Location = "Krasnoyarsk " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 57, GlobalTimeZoneId = "China Standard Time", TimeSpan = new TimeSpan(8, 0, 0), Location = "Beijing, Chongging, Hong Kong, Urumgi " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 58, GlobalTimeZoneId = "North Asia East Standard Time", TimeSpan = new TimeSpan(8, 0, 0), Location = "Irkutsk, Ulaan Bataar " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 59, GlobalTimeZoneId = "Singapore Standard Time", TimeSpan = new TimeSpan(8, 0, 0), Location = "Kuala Lumpur, Singapore " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 60, GlobalTimeZoneId = "W. Australia Standard Time", TimeSpan = new TimeSpan(8, 0, 0), Location = "Perth " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 61, GlobalTimeZoneId = "Taipei Standard Time", TimeSpan = new TimeSpan(8, 0, 0), Location = "Taipei " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 62, GlobalTimeZoneId = "Tokyo Standard Time", TimeSpan = new TimeSpan(9, 0, 0), Location = "Osaka, Sapporo, Tokyo " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 63, GlobalTimeZoneId = "Korea Standard Time", TimeSpan = new TimeSpan(9, 0, 0), Location = "Seoul " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 64, GlobalTimeZoneId = "Yakutsk Standard Time", TimeSpan = new TimeSpan(9, 0, 0), Location = "Yakutsk " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 65, GlobalTimeZoneId = "Cen. Australia Standard Time", TimeSpan = new TimeSpan(9, 30, 0), Location = "Adelaide " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 66, GlobalTimeZoneId = "AUS Central Standard Time", TimeSpan = new TimeSpan(9, 30, 0), Location = "Darwin " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 67, GlobalTimeZoneId = "E. Australia Standard Time", TimeSpan = new TimeSpan(10, 0, 0), Location = "Brisbane " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 68, GlobalTimeZoneId = "AUS Eastern Standard Time", TimeSpan = new TimeSpan(10, 0, 0), Location = "Canberra, Melbourne, Sydney " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 69, GlobalTimeZoneId = "West Pacific Standard Time", TimeSpan = new TimeSpan(10, 0, 0), Location = "Guam, Port Moresby " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 70, GlobalTimeZoneId = "Tasmania Standard Time", TimeSpan = new TimeSpan(10, 0, 0), Location = "Hobart " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 71, GlobalTimeZoneId = "Vladivostok Standard Time", TimeSpan = new TimeSpan(10, 0, 0), Location = "Vladivostok " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 72, GlobalTimeZoneId = "Central Pacific Standard Time", TimeSpan = new TimeSpan(11, 0, 0), Location = "Magadan, Solomon Islands, New Caledonia " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 73, GlobalTimeZoneId = "New Zealand Standard Time", TimeSpan = new TimeSpan(12, 0, 0), Location = "Auckland, Wellington " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 74, GlobalTimeZoneId = "Fiji Standard Time", TimeSpan = new TimeSpan(12, 0, 0), Location = "Figi, Kamchatka, Marshall Islands " });
            timeZones.Add(new VisiStatTimeZone() { TimeZoneId = 75, GlobalTimeZoneId = "Tonga Standard Time", TimeSpan = new TimeSpan(13, 0, 0), Location = "Nuku'alofa" });

            return timeZones;
        }
    }

    public class VisiStatTimeZone
    {
        public byte TimeZoneId { get; set; }
        public string GlobalTimeZoneId { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public string Location { get; set; }
    }
}
