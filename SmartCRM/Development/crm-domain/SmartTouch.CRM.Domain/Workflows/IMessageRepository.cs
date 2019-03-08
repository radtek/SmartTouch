using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public interface IMessageRepository : IRepository<TrackMessage, long>
    {
        void SendMessages(IEnumerable<TrackMessage> trackMessages);
        IEnumerable<LeadScoreMessage> GetLeadScoreMessages();
        void UpdateLeadScoreMessage(IEnumerable<LeadScoreMessage> messages);
    }
}
