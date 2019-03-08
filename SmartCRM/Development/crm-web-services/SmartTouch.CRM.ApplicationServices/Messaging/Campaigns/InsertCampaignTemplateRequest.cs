using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
   public class InsertCampaignTemplateRequest:ServiceRequestBase
    {
       public CampaignTemplateViewModel CampaignTemplateViewModel { get; set; }
    }

   public class InsertCampaignTemplateResponse : ServiceResponseBase
   {
      
   }
}
