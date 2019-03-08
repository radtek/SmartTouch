using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadScore
{
   public class UpdateLeadScoreRequest:ServiceRequestBase
    {
       public LeadScoreViewModel LeadScoreViewModel { get; set; }
    }
    public class UpdateLeadScoreResponse:ServiceResponseBase
    {

    }
}
