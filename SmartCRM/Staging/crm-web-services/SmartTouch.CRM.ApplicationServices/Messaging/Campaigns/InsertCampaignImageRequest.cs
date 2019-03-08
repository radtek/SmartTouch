using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
   public class InsertCampaignImageRequest : ServiceRequestBase
   {
       public ImageViewModel ImageViewModel { get; set; }
   }

   public class InsertCampaignImageResponse : ServiceResponseBase
   {
       public ImageViewModel ImageViewModel { get; set; }
   }
}
