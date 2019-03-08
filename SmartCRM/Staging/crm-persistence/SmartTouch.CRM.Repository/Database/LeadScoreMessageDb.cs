using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class LeadScoreMessageDb
    {
        [Key]
        public Guid LeadScoreMessageID { get; set; }
        public int EntityID { get; set; }
        public int UserID { get; set; }
        public byte LeadScoreConditionType { get; set; }
        public int ContactID { get; set; }
        public int AccountID { get; set; }
        public int LinkedEntityID { get; set; }
        public string ConditionValue { get; set; }
        private DateTime _createdOn { get; set; }
        public DateTime CreatedOn
        {
            get
            {
                return _createdOn;
            }
            set
            {
                if (value == DateTime.MinValue)
                    _createdOn = DateTime.UtcNow;
                else
                    _createdOn = value;
            }
        }

        private DateTime? _processedOn { get; set; }
        public DateTime? ProcessedOn
        {
            get
            {
                return _processedOn;
            }
            set
            {
                if (value == DateTime.MinValue)
                    _processedOn = null;
                else
                    _processedOn = value;
            }
        }
        public string Remarks { get; set; }
        public short LeadScoreProcessStatusID { get; set; }
    }
}
