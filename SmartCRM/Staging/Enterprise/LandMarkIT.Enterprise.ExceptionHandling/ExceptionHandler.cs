using LandmarkIT.Enterprise.Logging;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LandmarkIT.Enterprise.ExceptionHandling
{
    public class ExceptionHandler
    {
        private static ExceptionHandler _instance = default(ExceptionHandler);

        private List<ExceptionPolicyDefinition> policyDefinitions = new List<ExceptionPolicyDefinition>();

        public static ExceptionHandler Current { get { if (_instance == null) _instance = new ExceptionHandler(); return _instance; } }
        public void AddPolicy(string policyName, IEnumerable<ExceptionPolicyEntry> policyEntries)
        {
            this.policyDefinitions.Add(new ExceptionPolicyDefinition(policyName, policyEntries));
        }

        public void AddDefaultLogAndRethrowPolicy()
        {
            this.policyDefinitions.Add(new ExceptionPolicyDefinition(DefaultExceptionPolicies.LOG_AND_RETHROW_POLICY, DefaultExceptionPolicies.LogAndRethrow));
        }

        public void AddDefaultLogOnlyPolicy()
        {
            this.policyDefinitions.Add(new ExceptionPolicyDefinition(DefaultExceptionPolicies.LOG_ONLY_POLICY, DefaultExceptionPolicies.LogOnly));
        }

        public bool HandleException(Exception exceptionToHandle, string policyName
            , [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int lineNumber = 0,
             TimeSpan timeSpan = default(TimeSpan), params object[] values)
        {
            Logger.Current.Error(exceptionToHandle.Message, exceptionToHandle, methodName, sourceFile, lineNumber, timeSpan, values);
            return this.Manager.HandleException(exceptionToHandle, policyName);
        }

        public bool HandleException(Exception exceptionToHandle, string policyName, out Exception exceptionToThrow
            , [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int lineNumber = 0,
             TimeSpan timeSpan = default(TimeSpan), params object[] values)
        {
            Logger.Current.Error(exceptionToHandle.Message, exceptionToHandle, methodName, sourceFile, lineNumber, timeSpan, values);
            return this.Manager.HandleException(exceptionToHandle, policyName, out exceptionToThrow);
        }

        internal ExceptionManager Manager
        {
            get
            {
                return new ExceptionManager(ExceptionHandler.Current.policyDefinitions);
            }
        }
    }
}
