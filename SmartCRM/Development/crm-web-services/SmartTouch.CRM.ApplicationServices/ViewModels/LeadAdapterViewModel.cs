using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class LeadAdapterViewModel
    {
        public int LeadAdapterAndAccountMapId { get; set; }
        public LeadAdapterTypes LeadAdapterType { get; set; }
        public string ArchivePath { get; set; }
        public string LocalFilePath { get; set; }
        public string LeadAdapterName { get; set; }
        public string BuilderNumber { get; set; }
        public Guid RequestGuid { get; set; }
        public bool Status { get; set; }
        public string Url { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int? Port { get; set; }
        public bool EnableSSL { get; set; }
        public int AccountID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
        public DateTime? LastProcessed { get; set; }
        public LeadAdapterErrorStatus? LeadAdapterErrorStatusID { get; set; }
        public string LeadAdapterErrorName { get; set; }
        public LeadAdapterServiceStatus? LeadAdapterServiceStatusID { get; set; }
        public string ServiceStatusMessage { get; set; }
        public short? LeadSourceType { get; set; }
        public IEnumerable<TagViewModel> TagsList { get; set; }
        public IEnumerable<DropdownValueViewModel> LeadSourceDropdownValues { get; set; }
        public string Name { get; set; }
        public string CommunityNumber { get; set; }

        //Facebook LeadAdapter Fields
        public string PageAccessToken { get; set; }
        public string UserAccessToken { get; set; }
        public string AddID { get; set; }
        public long PageID { get; set; }
        public int FacebookLeadAdapterID { get; set; }
        public string FacebookLeadAdapterName { get; set; }
        public int TotalCount { get; set; }
    }
}
