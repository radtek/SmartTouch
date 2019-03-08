using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetAccountAuthorizationRequest : ServiceRequestBase
    {
        public string name { get; set; }
    }

    public class GetAccountAuthorizationResponse : ServiceResponseBase
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public string PrimaryEmail { get; set; }
        public byte Status { get; set; }
        public string HelpURL { get; set; }
        public int SubscriptionId { get; set; }
    }
}
