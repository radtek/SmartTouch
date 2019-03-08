using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Action
{
    public class InsertActionRequest : ServiceRequestBase
    {
        public ActionViewModel ActionViewModel { get; set; }
        public string AccountPrimaryEmail { get; set; }
        public string AccountAddress { get; set; }
        public string Location { get; set; }
        public string AccountPhoneNumber { get; set; }
        public string AccountDomain { get; set; }
        public string SelectAllSearchCriteria { get; set; }
        public string AdvancedSearchCritieria { get; set; }
        public int[] DrillDownContactIds { get; set; }
    }

    public class InsertActionResponse : ServiceResponseBase
    {
        public virtual ActionViewModel ActionViewModel { get; set; }
    }
}
