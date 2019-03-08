




CREATE PROCEDURE [dbo].[getCampaignRecipientInfo_test](
	@CampaignID int,
	@Debug bit = 0
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @HTMLContent nvarchar(max),
			@currentPos bigint = 0,
			@ContentLen bigint,
			@FindStr int = 0,
			@EndIndex int = 0,
			@Searchstring varchar(100),
			@checkDuplicate int = 0,
			@FieldType varchar(1),
			@FieldID int,
			@AccountID int

	declare @FieldList CustomFieldList --table ( MergeCodeID int identity primary key, MergeCode varchar(100), [FieldType] varchar(1), FieldID int )

	select	@HTMLContent = HTMLContent, @ContentLen = DATALENGTH(HTMLContent), @AccountID = AccountID
	from	Campaigns
	where	campaignid = @CampaignID

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

				IF( @FieldType = 'D' )
				BEGIN
					IF( @Debug = 1 )
					BEGIN
						SELECT	*
						FROM	dbo.GET_PhoneFields
						WHERE	CustomFieldID = @FieldID
					END
				END
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

	DECLARE @CFList nvarchar(4000),
			@SFList nvarchar(4000)

	SELECT	@CFList = ISNULL(@CFList, '') + '[' + MergeCode + '], '
	FROM	@FieldList
	WHERE	FieldType IN ( 'C', 'D' )

	SET @CFList = LEFT(@CFList, LEN(@CFList)-1)

	--PRINT @CFList

	SELECT	@SFList = ISNULL(@SFList, '') + '[' + MergeCode + '], '
	FROM	@FieldList
	WHERE	FieldType = 'S'
			AND MergeCode NOT IN ('EMAILID', 'CRID', 'CAMPID' )

	SET @SFList = LEFT(@SFList, LEN(@SFList)-1)

	--PRINT @SFList

	DECLARE @SQL nvarchar(MAX)

	SET @SQL = N'
		SELECT	ContactID2 CONTACTID, CRID, EMAILID'
	IF( LEN(ISNULL(@SFList, '')) > 0 )
	BEGIN
		SET @SQL =	@SQL + ', 
				' + ISNULL(@SFList, '')
	END
	IF( LEN(ISNULL(@CFList, '')) > 0 )
	BEGIN
		SET @SQL =	@SQL + ', 
				' + ISNULL(@CFList, '')
	END

	SET @SQL =	@SQL + N'
		FROM	(
					select	c.ContactID ContactID2, cr.CampaignRecipientID CRID, c.FirstName, c.LastName, c.Company, c.Title, ce.Email EmailID,
							cm.FacebookUrl FBURL, cm.LinkedInUrl LINKEDURL, cm.GooglePlusUrl GPLUSURL, cm.TwitterUrl TWITERURL,
							cm.WebSiteUrl WEBSITEURL, cm.BlogUrl BLOGURL,
							a.AddressLine1 ADDLINE1, a.AddressLine2 ADDLINE2, a.City, s.StateName [State], a.ZipCode, cn.CountryName Country'
	IF( LEN(ISNULL(@CFList, '')) > 0 )
	BEGIN
		SET @SQL =	@SQL + ',
							tmp.*'
	END
							
	SET @SQL =	@SQL + N'
					from	dbo.CampaignRecipients cr inner join dbo.Contacts c on c.ContactID = cr.ContactID AND C.AccountID = CR.AccountId
							left join dbo.ContactEmails ce on ce.ContactID = c.ContactID AND CE.AccountID = C.AccountID and ce.IsPrimary = 1
							left join dbo.Communications cm on cm.CommunicationID = c.CommunicationID
							left join (
								select	ContactID, a.*, RANK() OVER(PARTITION BY ContactID ORDER BY ContactAddressMapID DESC) SortOrder
								from	dbo.ContactAddressMap cam inner join dbo.Addresses a on cam.AddressID = a.AddressID and a.IsDefault = 1
							) a on a.ContactID = c.ContactID and a.SortOrder = 1
							left join dbo.states s on s.StateID = a.StateID
							left join dbo.Countries cn on cn.CountryID = a.CountryID'

	IF( LEN(ISNULL(@CFList, '')) > 0 )
	BEGIN
		SET @SQL =	@SQL + '
							left join (
								SELECT	ContactID, ' + ISNULL(@CFList, '') + '
								FROM	(
											SELECT	ContactID, CAST(CustomFieldID AS VARCHAR) + FieldType + ''F'' FieldName, FieldValue
											FROM	GET_ContactCustomField_Values_test cfv inner join @FieldList fl on fl.FieldID = cfv.CustomFieldID
											WHERE	FieldType IN ( ''C'' )
											UNION ALL
											SELECT	cdv.ContactID, CAST(CustomFieldID AS VARCHAR) + FieldType + ''F'' FieldName, FieldValue
											FROM	GET_DropdownFieldValues cdv inner join @FieldList fl on fl.FieldID = cdv.CustomFieldID
											WHERE	AccountID = @AccountID 
													AND FieldType IN ( ''D'' )
													AND SortOrder = 1
										) tmp
								PIVOT(
									MAX(FieldValue)
									FOR FieldName IN ( ' + ISNULL(@CFList, '') + ' )
								) P
							) tmp on tmp.ContactID = c.ContactID'
	END
	SET @SQL =	@SQL + N'
					WHERE	CampaignID = @CampaignID
				)tmp '

	PRINT @SQL

	EXEC sp_executesql @SQL, N'@FieldList CustomFieldList READONLY, @CampaignID int, @AccountID int', @FieldList, @CampaignID, @AccountID

/*
	SELECT	ContactID2 CONTACTID, CRID, EMAILID,
			[FIRSTNAME], [LASTNAME], [COMPANY], [TITLE], [FBURL], [ADDLINE1], [ADDLINE2], [CITY], [STATE],
			[2392CF], [2397CF], [2398CF], [2394CF], [2393CF], [2399CF], [2395CF], [2401CF]
	FROM	(
				select	c.ContactID ContactID2, cr.CampaignRecipientID CRID, c.FirstName, c.LastName, c.Company, c.Title, ce.Email EmailID,
						cm.FacebookUrl FBURL, cm.LinkedInUrl LINKEDURL, cm.GooglePlusUrl GPLUSURL, cm.TwitterUrl TWITERURL,
						cm.WebSiteUrl WEBSITEURL, cm.BlogUrl BLOGURL,
						a.AddressLine1 ADDLINE1, a.AddressLine2 ADDLINE2, a.City, s.StateName [State], a.ZipCode, cn.CountryName Country,
						tmp.*
				from	dbo.CampaignRecipients cr inner join dbo.Contacts c on c.ContactID = cr.ContactID
						left join dbo.ContactEmails ce on ce.ContactID = c.ContactID and ce.IsPrimary = 1
						left join dbo.Communications cm on cm.CommunicationID = c.CommunicationID
						left join (
							select	top 5 ContactID, a.*
							from	dbo.ContactAddressMap cam inner join dbo.Addresses a on cam.AddressID = a.AddressID and a.IsDefault = 1
						) a on a.ContactID = c.ContactID
						left join dbo.states s on s.StateID = a.StateID
						left join dbo.Countries cn on cn.CountryID = a.CountryID
						left join (
							SELECT	ContactID, [2392CF], [2397CF], [2398CF], [2394CF], [2393CF], [2399CF], [2395CF], [2401CF]
							FROM	(
										SELECT	ContactID, CAST(CustomFieldID AS VARCHAR) + FieldType + 'F' FieldName, FieldValue
										FROM	GET_ContactCustomField_Values cfv inner join @FieldList fl on fl.FieldID = cfv.CustomFieldID
										WHERE	FieldType IN ( 'D', 'C')
									) tmp
							PIVOT(
								MAX(FieldValue)
								FOR FieldName IN ( [2392CF], [2397CF], [2398CF], [2394CF], [2393CF], [2399CF], [2395CF], [2401CF] )
							) P
						) tmp on tmp.ContactID = c.ContactID
				WHERE	CampaignID = @CampaignID
			)tmp
*/

	SET NOCOUNT OFF
END





