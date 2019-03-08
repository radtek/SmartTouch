using System;

namespace SmartTouch.CRM.ApplicationServices.Exceptions
{
    [Serializable]
    public class ResourceNotFoundException : Exception
    {
        public ResourceNotFoundException(string message) : base(message) 
        {}

        public ResourceNotFoundException() : base("The requested resource was not found.")
        {}
    }
}
