using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SmartTouch.CRM.Domain.LeadScoreRules
{
    public interface ILeadScoreRepository : IRepository<LeadScore, int>
    {
        IEnumerable<LeadScoreRule> IsScoreAudited(int contactId, int accountId, int conditionId, string conditionValue, int entityId, IEnumerable<LeadScoreRule> rules);
        void AdjustLeadScore(int leadScore, int contactId, int accountId, int workflowActionId);
        int InsertLeadScores(IEnumerable<LeadScore> leadScores,int accountId);
    }
}
