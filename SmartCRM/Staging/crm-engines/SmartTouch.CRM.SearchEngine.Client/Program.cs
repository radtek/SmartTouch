using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Tracing;
using System.Configuration;
using SimpleInjector;
using SmartTouch.CRM.ApplicationServices;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace SmartTouch.CRM.SearchEngine.Client
{
   static class Program
    {
        static void Main(string[] args)
        {
            Container container = new Container();
            IoC.Configure(container);
            InitializeAutoMapper.Initialize();

            #region Logging Configuration
            ExceptionHandler.Current.AddDefaultLogAndRethrowPolicy();
            ExceptionHandler.Current.AddDefaultLogOnlyPolicy();
            Logger.Current.CreateRollingFlatFileListener(EventLevel.Verbose, ConfigurationManager.AppSettings["SEARCH_ENGINE_CLIENT_LOG_FILE_PATH"], 2048);
            #endregion

            ElasticDataProcessor processor = new ElasticDataProcessor();
            processor.ReIndexAllEntities();

            Console.WriteLine("Completed indexing documents.");
            Console.WriteLine("Press any key to close.");
            Console.ReadLine();
        }
    }
}
