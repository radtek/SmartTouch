using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.MessageQueues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Messages
{
    public class GetMessagesRequest:ServiceRequestBase
    {
        public IEnumerable<LeadScoreConditionType> LeadScoreConditions { get; set; }
    }
    public class GetLeadScoreMessagesResponse : ServiceResponseBase
    {
        public IEnumerable<LeadScoreMessage> Messages { get; set; }
    }
    public class UpdateLeadScoreMessage : ServiceRequestBase
    {
        public IEnumerable<LeadScoreMessage> Messages { get; set; }
    }
    public class SendMessagesRequest :ServiceRequestBase
    {
        public IEnumerable<TrackMessage> Messages { get; set; }
        public TrackMessage Message { get; set; }
    }
}
