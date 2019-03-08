using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class TourByContactsViewModel
    {
        public int ContactId { get; set; }
        public string ContactName { get; set; }
        public string PRimaryEmail { get; set; }
        public string PrimaryPhoneNumber { get; set; }
        public string TourType { get; set; }
        public string TourDetails { get; set; }
        public DateTime TourDate { get; set; }
        public bool TourStatus { get; set; }
        public string Commnity { get; set; }
        public string LifeCycleStage { get; set; }
        //public string OpportunityStage { get; set; }
        public string assignedTo { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public int TotalCount { get; set; }
        public byte ContactType { get; set; }
    }
}
