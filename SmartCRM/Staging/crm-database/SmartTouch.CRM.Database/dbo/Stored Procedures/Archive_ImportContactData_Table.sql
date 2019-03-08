

CREATE   PROCEDURE [dbo].[Archive_ImportContactData_Table]

AS
BEGIN

  DECLARE @COUNT int

			SELECT  @COUNT = COUNT(distinct ImportContactDataID)
			FROM  SmartCRM.[dbo].[ImportContactData] ICD WITH (NOLOCK) 
			INNER JOIN  SmartCRM.[dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
			WHERE [CreatedDateTime] < DATEADD(d, -30, getdate()) 
  
  print @COUNT

IF @COUNT > 1

  BEGIN
      BEGIN
	    
     
		  INSERT INTO  SmartCRMArchive..ImportContactData_Archive 
		  (
		  ImportContactDataID,FirstName,LastName,CompanyName,Title,LeadSource,LifecycleStage,PartnerType,DoNotEmail,
		  HomePhone,MobilePhone,WorkPhone,AccountID,PrimaryEmail,SecondaryEmails,FacebookUrl,TwitterUrl,GooglePlusUrl,LinkedInUrl,BlogUrl,WebSiteUrl,AddressLine1,AddressLine2,
		  City,State,Country,ZipCode,CustomFieldsData,ContactID,ContactStatusID,ReferenceID,ContactTypeID,OwnerID,JobID,LeadSourceID,LifecycleStageID,PartnerTypeID,LoopID,CompanyID,
		  PhoneData,CommunicationID,EmailExists,IsBuilderNumberPass,LeadAdapterSubmittedData,LeadAdapterRowData,ValidEmail,OrginalRefId,IsDuplicate,IsCommunityNumberPass
          )
		  SELECT DISTINCT ImportContactDataID,FirstName,LastName,CompanyName,Title,LeadSource,LifecycleStage,PartnerType,DoNotEmail,
		  HomePhone,MobilePhone,WorkPhone,AccountID,PrimaryEmail,SecondaryEmails,FacebookUrl,TwitterUrl,GooglePlusUrl,LinkedInUrl,BlogUrl,WebSiteUrl,AddressLine1,AddressLine2,
		  City,State,Country,ZipCode,CustomFieldsData,ContactID,ContactStatusID,ReferenceID,ContactTypeID,OwnerID,JobID,LeadSourceID,LifecycleStageID,PartnerTypeID,LoopID,CompanyID,
		  PhoneData,CommunicationID,EmailExists,IsBuilderNumberPass,LeadAdapterSubmittedData,LeadAdapterRowData,ValidEmail,OrginalRefId,IsDuplicate,IsCommunityNumberPass
          FROM  SmartCRM.[dbo].[ImportContactData] ICD WITH (NOLOCK) 
	      INNER JOIN  SmartCRM.[dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
		  WHERE [CreatedDateTime] < DATEADD(d, -30, getdate())  
		  order by  ImportContactDataID
			

      END
 

		SELECT  distinct  ICD.* INTO #tem
		FROM   SmartCRM.[dbo].[ImportContactData] ICD WITH (NOLOCK) 
			INNER JOIN  SmartCRM.[dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
			WHERE [CreatedDateTime] < DATEADD(d, -30, getdate()) 
		  order by  ImportContactDataID

		SELECT COUNT(1) FROM #TEM

		declare @c int 
		set @c = (select  count(1) from SmartCRMArchive..ImportContactData_Archive )

		If @c >0

		begin

	     delete  FROM SmartCRM..[ImportContactData]
		 WHERE ImportContactDataID IN (SELECT ImportContactDataID   from #tem)

		 end
		
		 SELECT  @COUNT = COUNT(distinct ImportCustomDataID)
			FROM SmartCRM.[dbo].[ImportCustomData]  ID WITH (NOLOCK)
			INNER JOIN   SmartCRM.[dbo].[ImportContactData] ICD WITH (NOLOCK) ON ICD.ReferenceID = ID.ReferenceID
			INNER JOIN  SmartCRM.[dbo].[LeadAdapterJobLogs] LA WITH (NOLOCK) ON LA.LeadAdapterJobLogID =ICD.JobID
			WHERE [CreatedDateTime] < DATEADD(d, -30, getdate()) 

   END
  
END
