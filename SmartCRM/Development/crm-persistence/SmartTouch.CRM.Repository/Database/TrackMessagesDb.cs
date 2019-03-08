using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class TrackMessagesDb
    {
        [Key]
        public long TrackMessageID { get; set; }
        private Guid _message { get; set; }
        public Guid MessageID
        {
            get
            {
                if (_message == default(Guid))
                    _message = Guid.NewGuid();
                return _message;
            }
            set
            {
                _message = value;
            }
        }
        public byte LeadScoreConditionType { get; set; }
        public int EntityID { get; set; }
        public int LinkedEntityID { get; set; }
        public int ContactID { get; set; }
        public int UserID { get; set; }
        public int AccountID { get; set; }
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
                if(value == DateTime.MinValue)
                    _createdOn = DateTime.UtcNow;
                else
                    _createdOn = value;
            }
        }
        
        public TrackMessageProcessStatus MessageProcessStatusID { get;set;}
    }
}
