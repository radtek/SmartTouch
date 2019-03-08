using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
   public class CommunicationTrackerRequest:ServiceRequestBase
    {
       public CommunicationTrackerViewModel CommunicationTrackerViewModel { get; set; }
    }
   public class CommunicationTrackerResponse : ServiceResponseBase
   {
       public virtual CommunicationTrackerViewModel CommunicationTrackerViewModel { get; set; }
   }
}
