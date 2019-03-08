
namespace Prayogs.Communication.SmsService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.smartCrmSchedulerServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            this.smartCrmSchedulerProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            // 
            // smartCrmSchedulerServiceInstaller
            // 
            this.smartCrmSchedulerServiceInstaller.Description = "Runs Smart CRM jobs on scheduled time";
            this.smartCrmSchedulerServiceInstaller.DisplayName = "Smart CRM Scheduler";
            this.smartCrmSchedulerServiceInstaller.ServiceName = "SmartCrmSchedulerService";
            this.smartCrmSchedulerServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // smartCrmSchedulerProcessInstaller
            // 
            this.smartCrmSchedulerProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.smartCrmSchedulerProcessInstaller.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.smartCrmSchedulerServiceInstaller});
            this.smartCrmSchedulerProcessInstaller.Password = null;
            this.smartCrmSchedulerProcessInstaller.Username = null;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.smartCrmSchedulerProcessInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceInstaller smartCrmSchedulerServiceInstaller;
        private System.ServiceProcess.ServiceProcessInstaller smartCrmSchedulerProcessInstaller;
    }
}