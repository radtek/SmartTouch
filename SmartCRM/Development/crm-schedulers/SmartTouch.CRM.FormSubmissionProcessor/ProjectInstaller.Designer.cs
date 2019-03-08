namespace SmartTouch.CRM.FormSubmissionProcessor
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
            this.smartCRMFormSubmissionProcessorProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.smartCRMFormSubmissionProcessor = new System.ServiceProcess.ServiceInstaller();
            // 
            // smartCRMFormSubmissionProcessorProcessInstaller
            // 
            this.smartCRMFormSubmissionProcessorProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.smartCRMFormSubmissionProcessorProcessInstaller.Password = null;
            this.smartCRMFormSubmissionProcessorProcessInstaller.Username = null;
            // 
            // smartCRMFormSubmissionProcessor
            // 
            this.smartCRMFormSubmissionProcessor.DisplayName = "Smart CRM - Form Submission Processor";
            this.smartCRMFormSubmissionProcessor.ServiceName = "Smart CRM - Form Submission Processor";
            this.smartCRMFormSubmissionProcessor.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.smartCRMFormSubmissionProcessorProcessInstaller,
            this.smartCRMFormSubmissionProcessor});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller smartCRMFormSubmissionProcessorProcessInstaller;
        private System.ServiceProcess.ServiceInstaller smartCRMFormSubmissionProcessor;
    }
}