


CREATE  PROCEDURE [dbo].[Archive_ImportCustomData_Table]

AS
BEGIN

  DECLARE @COUNT int

			SELECT  @COUNT = COUNT(distinct ImportCustomDataID)
			FROM SmartCRM.[dbo].[ImportCustomData]  ID WITH (NOLOCK)
			INNER JOIN   SmartCRM.[dbo].[ImportContactData] ICD WITH (NOLOCK) ON ICD.ReferenceID = ID.ReferenceID
			INNER JOIN  SmartCRM.[dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
			WHERE [CreatedDateTime] < DATEADD(d, -30, getdate()) 
  
  print @COUNT

IF @COUNT > 1

  BEGIN
      BEGIN
	    
  
		  INSERT INTO  SmartCRMArchive..ImportCustomData_Archive ([ImportCustomDataID],[FieldID],[FieldTypeID],[FieldValue],[ReferenceID])
		  SELECT DISTINCT [ImportCustomDataID],[FieldID],[FieldTypeID],[FieldValue],ID.[ReferenceID]
		  FROM SmartCRM.[dbo].[ImportCustomData]  ID WITH (NOLOCK)
			INNER JOIN   SmartCRM.[dbo].[ImportContactData] ICD  WITH (NOLOCK) ON ICD.ReferenceID = ID.ReferenceID
			INNER JOIN  SmartCRM.[dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
			WHERE [CreatedDateTime] < DATEADD(d, -30, getdate()) 
			

      END
 

		SELECT  distinct  ID.* INTO #tem
		FROM SmartCRM.[dbo].[ImportCustomData]  ID WITH (NOLOCK)
			INNER JOIN   SmartCRM.[dbo].[ImportContactData] ICD WITH (NOLOCK) ON ICD.ReferenceID = ID.ReferenceID
			INNER JOIN  SmartCRM.[dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
			WHERE [CreatedDateTime] < DATEADD(d, -30, getdate()) 
		  order by  ImportCustomDataID

		

	
	   
		
		DELETE  FROM SmartCRM..[ImportCustomData]
		 WHERE ImportCustomDataID IN (SELECT ImportCustomDataID   from #tem)
		 
		 
			
		 

			SELECT  @COUNT = COUNT(distinct ImportCustomDataID)
			FROM SmartCRM.[dbo].[ImportCustomData]  ID WITH (NOLOCK)
			INNER JOIN   SmartCRM.[dbo].[ImportContactData] ICD WITH (NOLOCK) ON ICD.ReferenceID = ID.ReferenceID
			INNER JOIN  SmartCRM.[dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
			WHERE [CreatedDateTime] < DATEADD(d, -30, getdate()) 

   END
  
END
