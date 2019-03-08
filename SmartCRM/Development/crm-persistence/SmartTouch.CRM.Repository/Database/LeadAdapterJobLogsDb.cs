using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class LeadAdapterJobLogsDb
    {
        [Key]
        public int LeadAdapterJobLogID { get; set; }
        [ForeignKey("LeadAdapter")]
        public virtual int LeadAdapterAndAccountMapID { get; set; }
        public LeadAdapterAndAccountMapDb LeadAdapter { get; set; }
        public IEnumerable<LeadAdapterJobLogDetailsDb> LeadAdapterJobLogDetails { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public LeadAdapterJobStatus LeadAdapterJobStatusID { get; set; }
        public string Remarks { get; set; }
        public string FileName { get; set; }
        public int CreatedBy { get; set; }
        public int OwnerID { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string StorageName { get; set; }
        public string ProcessedFileName { get; set; }
    }
}
