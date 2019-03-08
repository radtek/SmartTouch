
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
            this.schedulerServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            this.schedulerProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            // 
            // schedulerServiceInstaller
            // 
            this.schedulerServiceInstaller.Description = "Runs jobs on scheduled time";
            this.schedulerServiceInstaller.DisplayName = "LandmarkIT Scheduler";
            this.schedulerServiceInstaller.ServiceName = "LMITSchedulerService";
            this.schedulerServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // schedulerProcessInstaller
            // 
            this.schedulerProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.schedulerProcessInstaller.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.schedulerServiceInstaller});
            this.schedulerProcessInstaller.Password = null;
            this.schedulerProcessInstaller.Username = null;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.schedulerProcessInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceInstaller schedulerServiceInstaller;
        private System.ServiceProcess.ServiceProcessInstaller schedulerProcessInstaller;
    }
}