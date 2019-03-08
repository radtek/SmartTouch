using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.ApplicationServices.Messaging.Messages;
namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface ILeadScoreService
    {
        InsertLeadScoreResponse InsertLeadScore(InsertLeadScoreRequest request);
        UpdateLeadScoreResponse UpdateLeadScore(UpdateLeadScoreRequest request);
        LeadScoreAuditCheckResponse IsScoreAudited(LeadScoreAuditCheckRequest request);
        AdjustLeadscoreResponse AdjustLeadscore(AdjustLeadscoreRequest request);
    }
}
