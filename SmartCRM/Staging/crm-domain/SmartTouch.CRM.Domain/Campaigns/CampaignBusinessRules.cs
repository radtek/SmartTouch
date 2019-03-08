using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Campaigns
{
    public static class CampaignBusinessRules
    {
        public static readonly BusinessRule CampaignNameEmpty = new BusinessRule("[|Campaign name cannot be empty|]");
        public static readonly BusinessRule SubjectEmpty = new BusinessRule("[|Subject cannot be empty|]");
        public static readonly BusinessRule FromEmailEmpty = new BusinessRule("[|Sender(from) email cannot be empty|]");
        public static readonly BusinessRule HTMLContentEmpty = new BusinessRule("[|Campaign body cannot be empty|]");
        public static readonly BusinessRule ScheduleTimeInvalid = new BusinessRule("[|Schedule time is invalid|]");
        public static readonly BusinessRule CampaignTagsEmpty = new BusinessRule("[|Add alteast one tag to add recipients|]");
        
    }
}
