using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System;
using System.Collections.Generic;

namespace LandmarkIT.Enterprise.ExceptionHandling
{
    public class DefaultExceptionPolicies
    {
        internal static IEnumerable<ExceptionPolicyEntry> LogAndRethrow = new List<ExceptionPolicyEntry>
                  {
                      {
                          new ExceptionPolicyEntry(typeof (Exception),
                              PostHandlingAction.NotifyRethrow,
                              new IExceptionHandler[]
                                {
                                    //new SemanticLoggingErrorHandler()                  
                                })
                      }      
                   };

        internal static IEnumerable<ExceptionPolicyEntry> LogOnly = new List<ExceptionPolicyEntry>
                  {
                      {
                          new ExceptionPolicyEntry(typeof (Exception),
                              PostHandlingAction.None,
                              new IExceptionHandler[]
                                {
                                  //new SemanticLoggingErrorHandler()                  
                                })
                      }      
                   };
        public const string LOG_AND_RETHROW_POLICY = "LogAndRethrow";
        public const string LOG_ONLY_POLICY = "LogOnly";
    }
}
