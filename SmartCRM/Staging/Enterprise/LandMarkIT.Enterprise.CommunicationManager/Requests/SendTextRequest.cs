
using SmartTouch.CRM.Domain.Contacts;
using System;
using System.Collections.Generic;
namespace LandmarkIT.Enterprise.CommunicationManager.Requests
{
    public class SendTextRequest
    {
        public string From { get; set; }
        public List<string> To { get; set; }
        public string SenderId { get; set; }
        public string Message { get; set; }
        public Guid RequestGuid { get; set; }
        public Guid TokenGuid { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public List<ContactOwnerPhone> OwnerNumbers { get; set; }
    }

    public class SendSingleTextRequest : SendTextRequest
    {
        public new string To { get; set; }
    }
}
