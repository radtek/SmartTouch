using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class UpdateContactViewRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public string FirstName { get; set; }
        public string LastName {get; set;}
        public string Title { get; set; }
        public Phone Phone { get; set; }
        public short LifecycleStage { get; set; }
        public short PreviousLifecycleStage { get; set; }
        public short ContactLeadSourceId { get; set; }
        public Email Email { get; set; }
        public AddressViewModel Address { get; set; }
        public ImageViewModel Image { get; set; }
        public string CompanyName { get; set; }
        public PersonViewModel PersonViewModel { get; set; }
        public CompanyViewModel CompanyViewModel { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public int? LastUpdatedBy { get; set; }
        public bool isStAdmin { get; set; }
    }

    public class UpdateContactViewResponse  : ServiceResponseBase
    {
        public PersonViewModel person { get; set; }
        public CompanyViewModel company { get; set; }
    }
}
