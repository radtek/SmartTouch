using LandmarkIT.Enterprise.Logging;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System;

namespace LandmarkIT.Enterprise.ExceptionHandling
{
    public class SemanticLoggingErrorHandler : IExceptionHandler
    {
        public Exception HandleException(Exception exception, Guid handlingInstanceId)
        {
            Logger.Current.Error(exception.Message, exception, string.Empty, string.Empty, default(int), default(TimeSpan), handlingInstanceId);
            return exception;
        }
    }
}
