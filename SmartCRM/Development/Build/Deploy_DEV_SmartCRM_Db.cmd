echo off

"C:\Program Files (x86)\Microsoft SQL Server\130\DAC\bin\SqlPackage.exe" /Action:Publish /SourceFile:"..\crm-database\SmartTouch.CRM.SmartCrmDatabase\bin\Debug\SmartTouch.CRM.SmartCrmDatabase.dacpac" /TargetConnectionString:"Server=192.168.1.14;Initial Catalog=SmartCRM_SSDT;User Id=smarttouchdev;Password=myp@$$dev" /p:BlockOnPossibleDataLoss=true /v:EnterpriseCommunicationDb="EnterpriseCommunication"
