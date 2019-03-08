namespace SmartTouch.CRM.WebAnalyticsEngine
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
            this.smartCRMWebAnalyticsProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.smartCRMWebAnalyticsInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // smartCRMWebAnalyticsProcessInstaller
            // 
            this.smartCRMWebAnalyticsProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.smartCRMWebAnalyticsProcessInstaller.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.smartCRMWebAnalyticsInstaller});
            this.smartCRMWebAnalyticsProcessInstaller.Password = null;
            this.smartCRMWebAnalyticsProcessInstaller.Username = null;
            // 
            // smartCRMWebAnalyticsInstaller
            // 
            this.smartCRMWebAnalyticsInstaller.DisplayName = "SmartTouch Web Analytics Engine";
            this.smartCRMWebAnalyticsInstaller.ServiceName = "SmartCRMWebAnalyticsEngine";
            this.smartCRMWebAnalyticsInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.smartCRMWebAnalyticsProcessInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller smartCRMWebAnalyticsProcessInstaller;
        private System.ServiceProcess.ServiceInstaller smartCRMWebAnalyticsInstaller;
    }
}