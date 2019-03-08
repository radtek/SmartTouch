using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ContactResultViewModel
    {
        public string Name { get; set; }   //Fname + LName
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string ContactType { get; set; }
        public string PrimaryEmail { get; set; }
        public string Title { get; set; }
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string GooglePlusUrl { get; set; }
        public string WebsiteUrl { get; set; }
        public string BlogUrl { get; set; }
        public AddressViewModel PrimaryAddress { get; set; }

        public short? PartnerTypeID { get; set; }
        public string PartnerType { get; set; }

        public short LifecycleStage { get; set; }      //E
        public string LifecycleName { get; set; }

        public int? OwnerId { get; set; }
        public string OwnerName { get; set; }

        public bool DoNotEmail { get; set; }
        public string LeadSources { get; set; }
        public int LeadScore { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? LastContacted { get; set; }
        public string LastTouchedThrough { get; set; }
    }
}
