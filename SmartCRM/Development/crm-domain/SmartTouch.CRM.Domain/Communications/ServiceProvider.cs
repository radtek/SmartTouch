using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.ImageDomains;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;

namespace SmartTouch.CRM.Domain.Communication
{
    public class ServiceProvider : EntityBase<int>, IAggregateRoot
    {
        public CommunicationType CommunicationTypeID { get; set; }
        public System.Guid LoginToken { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int UserID { get; set; }
        public string ProviderName { get; set; }
        public MailType MailType { get; set; }
        public int AccountId { get; set; }
        public Account Account { get; set; }
        public bool IsDefault { get; set; }
        public string SenderPhoneNumber { get; set; }
        public byte? ImageDomainId { get; set; }
        public ImageDomain ImageDomain { get; set; }
        protected override void Validate()
        {
        }
    }
}
