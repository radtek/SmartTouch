using System;

namespace SmartTouch.CRM.StorageProvider.Exceptions
{
    public class InvalidDocumentException : Exception
    {
        public InvalidDocumentException() : base() { }
        public InvalidDocumentException(string message) : base(message) { }
        public InvalidDocumentException(string message, Exception innerException) : base(message, innerException) { }
    }
}
