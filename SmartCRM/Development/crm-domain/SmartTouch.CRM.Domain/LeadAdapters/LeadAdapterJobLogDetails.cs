using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;

namespace SmartTouch.CRM.Domain.LeadAdapters
{
    public class LeadAdapterJobLogDetails : EntityBase<int>, IAggregateRoot
    {
        public string SubmittedData { get; set; }
        public string RowData { get; set; }
        public Guid? ReferenceId { get; set; }
        public byte LeadAdapterRecordStatusID { get; set; }
        public string Remarks { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int LeadAdapterJobLogID { get; set; }
        public string LeadAdapterRecordStatus { get; set; }
        
        protected override void Validate()
        {
        }
    }

    public class LeadAdapterData
    {
        public string LeadAdapterType { get; set; }
        public string SubmittedData { get; set; }
    }
}
