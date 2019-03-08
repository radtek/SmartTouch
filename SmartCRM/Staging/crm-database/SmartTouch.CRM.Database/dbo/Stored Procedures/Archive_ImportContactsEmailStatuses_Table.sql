

CREATE PROCEDURE [dbo].[Archive_ImportContactsEmailStatuses_Table]

AS
BEGIN

  DECLARE @COUNT int
  
        SELECT  @COUNT = COUNT(DISTINCT MailGunVerificationID)
        FROM [dbo].[ImportContactsEmailStatuses] ICS WITH (NOLOCK)
		INNER JOIN   [dbo].[ImportContactData] ICD WITH (NOLOCK) ON ICD.ReferenceID = ICS.ReferenceID
		INNER JOIN  [dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
		WHERE [CreatedDateTime] < DATEADD(d, -30, getdate()) 
		


 PRINT @COUNT


  IF @COUNT > 1

  BEGIN
       BEGIN

  
			  INSERT INTO SmartCRMArchive..ImportContactsEmailStatuses_Archive ([MailGunVerificationID],[ReferenceID],[EmailStatus])
			  SELECT DISTINCT [MailGunVerificationID],ICS.[ReferenceID],[EmailStatus]
			  from [dbo].[ImportContactsEmailStatuses] ICS WITH (NOLOCK)
			  INNER JOIN   [dbo].[ImportContactData] ICD WITH (NOLOCK) ON ICD.ReferenceID = ICS.ReferenceID
			  INNER JOIN  [dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
			  WHERE [CreatedDateTime] < DATEADD(d, -30, getdate()) 
			  ORDER BY  MailGunVerificationID asc
	

        END
 

		  SELECT   DISTINCT ICS.* INTO #tem
		  from [dbo].[ImportContactsEmailStatuses] ICS WITH (NOLOCK)
		  INNER JOIN   [dbo].[ImportContactData] ICD WITH (NOLOCK) ON ICD.ReferenceID = ICS.ReferenceID
		  INNER JOIN  [dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
		  WHERE [CreatedDateTime] < DATEADD(d, -30, getdate()) 
		  ORDER BY MailGunVerificationID

		  SELECT COUNT(1) FROM #tem

	
		;
		WITH Tem (MailGunVerificationID)
		AS 
		(
			 SELECT  MailGunVerificationID FROM #TEM
		)
		DELETE FROM SmartCRM..ImportContactsEmailStatuses
		WHERE MailGunVerificationID IN (SELECT MailGunVerificationID FROM Tem) 
		;


        SELECT  @COUNT = COUNT(DISTINCT MailGunVerificationID)
        FROM [dbo].[ImportContactsEmailStatuses] ICS WITH (NOLOCK)
		INNER JOIN   [dbo].[ImportContactData] ICD WITH (NOLOCK) ON ICD.ReferenceID = ICS.ReferenceID
		INNER JOIN  [dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
		WHERE [CreatedDateTime] < DATEADD(d, -30, getdate()) 

   END
  
END
