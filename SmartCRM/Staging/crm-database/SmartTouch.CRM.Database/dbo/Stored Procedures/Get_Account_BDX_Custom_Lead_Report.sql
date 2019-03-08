

CREATE PROCEDURE [dbo].[Get_Account_BDX_Custom_Lead_Report]
	(
		@AccountID		int,
		@FromDate       datetime,
		@ToDate         datetime,
		@IsAdmin		tinyint,
		@OwnerID		int
	)
AS 
BEGIN
	SET NOCOUNT ON
	BEGIN TRY

	DECLARE @CustomFieldsTemptable	TABLE (	[ContactId] [int]  NULL,	[CommunityName] [nvarchar](1000)  NULL,	
                                        [LeadType] [nvarchar](1000)  NULL,[CommunityNumber] [nvarchar](1000)  NULL,
										[MarketName] [nvarchar](1000)  NULL,[PlanNumber] [nvarchar](1000)  NULL,
										[PlanName] [nvarchar](1000)  NULL,[Comments] [nvarchar](1000)  NULL,[BuilderName] [nvarchar](1000)  NULL,[StateName] [nvarchar](1000) NULL,[BuilderNumber] [nvarchar](1000) NULL)


	
   
	INSERT INTO @CustomFieldsTemptable(ContactId,CommunityName,LeadType,CommunityNumber,MarketName,PlanNumber,PlanName,Comments,BuilderName,StateName,BuilderNumber)
	SELECT ContactID, [0] 'CommunityName', [1] 'LeadType' ,[2] 'Community Number',[3] 'Market Name',[4] 'Plan Number',[5] 'Plan Name',[6] 'Comments',[7] 'Builder Name',[8] 'State Name',[9] 'Builder Number'
    FROM 
      (SELECT CCF.ContactID, CCF.Value, CASE WHEN FL.Title LIKE 'LeadType' THEN 1 
	                                         WHEN FL.Title LIKE 'Community Number' THEN 2
											 WHEN FL.Title LIKE 'Market Name' THEN 3
											 WHEN FL.Title LIKE 'Plan Number' THEN 4
											 WHEN FL.Title LIKE 'Plan Name' THEN 5
											 WHEN FL.Title LIKE 'Comments' THEN 6
											 WHEN FL.Title LIKE 'Builder Name' THEN 7
											 WHEN FL.Title LIKE 'State Name' THEN 8
											 WHEN FL.Title LIKE 'Builder Number' THEN 9
	                                         ELSE 0 END RN
      from ContactCustomFieldMap(NOLOCK) CCF  INNER JOIN dbo.Fields(NOLOCK) FL on FL.FieldID = CCF.CustomFieldID and FL.LeadAdapterType = 1 
      AND (FL.Title = 'LeadType' OR FL.Title = 'Community Name' OR FL.Title = 'Community Number' OR FL.Title = 'Comments' OR
	  FL.Title = 'Market Name' OR FL.Title = 'Plan Number' OR FL.Title = 'Plan Name' OR FL.Title = 'Builder Name' OR FL.Title = 'State Name' OR FL.Title = 'Builder Number') AND FL.AccountID = @AccountID) p
      PIVOT (MAX(Value) FOR p.RN IN ([0],[1],[2],[3],[4],[5],[6],[7],[8],[9])) as pt

	  	;WITH ContactAddressData
			AS (SELECT A.*,CA.ContactID,S.StateName FROM
			 dbo.Contacts C 
			INNER JOIN dbo.ContactAddressMap(NOLOCK) CA ON C.ContactID = CA.ContactID AND C.AccountID = @AccountID
			INNER JOIN  dbo.Addresses(NOLOCK) A ON A.AddressID = CA.AddressID AND A.IsDefault = 1	
			INNER JOIN dbo.States(NOLOCK) S ON A.StateID = S.StateID
			)

		
	  	SELECT DISTINCT LAJLD.LeadAdapterJobLogDetailID AS [LeadAdapterJobLogDetailID],LAJL.CreatedDateTime AS [CreatedDate], 
				CASE WHEN LAJLD.LeadAdapterRecordStatusID = 1 THEN 'Yes' ELSE 'No' END AS [ContactCreated],						
				LAJL.FileName, C.ContactID,LAJLD.SubmittedData,DV.DropdownValue LeadSource, (c.FirstName + '  ' +c.LastName) FullName,CE.Email PrimaryEmail,CF.CommunityName,
				CF.LeadType,CF.CommunityNumber,CF.MarketName,CF.PlanName,CF.PlanNumber,CF.Comments,CP.PhoneNumber 'Phone',AM.AddressLine1 + ' ' + AM.AddressLine2 'StreetAddress',AM.City,AM.StateName 'State',
				AM.ZipCode 'PostalCode',CF.BuilderName,CF.BuilderNumber,CF.StateName
	        FROM dbo.Contacts(NOLOCK) C          
			INNER JOIN dbo.LeadAdapterJobLogDetails(NOLOCK) LAJLD ON C.ReferenceID = LAJLD.ReferenceID and c.AccountID= @AccountID  and c.FirstContactSource = 1
			INNER JOIN dbo.LeadAdapterJobLogs(NOLOCK) LAJL ON LAJLD.LeadAdapterJobLogID = LAJL.LeadAdapterJobLogID	
			INNER JOIN dbo.LeadAdapterAndAccountMap(NOLOCK) LAAM ON LAAM.LeadAdapterAndAccountMapID = LAJL.LeadAdapterAndAccountMapID	AND LAAM.AccountID=C.AccountID	
		    INNER JOIN DBO.ContactLeadSourceMap(NOLOCK) CLM ON C.ContactID = CLM.ContactID AND CLM.IsPrimaryLeadSource = 1
			INNER JOIN DBO.DropdownValues(NOLOCK) DV ON DV.DropdownValueID = CLM.LeadSouceID AND DV.AccountID = C.AccountID
			LEFT JOIN dbo.ContactEmails(NOLOCK) CE ON C.ContactID = CE.ContactID AND CE.IsPrimary = 1 AND CE.AccountID = C.AccountID
			LEFT JOIN dbo.ContactPhoneNumbers(NOLOCK) CP ON C.ContactID = CP.ContactID AND CP.IsPrimary = 1	AND CP.AccountID = C.AccountID
			LEFT JOIN ContactAddressData AM ON C.ContactID = AM.ContactID	
            LEFT JOIN @CustomFieldsTemptable CF	ON CF.ContactID = C.ContactID						
		WHERE C.AccountID = @AccountID AND LAAM.LeadAdapterTypeID = 1

			AND (@IsAdmin = 1 OR C.OwnerID = @OwnerID)
			AND CONVERT(VARCHAR(10),LAJL.CreatedDateTime,120) >= @FromDate
			AND CONVERT(VARCHAR(10),LAJL.CreatedDateTime,120) <= @ToDate
		ORDER BY LAJL.CreatedDateTime DESC
	END TRY
	BEGIN CATCH		
		SELECT ErrorNumber = ERROR_NUMBER(), ErrorSeverity = ERROR_SEVERITY(), ErrorState = ERROR_STATE(),
			ErrorProcedure = ERROR_PROCEDURE(), ErrorLine = ERROR_LINE(), ErrorMessage = ERROR_MESSAGE()
	END CATCH
	SET NOCOUNT OFF
END

/*
SET STATISTICS TIME ON
	  EXEC [dbo].[Get_Account_BDX_Custom_Lead_Report]
		 @AccountID			= 4218,
		 @FromDate          = '2015-01-27 00:00:00.000',       
		 @ToDate            = '2015-05-28 00:00:00.000',
		 @IsAdmin			= 1,
		 @OwnerID			= 100

		 SET STATISTICS TIME OFF

*/








