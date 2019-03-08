namespace SmartTouch.CRM.LeadProcessor
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
            this.smartCRMLeadProcessorProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.smartCRMLeadProcessor = new System.ServiceProcess.ServiceInstaller();
            // 
            // smartCRMLeadProcessorProcessInstaller
            // 
            this.smartCRMLeadProcessorProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.smartCRMLeadProcessorProcessInstaller.Password = null;
            this.smartCRMLeadProcessorProcessInstaller.Username = null;
            // 
            // smartCRMLeadProcessor
            // 
            this.smartCRMLeadProcessor.DisplayName = "Smart CRM - Lead Processor";
            this.smartCRMLeadProcessor.ServiceName = "Smart CRM - Lead Processor";
            this.smartCRMLeadProcessor.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.smartCRMLeadProcessorProcessInstaller,
            this.smartCRMLeadProcessor});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller smartCRMLeadProcessorProcessInstaller;
        private System.ServiceProcess.ServiceInstaller smartCRMLeadProcessor;
    }
}