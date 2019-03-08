﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.APILeadSubmissionProcessor
{
    [RunInstaller(true)]
    public partial class ServicesInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller process;

        public ServicesInstaller()
        {
            try
            {
                process = new ServiceProcessInstaller();
                process.Account = ServiceAccount.LocalSystem;
                var installer = new ServiceInstaller();
                installer.ServiceName = "Smart CRM - API Lead Submission Processor";
                installer.Description = "Process API Submission Leads";
                installer.StartType = ServiceStartMode.Automatic;

                Installers.Add(process);
                Installers.Add(installer);

            }
            catch
            {

            }

        }
    }
}