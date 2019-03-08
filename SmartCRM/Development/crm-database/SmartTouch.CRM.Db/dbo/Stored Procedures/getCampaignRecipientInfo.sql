CREATE  PROCEDURE [dbo].[GetCampaignRecipientInfo](
	@CampaignID int,  
	@IsLinkedToWorkflow bit = 0
) AS
BEGIN

    

	DECLARE @AccountID int

	SELECT @AccountID = AccountID  
	FROM Campaigns  WITH(NOLOCK)  
	WHERE campaignid = @CampaignID 

     DECLARE @ResultID INT 
		INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		VALUES('GetCampaignRecipientInfo', @AccountID, '@CampaignID:' + cast(@CampaignID as varchar(10)))
		SET @ResultID = scope_identity()


	DECLARE @HTMLContent nvarchar(max),
			@FindStr int = 0,
			@EndIndex int = 0,
			@Searchstring varchar(100),
			@checkDuplicate int = 0,
			@FieldType varchar(1),
			@FieldID int--,
			--@AccountID int

	DECLARE @FieldList table 
	(
		[MergeCodeID] [int] IDENTITY(1,1) NOT NULL,
		[MergeCode] [varchar](100) NULL,
		[FieldType] [varchar](1) NULL,
		[FieldID] [int] NULL
	)

	DECLARE @campaignid_char VARCHAR(100)= CONVERT(VARCHAR(100),@campaignid)

	SELECT @HTMLContent = HTMLContent, @AccountID = AccountID  
	FROM Campaigns  WITH(NOLOCK)  
	WHERE campaignid = @CampaignID 
	
	--Default Merge Codes
	INSERT INTO @FieldList
	SELECT 'EMAILID', 'S', 0
	
	SET @FindStr = CHARINDEX('*|', @HTMLContent )
	
	WHILE(@FindStr > 0)
	BEGIN
		SET @EndIndex = CHARINDEX('|*', @HTMLContent, @FindStr+1 )
		--PRINT @EndIndex
		IF(@EndIndex > 0)
		BEGIN
			SET @Searchstring = SUBSTRING(@HTMLContent, @FindStr+2, (@EndIndex-@FindStr-2))
		
			SET @checkDuplicate = 0
			SET @FieldType = 'S'
			SET @FieldID = 0

			IF( ISNUMERIC(REPLACE(REPLACE(@Searchstring, 'CF', ''), 'DF', '')) = 1 )
			BEGIN
				SET @FieldType = LEFT(RIGHT(@Searchstring, 2), 1)
				SET @FieldID = CAST( (REPLACE(REPLACE(@Searchstring, 'CF', ''), 'DF', '')) as int)
			END

			SELECT	@checkDuplicate = MergeCodeID
			FROM	@FieldList
			WHERE	MergeCode = @Searchstring

			IF( ISNULL(@checkDuplicate, 0) = 0 )
			BEGIN
				INSERT INTO @FieldList( MergeCode, [FieldType], FieldID )
				VALUES( @Searchstring, @FieldType, @FieldID )
			END

			--PRINT @Searchstring
		END
		ELSE
		BEGIN
			BREAK
		END
		SET @FindStr = CHARINDEX('*|', @HTMLContent, @FindStr+1 )

	END
	

	SELECT CampaignRecipientID CRID, ContactId Id, [To]  Email
	INTO #Recipients
	FROM CampaignRecipients CR (NOLOCK) WHERE CampaignID = @CampaignID  
	AND AccountId = @AccountID AND ((@IsLinkedToWorkflow = 1 AND WorkflowID > 0) OR (@IsLinkedToWorkflow = 0 AND 1=1))
	AND (DeliveryStatus IS NULL OR DeliveryStatus = 116) 



	CREATE TABLE #contactFields (CRID INT, ContactId INT, FieldCode VARCHAR(10), FieldValue VARCHAR(200))
	
	INSERT INTO #contactFields
	SELECT	r.CRID, c.ContactID, x.MergeCode, x.MergeValue
	FROM	SmartCRM.dbo.Contacts (NOLOCK) c
	INNER JOIN #Recipients r ON R.Id = C.ContactID
	LEFT JOIN SmartCRM.dbo.Communications (NOLOCK) cm on cm.CommunicationID = c.CommunicationID
	LEFT JOIN (  
        select top 1 ContactID, a.*  
        from dbo.ContactAddressMap cam  WITH(NOLOCK) inner join dbo.Addresses a  WITH(NOLOCK) on cam.AddressID = a.AddressID and a.IsDefault = 1  
       ) a on a.ContactID = c.ContactID 
	LEFT JOIN SmartCRM.dbo.states (NOLOCK) s on s.StateID = a.StateID
	LEFT JOIN SmartCRM.dbo.Countries (NOLOCK) cn on cn.CountryID = a.CountryID
	CROSS APPLY (
		VALUES ('FIRSTNAME', C.FirstName),
				('LASTNAME', C.LastName),
				('EMAILID', r.Email),
				('COMPANY', C.Company),
				('TITLE',C.Title),
				('FBURL', CM.FacebookUrl),
				('LINKEDURL', CM.LinkedInUrl),
				('GPLUSURL', CM.GooglePlusUrl),
				('TWITERURL', CM.TwitterUrl),
				('WEBSITEURL', CM.WebSiteUrl),
				('BLOGURL', CM.BlogUrl),
				('ADDLINE1', A.AddressLine1),
				('ADDLINE2',A.AddressLine2),
				('CITY',A.City),
				('STATE',S.StateName),
				('ZIPCODE',A.ZipCode),
				('COUNTRY', CN.CountryName),
				('CRID', CONVERT(VARCHAR(1000),r.CRId)),
				('CAMPID',@campaignid_char)
	) as x (MergeCode, MergeValue)
	INNER JOIN @FieldList fl ON fl.MergeCode = x.MergeCode
	WHERE C.AccountID = @AccountID
	AND x.MergeValue IS NOT NULL
	

			
	INSERT INTO #contactFields
	SELECT	r.CRID, ContactID, CAST(CustomFieldID AS VARCHAR) + FieldType + 'F' FieldName, FieldValue
	FROM	SmartCrm.dbo.GET_ContactCustomField_Values cfv 
	inner join @FieldList fl on fl.FieldID = cfv.CustomFieldID
	INNER JOIN #Recipients R ON R.Id = cfv.ContactID
	WHERE	FieldType IN ( 'C' )
			
	INSERT INTO #contactFields
	SELECT	r.CRID, cdv.ContactID, CAST(CustomFieldID AS VARCHAR) + FieldType + 'F' FieldName, FieldValue
	FROM	SmartCrm.dbo.GET_DropdownFieldValues cdv 
	inner join @FieldList fl on fl.FieldID = cdv.CustomFieldID
	INNER JOIN #Recipients R ON R.Id = cdv.ContactID
	WHERE	AccountID = @AccountID 
			AND FieldType IN ( 'D' )
			AND IsPrimary = 1



	DELETE FROM @FieldList WHERE MergeCode IN ('CRID','CAMPID')

	DECLARE @CFList VARCHAR(MAX)
	SELECT @CFList = COALESCE(@CFList+',','')+ '[' +MergeCode +']' FROM @FieldList
	ORDER BY FieldType DESC, MergeCode ASC

	DECLARE @SQL NVARCHAR(MAX) = N'SELECT  CONTACTID, CRID,' + @campaignid_char  + ' as CAMPID,  @CFList 
	FROM ( SELECT   ContactId, CRID,    FieldCode ,FieldValue FROM #contactFields) AS S  
	PIVOT 
	( 
		MAX(FieldValue) 
		FOR FieldCode IN (@CFList) 
	) P '
	
	SET @SQL =N'' + REPLACE(@SQL,'@CFList', @CFList)

	EXEC sp_executesql @SQL

	print @SQL

	UPDATE	StoreProcExecutionResults
		SET		EndTime = GETDATE(),
				TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
				Status = 'C'
		WHERE	ResultID = @ResultID

END

/*
EXEC dbo.GetCampaignRecipientInfo_628 18235,0
EXEC dbo.GetCampaignRecipientInfo 18235,0
*/

