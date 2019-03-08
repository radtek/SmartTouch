using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;

namespace SmartTouch.CRM.Domain.LeadAdapters
{
    public class LeadAdapterJobLogs : EntityBase<int>, IAggregateRoot
    {        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public LeadAdapterJobStatus LeadAdapterJobStatusID { get; set; }
        public int LeadAdapterAndAccountMapID { get; set; }
        public string Remarks { get; set; }
        public string FileName { get; set; }
        public int CreatedBy { get; set; }
        public int OwnerID { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public IEnumerable<LeadAdapterJobLogDetails> LeadAdapterJobLogDetails { get; set; }
        public string StorageName { get; set; }
        public bool SuccessRecords { get; set; }
        public bool FailureRecords { get; set; }
        protected override void Validate()
        {
        }
    }
}
