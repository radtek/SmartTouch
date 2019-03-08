namespace SmartTouch.CRM.Automation.Engine
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
            this.smartCRMAutomationProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.SmartCRMAutomationEngineInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // smartCRMAutomationProcessInstaller
            // 
            this.smartCRMAutomationProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.smartCRMAutomationProcessInstaller.Password = null;
            this.smartCRMAutomationProcessInstaller.Username = null;
            // 
            // SmartCRMAutomationEngineInstaller
            // 
            this.SmartCRMAutomationEngineInstaller.DisplayName = "Smart CRM Automation Engine";
            this.SmartCRMAutomationEngineInstaller.ServiceName = "SmartCRMAutomationEngine";
            this.SmartCRMAutomationEngineInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.smartCRMAutomationProcessInstaller,
            this.SmartCRMAutomationEngineInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller smartCRMAutomationProcessInstaller;
        private System.ServiceProcess.ServiceInstaller SmartCRMAutomationEngineInstaller;
    }
}