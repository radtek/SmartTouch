using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Tracing;
using System.Configuration;

using SimpleInjector;
using SmartTouch.CRM.ApplicationServices;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace SmartTouch.CRM.LeadScoringEngine
{
    public partial class SmartCRMLeadScoringEngine : ServiceBase
    {
        public SmartCRMLeadScoringEngine()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Container container = new Container();
            IoC.Configure(container);
            InitializeAutoMapper.Initialize();

            #region Logging Configuration
            ExceptionHandler.Current.AddDefaultLogAndRethrowPolicy();
            ExceptionHandler.Current.AddDefaultLogOnlyPolicy();
            Logger.Current.CreateRollingFlatFileListener(EventLevel.Verbose, ConfigurationManager.AppSettings["LEADSCORE_ENGINE_LOG_FILE_PATH"], 2048);
            #endregion

            ScoringEngine engine = new ScoringEngine();
            engine.Start();
            Logger.Current.Informational("Engine started successfully.");
        }

        protected override void OnStop()
        {
            Logger.Current.Informational("Engine stopped successfully.");
        }
    }
}
