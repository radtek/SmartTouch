using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.MessageQueues;

namespace SmartTouch.CRM.MessageQueues
{
    public class Message : IDeepCloneable<Message>
    {
        public string MessageId { get; set; }
        public int EntityId { get; set; }
        public int UserId { get; set; }
        public int LeadScoreConditionType { get; set; }
        public int ContactId { get; set; }
        public int AccountId { get; set; }
        public int LinkedEntityId { get; set; }
        public Guid LockToken { get; set; }        
        public string ConditionValue { get; set; }
        public int TrackMessageId { get; set; }
        public int WorkflowId { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("MessageId:" + MessageId);
            builder.Append(", WorkflowId:" + WorkflowId);
            builder.Append(", EntityId:" + EntityId);
            builder.Append(", UserId:" + UserId);
            builder.Append(", ContactId:" + ContactId);
            builder.Append(", AccountId:" + AccountId);
            builder.Append(", LeadScoreConditionType:" + LeadScoreConditionType);
            builder.Append(", LinkedEntityId:" + LinkedEntityId);
            builder.Append(", ConditionValue:" + ConditionValue);
            builder.Append(", Lock-Token:" + LockToken.ToString());

            return builder.ToString();
        }

        public Message DeepClone()
        {
            return new Message()
            {
                AccountId = this.AccountId,
                ConditionValue = this.ConditionValue,
                EntityId = this.EntityId,
                LeadScoreConditionType = this.LeadScoreConditionType,
                ContactId = this.ContactId,
                LinkedEntityId = this.LinkedEntityId,
                MessageId = this.MessageId,
                UserId = this.UserId,
                WorkflowId = this.WorkflowId
            };
        }
    }

    public interface IDeepCloneable<T>
    {
        T DeepClone();
    }
}
