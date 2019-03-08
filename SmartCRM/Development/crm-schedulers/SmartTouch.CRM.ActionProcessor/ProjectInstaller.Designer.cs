namespace SmartTouch.CRM.ActionProcessor
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
            this.smartCRMActionProcessorProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.smartCRMActionProcessor = new System.ServiceProcess.ServiceInstaller();
            // 
            // smartCRMActionProcessorProcessInstaller
            // 
            this.smartCRMActionProcessorProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.smartCRMActionProcessorProcessInstaller.Password = null;
            this.smartCRMActionProcessorProcessInstaller.Username = null;
            // 
            // smartCRMActionProcessor
            // 
            this.smartCRMActionProcessor.DisplayName = "Smart CRM - Action Processor";
            this.smartCRMActionProcessor.ServiceName = "Smart CRM - Action Processor";
            this.smartCRMActionProcessor.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.smartCRMActionProcessorProcessInstaller,
            this.smartCRMActionProcessor});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller smartCRMActionProcessorProcessInstaller;
        private System.ServiceProcess.ServiceInstaller smartCRMActionProcessor;
    }
}