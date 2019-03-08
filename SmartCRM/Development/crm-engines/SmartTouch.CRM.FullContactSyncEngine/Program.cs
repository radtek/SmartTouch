using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SimpleInjector;
using SmartTouch.CRM.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.FullContactSyncEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            Container container = new Container();
            IoC.Configure(container);
            InitializeAutoMapper.Initialize();

            #region Logging Configuration
            ExceptionHandler.Current.AddDefaultLogAndRethrowPolicy();
            ExceptionHandler.Current.AddDefaultLogOnlyPolicy();
            Logger.Current.CreateRollingFlatFileListener(EventLevel.Verbose, ConfigurationManager.AppSettings["LOG_FILE_PATH"], 2048);
            #endregion

            Console.WriteLine("Data refresh with Fullcontact api is scheduled. ");
            FullContactProcessor processor = new FullContactProcessor();
            processor.RefreshData();
            Console.WriteLine("Press any key to terminate the process. ");
            Console.ReadLine();
        }
    }
}
