using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.LeadScoreRules
{
    public class LeadScoreMessage
    {
        public Guid LeadScoreMessageID { get; set; }
        public int EntityID { get; set; }
        public int UserID { get; set; }
        public byte LeadScoreConditionType { get; set; }
        public int ContactID { get; set; }
        public int AccountID { get; set; }
        public int LinkedEntityID { get; set; }
        public string ConditionValue { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ProcessedOn { get; set; }
        public string Remarks { get; set; }
        public short LeadScoreProcessStatusID { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("EntityId:" + EntityID);
            builder.Append(", UserId:" + UserID);
            builder.Append(", ContactId:" + ContactID);
            builder.Append(", AccountId:" + AccountID);
            builder.Append(", LeadScoreConditionType:" + LeadScoreConditionType);
            builder.Append(", LinkedEntityId:" + LinkedEntityID);
            builder.Append(", ConditionValue:" + ConditionValue);
            return builder.ToString();
        }
    }
}
