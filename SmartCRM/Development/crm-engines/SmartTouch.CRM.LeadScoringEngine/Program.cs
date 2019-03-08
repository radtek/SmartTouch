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
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            bool env = Environment.UserInteractive;

            //if ((args.Length > 0) && (args[0] == "/console"))
            if (env)
            {
                Logger.Current.Informational("Starting the engine in CommandLine mode.");
                ScoringEngine engine;
                Container container = new Container();
                IoC.Configure(container);
                InitializeAutoMapper.Initialize();

                #region Logging Configuration
                ExceptionHandler.Current.AddDefaultLogAndRethrowPolicy();
                ExceptionHandler.Current.AddDefaultLogOnlyPolicy();
                Logger.Current.CreateRollingFlatFileListener(EventLevel.Verbose, ConfigurationManager.AppSettings["LEADSCORE_ENGINE_LOG_FILE_PATH"], 2048);
                #endregion

                engine = new ScoringEngine();
                engine.Start();
                Logger.Current.Informational("Engine started successfully.");
            }
            else
            {
                Logger.Current.Informational("Starting the engine in Windows Service mode.");
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                { 
                    new SmartCRMLeadScoringEngine() 
                };
                ServiceBase.Run(ServicesToRun);             
            }

            Console.ReadLine();
        }
    }
}
