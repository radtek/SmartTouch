namespace SmartTouch.CRM.LeadScoringEngine
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
            this.SmartCRMLeadScoreEngineProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.SmartCRMLeadScoreEngineServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // SmartCRMLeadScoreEngineProcessInstaller
            // 
            this.SmartCRMLeadScoreEngineProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.SmartCRMLeadScoreEngineProcessInstaller.Password = null;
            this.SmartCRMLeadScoreEngineProcessInstaller.Username = null;
            // 
            // SmartCRMLeadScoreEngineServiceInstaller
            // 
            this.SmartCRMLeadScoreEngineServiceInstaller.Description = "SmartTouch Lead Scoring Engine.";
            this.SmartCRMLeadScoreEngineServiceInstaller.DisplayName = "SmartCRM LeadScoring Engine";
            this.SmartCRMLeadScoreEngineServiceInstaller.ServiceName = "LeadScoringService";
            this.SmartCRMLeadScoreEngineServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.SmartCRMLeadScoreEngineProcessInstaller,
            this.SmartCRMLeadScoreEngineServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller SmartCRMLeadScoreEngineProcessInstaller;
        private System.ServiceProcess.ServiceInstaller SmartCRMLeadScoreEngineServiceInstaller;
    }
}