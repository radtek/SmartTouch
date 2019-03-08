


CREATE   PROCEDURE [dbo].[Archive_ImportPhoneData_Table]

AS
BEGIN

  DECLARE @COUNT int

        SELECT  @COUNT = COUNT(distinct ImportPhoneDataID)
	    FROM SmartCRM.[dbo].[ImportPhoneData] IPD   WITH (NOLOCK)
	    INNER JOIN   SmartCRM.[dbo].[ImportContactData] ICD WITH (NOLOCK) ON ICD.ReferenceID = IPD.ReferenceID
		INNER JOIN  SmartCRM. [dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
		WHERE LA.[CreatedDateTime] < DATEADD(d, -30, getdate())
  
  print @COUNT

IF @COUNT > 1

  BEGIN
      BEGIN
	    
  
		  INSERT INTO SmartCRMArchive..ImportPhoneData_Archive (ImportPhoneDataID,PhoneType,[PhoneNumber],[ReferenceID])
		  SELECT distinct ImportPhoneDataID,PhoneType,[PhoneNumber],IPD.[ReferenceID]
		  FROM SmartCRM.[dbo].[ImportPhoneData] IPD   WITH (NOLOCK)
		  INNER JOIN   SmartCRM.[dbo].[ImportContactData] ICD WITH (NOLOCK) ON ICD.ReferenceID = IPD.ReferenceID
		  INNER JOIN  SmartCRM. [dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
		  WHERE LA.[CreatedDateTime] < DATEADD(d, -30, getdate())
		  order by  ImportPhoneDataID

      END
 

		SELECT  distinct  IPD.* INTO #tem
		FROM SmartCRM.[dbo].[ImportPhoneData] IPD   WITH (NOLOCK)
		INNER JOIN   SmartCRM.[dbo].[ImportContactData] ICD WITH (NOLOCK) ON ICD.ReferenceID = IPD.ReferenceID
		INNER JOIN  SmartCRM. [dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
		WHERE LA.[CreatedDateTime] < DATEADD(d, -30, getdate())
		order by ImportPhoneDataID

		

	
	   
		
		 delete FROM SmartCRM..[ImportPhoneData]
		 WHERE ImportPhoneDataID IN (SELECT ImportPhoneDataID   from #tem)
		 
			
		 


		SELECT  @COUNT = COUNT(distinct ImportPhoneDataID)
	    FROM SmartCRM.[dbo].[ImportPhoneData] IPD   WITH (NOLOCK)
	    INNER JOIN   SmartCRM.[dbo].[ImportContactData] ICD WITH (NOLOCK) ON ICD.ReferenceID = IPD.ReferenceID
		INNER JOIN  SmartCRM. [dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
		WHERE LA.[CreatedDateTime] < DATEADD(d, -30, getdate())

   END
  
END
