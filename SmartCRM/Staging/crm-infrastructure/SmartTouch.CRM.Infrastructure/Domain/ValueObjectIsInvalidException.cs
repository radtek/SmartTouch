using System;

namespace SmartTouch.CRM.Infrastructure.Domain
{
    [Serializable]
    public class ValueObjectIsInvalidException : Exception
    {
        public ValueObjectIsInvalidException(string message)
            : base(message)
        { }
    }
}
