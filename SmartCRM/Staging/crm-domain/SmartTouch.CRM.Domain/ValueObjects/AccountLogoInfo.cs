using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Entities;
using System;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class AccountLogoInfo : ValueObjectBase
    {
        public string StorageName { get; set; }
        public string AccountName { get; set; }
        public string PrivacyPolicy { get; set; }
        public string HelpURL { get; set; }
        public string WebsiteURL { get; set; }
        public int SubscriptionId { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
