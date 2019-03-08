using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tags
{
   public class ContactTagSummaryRequest: ServiceRequestBase
    {
       public TagViewModel Tag { get; set; }
       public IEnumerable<TagViewModel> AllTags { get; set; }
    }

   public class ContactTagSummaryResponse : ServiceResponseBase
   {
       public int CountByTag { get; set; }
       public int CountsByAllTags { get; set; }
   }
}
