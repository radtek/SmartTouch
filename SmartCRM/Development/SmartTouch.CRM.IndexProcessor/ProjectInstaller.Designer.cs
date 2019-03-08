namespace SmartTouch.CRM.IndexProcessor
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
            this.smartCRMIndexProcessorProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.smartCRMIndexProcessor = new System.ServiceProcess.ServiceInstaller();
            // 
            // smartCRMIndexProcessorProcessInstaller
            // 
            this.smartCRMIndexProcessorProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.smartCRMIndexProcessorProcessInstaller.Password = null;
            this.smartCRMIndexProcessorProcessInstaller.Username = null;
            // 
            // smartCRMIndexProcessor
            // 
            this.smartCRMIndexProcessor.DisplayName = "Smart CRM - Index Processor";
            this.smartCRMIndexProcessor.ServiceName = "Smart CRM - Index Processor";
            this.smartCRMIndexProcessor.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.smartCRMIndexProcessorProcessInstaller,
            this.smartCRMIndexProcessor});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller smartCRMIndexProcessorProcessInstaller;
        private System.ServiceProcess.ServiceInstaller smartCRMIndexProcessor;
    }
}