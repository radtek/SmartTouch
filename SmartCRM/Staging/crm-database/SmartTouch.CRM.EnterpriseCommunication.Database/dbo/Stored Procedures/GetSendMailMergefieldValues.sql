
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetSendMailMergefieldValues]
(
	@SentMailDetailID INT	
)
AS
BEGIN
	DECLARE @HTMLContent nvarchar(max),
			@FindStr int = 0,
			@EndIndex int = 0,
			@Searchstring varchar(100),
			@checkDuplicate int = 0,
			@FieldType varchar(1),
			@FieldID int,
			@AccountID int,
			@To VARCHAR(MAX),
			@tokenGuid UNIQUEIDENTIFIER,
			@requestGuid UNIQUEIDENTIFIER,
			@accountDomain VARCHAR(MAX),
			@accountUrl VARCHAR(MAX)
			


	DECLARE @FieldList table 
	(
		[MergeCodeID] [int] IDENTITY(1,1) NOT NULL,
		[MergeCode] [varchar](100) NULL,
		[FieldType] [varchar](1) NULL,
		[FieldID] [int] NULL
	)

	SELECT @HTMLContent = Body, @AccountID = AccountID, @To = [To] FROM dbo.SentMailDetails (NOLOCK) WHERE SentMailDetailID = @SentMailDetailID
	SELECT @tokenGuid = TokenGuid FROM SentMailQueue (NOLOCK) WHERE RequestGuid = @requestGuid
	SELECT @accountUrl=  DomainUrl from SmartCrm.dbo.Accounts (nolock) WHERE AccountID = @AccountID
	
	IF CHARINDEX('.', @accountUrl) > 0
		BEGIN
			SELECT @accountDomain = LEFT(@accountUrl,CHARINDEX('.',@accountUrl)-1) 
		END
	


	DECLARE @TOAddresses TABLE (ToAddress VARCHAR(100))
	DECLARE @Recipients TABLE (Id INT, Email Varchar(100))

	INSERT INTO @TOAddresses 
	SELECT DataValue FROM SmartCrm.dbo.Split(@To,',')

	INSERT INTO @Recipients
	SELECT C.ContactID, TA.ToAddress FROM SmartCRM.dbo.Contacts (NOLOCK) C
	INNER JOIN SmartCRM.dbo.ContactEmails CE (NOLOCK) ON CE.ContactID = C.ContactID
	INNER JOIN @TOAddresses TA ON TA.ToAddress = CE.Email
	WHERE C.IsDeleted = 0 AND CE.IsDeleted = 0 AND CE.IsPrimary = 1 AND C.AccountID = @AccountID


	--Default Merge Codes
	INSERT INTO @FieldList
	SELECT 'EMAILID', 'S', 0

	SET @FindStr = CHARINDEX('*|', @HTMLContent )
	--PRINT @FindStr

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

	
	CREATE TABLE #contactFields (ContactId INT, FieldCode VARCHAR(10), FieldValue VARCHAR(200), Email Varchar(100))

	INSERT INTO #contactFields
	SELECT	c.ContactID CONTACTID, x.MergeCode, x.MergeValue, r.Email
	FROM	SmartCRM.dbo.Contacts (NOLOCK) c
			INNER JOIN SmartCRM.dbo.ContactEmails (NOLOCK) ce on ce.ContactID = c.ContactID AND C.AccountID = CE.AccountID
			INNER JOIN @Recipients r ON R.Id = C.ContactID
			LEFT JOIN SmartCRM.dbo.Communications (NOLOCK) cm on cm.CommunicationID = c.CommunicationID
			LEFT JOIN (
				select	ContactID, a.*
				from	SmartCRM.dbo.ContactAddressMap (NOLOCK) cam inner JOIN SmartCRM.dbo.Addresses (NOLOCK) a on cam.AddressID = a.AddressID and a.IsDefault = 1
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
						('COUNTRY', CN.CountryName)
			) as x (MergeCode, MergeValue)
			WHERE C.AccountID = @AccountID

			
			INSERT INTO #contactFields
			SELECT	ContactID, CAST(CustomFieldID AS VARCHAR) + FieldType + 'F' FieldName, FieldValue, R.Email
			FROM	SmartCrm.dbo.GET_ContactCustomField_Values cfv 
			inner join @FieldList fl on fl.FieldID = cfv.CustomFieldID
			INNER JOIN @Recipients R ON R.Id = cfv.ContactID
			WHERE	FieldType IN ( 'C' )
			
			INSERT INTO #contactFields
			SELECT	cdv.ContactID, CAST(CustomFieldID AS VARCHAR) + FieldType + 'F' FieldName, FieldValue, R.Email
			FROM	SmartCrm.dbo.GET_DropdownFieldValues cdv 
			inner join @FieldList fl on fl.FieldID = cdv.CustomFieldID
			INNER JOIN @Recipients R ON R.Id = cdv.ContactID
			WHERE	AccountID = @AccountID 
					AND FieldType IN ( 'D' )
					AND SortOrder = 1

			--Add link urls
			DECLARE @imageDomain VARCHAR(MAX) = ''

			SELECT @imageDomain = ID.ImageDomain FROM SmartCRM.dbo.ImageDomains ID (NOLOCK)
			INNER JOIN SmartCRM.dbo.ServiceProviders SD (NOLOCK) ON SD.ImageDomainID = ID.ImageDomainID
			WHERE SD.AccountID = @AccountID AND SD.LoginToken = @tokenGuid
			
			SET @imageDomain = REPLACE(@imageDomain,'www.','_')
			SET @imageDomain = REPLACE(@imageDomain,'http://','_')
			SET @imageDomain = REPLACE(@imageDomain,'https://','_')

			IF (LEN(@imageDomain)-LEN(REPLACE(@imageDomain,'.','') )) = 1
				BEGIN
					SELECT @imageDomain = @accountDomain + '.' + RIGHT(@imageDomain, LEN(@imageDomain)-CHARINDEX('_',@imageDomain))
				END
			ELSE
				BEGIN
					SET @imageDomain = @accountUrl
				END
	
			SELECT CF.* FROM #contactFields CF
			INNER JOIN @FieldList F ON F.MergeCode = CF.FieldCode
			UNION
			SELECT r.Id,'LINK'+CAST(LinkIndex AS VARCHAR(250)), 'https://'+ @imageDomain +'/emailtrack?c='+ CAST(r.Id AS VARCHAR) +'&l=' + CAST(EmailLinkID AS VARCHAR(250)) +'&a=' + CAST(@AccountID AS VARCHAR(250)), R.Email
			FROM EmailLinks (NOLOCK) 
			CROSS APPLY @Recipients r
			WHERE SentMailDetailID = @SentMailDetailID
			UNION
			SELECT r.Id, 'PTX', 'https://'+ @imageDomain +'/emailtrack?c='+ CAST(r.Id AS VARCHAR) +'&a='+CAST(@AccountID AS VARCHAR(250)) +'&S=' + CAST(@SentMailDetailID AS VARCHAR(250)), R.Email FROM @Recipients r
			
END


/*
	exec [dbo].[GetSendMailMergfieldValues] 104775

 */


