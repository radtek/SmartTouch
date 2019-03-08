using SmartTouch.CRM.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class LeadAdapterJobLogDetailsDb
    {
        [Key]
        public int LeadAdapterJobLogDetailID { get; set; }
        
        [ForeignKey("LeadAdapterJobLogs")]
        public virtual int LeadAdapterJobLogID { get; set; }
        public LeadAdapterJobLogsDb LeadAdapterJobLogs { get; set; }

        public string RowData { get; set; }
        public Guid? ReferenceId { get; set; }

        [ForeignKey("LeadAdapterRecordStatus")]
        public LeadAdapterRecordStatus LeadAdapterRecordStatusID { get; set; }        
        public LeadAdapterRecordStatusDb LeadAdapterRecordStatus { get; set; }

        public string Remarks { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string SubmittedData { get; set; }
    }
}
