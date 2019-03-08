namespace SmartTouch.CRM.BulkOperationProcessor
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
            this.smartCRMBulkOperationProcessorProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.smartCRMBulkOperationProcessor = new System.ServiceProcess.ServiceInstaller();
            // 
            // smartCRMBulkOperationProcessorProcessInstaller
            // 
            this.smartCRMBulkOperationProcessorProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.smartCRMBulkOperationProcessorProcessInstaller.Password = null;
            this.smartCRMBulkOperationProcessorProcessInstaller.Username = null;
            // 
            // smartCRMBulkOperationProcessor
            // 
            this.smartCRMBulkOperationProcessor.DisplayName = "Smart CRM - Bulk Operation Processor";
            this.smartCRMBulkOperationProcessor.ServiceName = "Smart CRM - Bulk Operation Processor";
            this.smartCRMBulkOperationProcessor.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.smartCRMBulkOperationProcessorProcessInstaller,
            this.smartCRMBulkOperationProcessor});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller smartCRMBulkOperationProcessorProcessInstaller;
        private System.ServiceProcess.ServiceInstaller smartCRMBulkOperationProcessor;
    }
}