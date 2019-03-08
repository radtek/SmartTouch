using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class SearchCampaignsRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
        public int[] CampaignIDs { get; set; }
        public byte ShowingFieldType { get; set; }
        public string SortField { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public ContactSortFieldType SortFieldType { get; set; }

        public DateTime? CustomStartDate{get;set;}
        public DateTime? CustomEndDate{get;set;}
        public byte? CampaignSentPeriod { get; set; }
        public int? ReportID { get; set; }
        public int? UserID { get; set; }
        public int[] UserIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }

    public class SearchCampaignsResponse : ServiceResponseBase
    {
        public long TotalHits { get; set; }
        public IEnumerable<CampaignViewModel> Campaigns { get; set; }
    }
}
