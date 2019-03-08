using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.CustomFields
{
   public class GetSavedSearchsCountForCustomFieldRequest: ServiceRequestBase
    {
        public int fieldId { get; set; }
        public int? valueOptionId { get; set; }
    }

   public class GetSavedSearchsCountForCustomFieldResponse : ServiceResponseBase
   {
       public int Count { get; set; }
   }
}
