using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using SmartTouch.CRM.Domain.Tags;

namespace SmartTouch.CRM.Domain.LeadAdapters
{
    public class LeadAdapterAndAccountMap : EntityBase<int>, IAggregateRoot
    {        
        public LeadAdapterTypes LeadAdapterTypeID { get; set; }
        public LeadAdapterCommunicationType LeadAdapterCommunicationTypeID { get; set; }
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public Guid RequestGuid { get; set; }
        public string BuilderNumber { get; set; }
        public string ArchivePath { get; set; }
        public string LocalFilePath { get; set; }
        public bool EnableSsl { get; set; }
        public string Url { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int? Port { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public ICollection<LeadAdapterJobLogs> LeadAdapterJobLogs { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
        public LeadAdapterErrorStatus? LeadAdapterErrorStatusID { get; set; }
        public string LeadAdapterErrorName { get; set; }
        public DateTime? LastProcessed { get; set; }
        public short? LeadSourceType { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public LeadAdapterServiceStatus? LeadAdapterServiceStatusID { get; set; }
        public string ServiceStatusMessage { get; set; }
        public string LeadAdapterType { get; set; }
        public string CommunityNumber { get; set; }

        public FacebookLeadAdapter FacebookLeadAdapter { get; set; }
        public string FacebookLeadAdapterName { get; set; }
        public int TotalCount { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
