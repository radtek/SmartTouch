namespace SmartTouch.CRM.CampaignProcessor
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
            this.smartCRMCampaignProcessorProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.smartCRMCampaignProcessor = new System.ServiceProcess.ServiceInstaller();
            // 
            // smartCRMCampaignProcessorProcessInstaller
            // 
            this.smartCRMCampaignProcessorProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.smartCRMCampaignProcessorProcessInstaller.Password = null;
            this.smartCRMCampaignProcessorProcessInstaller.Username = null;
            // 
            // smartCRMCampaignProcessor
            // 
            this.smartCRMCampaignProcessor.DisplayName = "Smart CRM - Campaign Processor";
            this.smartCRMCampaignProcessor.ServiceName = "Smart CRM - Campaign Processor";
            this.smartCRMCampaignProcessor.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.smartCRMCampaignProcessorProcessInstaller,
            this.smartCRMCampaignProcessor});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller smartCRMCampaignProcessorProcessInstaller;
        private System.ServiceProcess.ServiceInstaller smartCRMCampaignProcessor;
    }
}