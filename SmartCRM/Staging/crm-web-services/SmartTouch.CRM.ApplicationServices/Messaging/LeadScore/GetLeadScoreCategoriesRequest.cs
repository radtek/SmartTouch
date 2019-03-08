using SmartTouch.CRM.Domain.LeadScoreRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadScore
{
    public class GetLeadScoreCategoriesRequest:ServiceRequestBase
    {
    }

    public class GetLeadScoreCategoriesResponse : ServiceResponseBase
    {
        public IEnumerable<ScoreCategories> Categories { get; set; }
    }
}
