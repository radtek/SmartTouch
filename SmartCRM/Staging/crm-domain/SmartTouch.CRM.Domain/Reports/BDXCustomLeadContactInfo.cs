using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Reports
{
    public class BDXCustomLeadContactInfo
    {
        public int ContactID { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ContactCreated { get; set; }
        public string PrimaryEmail { get; set; }
        public string FileName { get; set; }
        public string LeadType { get; set; }
        public string CommunityName { get; set; }
        public string SubmittedData { get; set; }
        public int LeadAdapterJobLogDetailID { get; set; }
        public string LeadSource { get; set; }

        public string CommunityNumber { get; set; }
        public string MarketName { get; set; }
        public string PlanName { get; set; }
        public string PlanNumber { get; set; }
        public string Comments { get; set; }
        public string Phone { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string BuilderName { get; set; }
        public string BuilderNumber { get; set; }
        public string StateName { get; set; }

    }
}
