using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.Utilities.ExceptionHandling
{
    [Serializable]
    public class UnsupportedOperationException : Exception
    {
        public UnsupportedOperationException()
        : base() { }

        public UnsupportedOperationException(string message) : base(message) { }
    
        public UnsupportedOperationException(string format, params object[] args) : base(string.Format(format, args)) { }
    
        public UnsupportedOperationException(string message, Exception innerException) : base(message, innerException) { }
    
        public UnsupportedOperationException(string format, Exception innerException, params object[] args) : base(string.Format(format, args), innerException) { }

        protected UnsupportedOperationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
