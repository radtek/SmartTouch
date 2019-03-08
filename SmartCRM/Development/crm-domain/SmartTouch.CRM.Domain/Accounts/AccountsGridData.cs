using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Accounts
{
    public class AccountsGridData : EntityBase<byte>, IAggregateRoot
    {
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public byte Status { get; set; }
        public string StatusMessage { get; set; }
        public int SenderReputationCount { get; set; }
        public int ContactsCount { get; set; }
        public int EmailsCount { get; set; }
        public string DomainURL { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ActiveUsersCount { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? LastCampaignSent { get; set; }
        public byte SubscriptionID { get; set; }
        public string SubscriptionName { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
