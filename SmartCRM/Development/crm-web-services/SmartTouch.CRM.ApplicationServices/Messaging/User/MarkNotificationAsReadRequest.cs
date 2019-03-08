using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class MarkNotificationAsReadRequest : ServiceRequestBase
    {
        public Notification Notification { get; set; }
    }

    public class MarkNotificationAsReadResponse : ServiceResponseBase
    { 
    }
}
