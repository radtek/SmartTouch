using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.MessageQueues;

namespace SmartTouch.CRM.Automation.Core
{
    public static class Triggrtilities
    {
        /// <summary>
        /// Determines whether [is match trigger] [the specified message].
        /// </summary>
        /// <param name="triggers">The triggers.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static bool IsMatchTrigger(this IDictionary<LeadScoreConditionType, WorkflowTrigger> triggers, Message message)
        {
            if (message.LeadScoreConditionType == (int)LeadScoreConditionType.ContactVisitsWebPage)
            {
                // Webvisit trigger is only single value
                var trigger = triggers.Values.FirstOrDefault();
                if (trigger != null && trigger.Operator.HasValue)
                {
                    WebPageDurationOperator qualifier = trigger.Operator.Value;
                    if (qualifier == WebPageDurationOperator.LessThan)
                    {
                        if (trigger.IsAnyWebPage)
                            return triggers.Any(a => message.LinkedEntityId < a.Value.Duration && a.Key == LeadScoreConditionType.ContactVisitsWebPage);
                        else
                            return triggers.Any(a => a.Value.WebPage == message.ConditionValue && message.LinkedEntityId < a.Value.Duration && a.Key == LeadScoreConditionType.ContactVisitsWebPage);
                    }
                    else if (qualifier == WebPageDurationOperator.IsEqualTo)
                    {
                        if (trigger.IsAnyWebPage)
                            return triggers.Any(a => message.LinkedEntityId == a.Value.Duration && a.Key == LeadScoreConditionType.ContactVisitsWebPage);
                        else
                            return triggers.Any(a => a.Value.WebPage == message.ConditionValue && message.LinkedEntityId == a.Value.Duration && a.Key == LeadScoreConditionType.ContactVisitsWebPage);
                    }
                    else if (qualifier == WebPageDurationOperator.GreaterThan)
                    {
                        if (trigger.IsAnyWebPage)
                            return triggers.Any(a => message.LinkedEntityId > a.Value.Duration && a.Key == LeadScoreConditionType.ContactVisitsWebPage);
                        else
                            return triggers.Any(a => a.Value.WebPage == message.ConditionValue && message.LinkedEntityId > a.Value.Duration && a.Key == LeadScoreConditionType.ContactVisitsWebPage);
                    }
                    else return false;
                }
                else return false;
            }
            else if (message.LeadScoreConditionType == (int)LeadScoreConditionType.ContactTagAdded)
                return triggers.Any(a => a.Value.TagID == message.EntityId && a.Key == LeadScoreConditionType.ContactTagAdded);
            else if (message.LeadScoreConditionType == (int)LeadScoreConditionType.ContactMatchesSavedSearch)
                return triggers.Any(a => a.Value.SearchDefinitionID == message.EntityId && a.Key == LeadScoreConditionType.ContactMatchesSavedSearch);
            else if (message.LeadScoreConditionType == (int)LeadScoreConditionType.ContactSubmitsForm)
                return triggers.Any(a => a.Value.FormID == message.EntityId && a.Key == LeadScoreConditionType.ContactSubmitsForm);
            else if (message.LeadScoreConditionType == (int)LeadScoreConditionType.ContactLifecycleChange)
                return triggers.Any(a => a.Value.LifecycleDropdownValueID == message.EntityId && a.Key == LeadScoreConditionType.ContactLifecycleChange);
            else if (message.LeadScoreConditionType == (int)LeadScoreConditionType.CampaignSent)
                return triggers.Any(a => a.Value.CampaignID == message.EntityId && a.Key == LeadScoreConditionType.CampaignSent);
            else if (message.LeadScoreConditionType == (int)LeadScoreConditionType.OpportunityStatusChanged)
                return triggers.Any(a => a.Value.OpportunityStageID == message.EntityId && a.Key == LeadScoreConditionType.OpportunityStatusChanged);
            else if (message.LeadScoreConditionType == (int)LeadScoreConditionType.LeadAdapterSubmitted)
                return triggers.Any(a => a.Value.LeadAdapterID == message.EntityId && a.Key == LeadScoreConditionType.LeadAdapterSubmitted);
            else if (message.LeadScoreConditionType == (int)LeadScoreConditionType.ContactClicksLink)
                return triggers.Any(a => a.Value.SelectedLinks.Contains(message.LinkedEntityId) && a.Key == LeadScoreConditionType.ContactClicksLink);
            else if (message.LeadScoreConditionType == (int)LeadScoreConditionType.LeadscoreReached)
                return triggers.Any(a => a.Value.LeadScore <= message.EntityId && a.Key == LeadScoreConditionType.LeadscoreReached);

            else
                return default(bool);
        }
    }
}
