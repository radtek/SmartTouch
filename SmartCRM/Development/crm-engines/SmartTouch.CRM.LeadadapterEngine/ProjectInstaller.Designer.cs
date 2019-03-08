namespace SmartTouch.CRM.LeadAdapterEngine
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
            this.smartCRMLeadAdapterProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.smartCRMLeadAdapterEngineInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // smartCRMLeadAdapterProcessInstaller
            // 
            this.smartCRMLeadAdapterProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.smartCRMLeadAdapterProcessInstaller.Password = null;
            this.smartCRMLeadAdapterProcessInstaller.Username = null;
            // 
            // smartCRMLeadAdapterEngineInstaller
            // 
            this.smartCRMLeadAdapterEngineInstaller.ServiceName = "SmartTouch LeadAdapter Engine";
            this.smartCRMLeadAdapterEngineInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.smartCRMLeadAdapterProcessInstaller,
            this.smartCRMLeadAdapterEngineInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller smartCRMLeadAdapterProcessInstaller;
        private System.ServiceProcess.ServiceInstaller smartCRMLeadAdapterEngineInstaller;
    }
}