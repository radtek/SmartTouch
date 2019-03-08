using System;

namespace SmartTouch.CRM.ApplicationServices.Messaging
{
    public abstract class ServiceResponseBase
    {
        public ServiceResponseBase()
        {
            
        }

        public virtual Exception Exception { get; set; }
    }
}
