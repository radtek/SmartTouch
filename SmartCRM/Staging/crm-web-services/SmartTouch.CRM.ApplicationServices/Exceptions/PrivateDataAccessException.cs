using System;

namespace SmartTouch.CRM.ApplicationServices.Exceptions
{
    public class PrivateDataAccessException : Exception
    {
        public PrivateDataAccessException(string message)
            : base(message)
        { }
    }
}
