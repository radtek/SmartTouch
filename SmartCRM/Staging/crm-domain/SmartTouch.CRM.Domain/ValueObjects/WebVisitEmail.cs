using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class WebVisitEmail
    {
        public IEnumerable<WebVisitEmail> WebVisits { get; set; }
    }

    public class WebVisitEmailItem
    {
        [DisplayName("Contact Id")]
        public int ContactId { get; set; }

        [DisplayName("Contact Name")]
        public string ContactName { get; set; }

        [DisplayName("Email")]
        public string Email { get; set; }

        [DisplayName("Phone")]
        public string Phone { get; set; }

        [DisplayName("Visit Time")]
        public DateTime VisitTime { get; set; }

        [DisplayName("Page Views")]
        public short PageViews { get; set; }

        [DisplayName("Duration(Seconds)")]
        public int Duration { get; set; }

        [DisplayName("Top Page #1")]
        public string TopPage1 { get; set; }

        [DisplayName("Top Page #2")]
        public string TopPage2 { get; set; }

        [DisplayName("Top Page #3")]
        public string TopPage3 { get; set; }

        [DisplayName("Source")]
        public string Source { get; set; }

        [DisplayName("Location")]
        public string Location { get; set; }

        [DisplayName("Owner Name")]
        public string OwnerName { get; set; }

        [DisplayName("Owner Id")]
        public int OwnerId { get; set; }

        [DisplayName("Visit Reference")]
        public string VisitReference { get; set; }

        [DisplayName("Account Admin")]
        public string AccountAdminEmail { get; set; }

        [DisplayName("Admin Name")]
        public string AdminAdminName { get; set; }

        [DisplayName("Account Name")]
        public string AccountName { get; set; }

        [DisplayName("Account ID")]
        public int AccountID { get; set; }
    }
}
