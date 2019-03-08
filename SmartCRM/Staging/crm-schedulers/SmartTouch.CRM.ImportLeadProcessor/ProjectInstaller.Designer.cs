namespace SmartTouch.CRM.ImportLeadProcessor
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
            this.smartCRMImportLeadProcessorProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.smartCRMImportLeadProcessor = new System.ServiceProcess.ServiceInstaller();
            // 
            // smartCRMImportLeadProcessorProcessInstaller
            // 
            this.smartCRMImportLeadProcessorProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.smartCRMImportLeadProcessorProcessInstaller.Password = null;
            this.smartCRMImportLeadProcessorProcessInstaller.Username = null;
            // 
            // smartCRMImportLeadProcessor
            // 
            this.smartCRMImportLeadProcessor.DisplayName = "Smart CRM - Import Lead Processor";
            this.smartCRMImportLeadProcessor.ServiceName = "Smart CRM - Import Lead Processor";
            this.smartCRMImportLeadProcessor.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.smartCRMImportLeadProcessorProcessInstaller,
            this.smartCRMImportLeadProcessor});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller smartCRMImportLeadProcessorProcessInstaller;
        private System.ServiceProcess.ServiceInstaller smartCRMImportLeadProcessor;
    }
}