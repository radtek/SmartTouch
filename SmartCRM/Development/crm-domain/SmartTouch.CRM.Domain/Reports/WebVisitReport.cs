using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Reports
{
    public class WebVisitReport
    {
        [DisplayName("Contact Id")]
        public int ContactId { get; set; }

        [DisplayName("Account Id")]
        public int AccountId { get; set; }

        [DisplayName("Account Name")]
        public string AccountName { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [DisplayName("Phone")]
        public string Phone { get; set; }

        [DisplayName("Zip")]
        public string Zip { get; set; }
        
        [DisplayName("Lifecycle Stage Id")]
        public short LifecycleStageId { get; set; }

        [DisplayName("Lifecycle Stage")]
        public string LifecycleStage{ get; set; }

        [DisplayName("Email")]
        public string Email { get; set; }

        [DisplayName("Visit Reference")]
        public string VisitReference { get; set; }

        [DisplayName("Visited On")]
        public DateTime VisitedOn { get; set; }

        [DisplayName("Visited On")]
        public DateTime VisitedOnUTZ { get; set; }

        [DisplayName("Page Views")]
        public short PageViews { get; set; }

        [DisplayName("Duration (Seconds)")]
        public int Duration { get; set; }

        [DisplayName("Top Page #1")]
        public string Page1 { get; set; }

        [DisplayName("Top Page #2")]
        public string Page2 { get; set; }

        [DisplayName("Top Page #3")]
        public string Page3 { get; set; }

        [DisplayName("Source")]
        public string Source { get; set; }

        [DisplayName("Location")]
        public string Location { get; set; }

        [DisplayName("Owner ID")]
        public int? OwnerID { get; set; }

        [DisplayName("Contact WebVisit ID")]
        public int ContactWebVisitID { get; set; }

        [DisplayName("Lead Source")]
        public String LeadSource { get; set; }

        [DisplayName("Lead Score")]
        public int LeadScore { get; set; }
    }
}
