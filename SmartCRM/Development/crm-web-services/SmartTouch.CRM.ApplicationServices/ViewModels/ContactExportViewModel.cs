using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ContactExportViewModel
    {
        public int AccountID { get; set; }
        public int ContactID { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public int ContactType { get; set; }
        public string PrimaryEmail { get; set; }
        public string LifecycleStage { get; set; }
        //public Phone Phones { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string WorkPhone { get; set; }
        public string PhoneType { get; set; }
        public string ContactImageUrl { get; set; }      
        public DateTime? LastContacted { get; set; }
        public string LastTouchedThrough { get; set; }
        public AddressViewModel Address { get; set; }
        public string LeadScore { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string DateFormat { get; set; }
    }
}
