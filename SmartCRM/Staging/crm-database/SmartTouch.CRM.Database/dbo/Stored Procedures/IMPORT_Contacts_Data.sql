
CREATE PROCEDURE [dbo].[IMPORT_Contacts_Data]
	(
		@ImportData					AS dbo.IMPORT_ContactDetails readonly,
		@LeadAdapterDetails			AS dbo.LeadAdapterDetails readonly,
		@filename					varchar(300),
		@GUID						uniqueidentifier,
		@FileUpload					tinyint,
		@AccountID					int,
		@LeadAdapterAndAccountMapID int,
		@ErrorStatus				tinyint,
		@LeadAdapterJobLogID		int,
		@OwnerID					int
	)
AS
BEGIN 
	SET NOCOUNT ON

	DECLARE @CustomFields		TABLE (InputData NVARCHAR(1000))
	DECLARE @LeadAdapterTags	TABLE (TagID int)
	DECLARE @UpdateImportTags	TABLE (TagID int)
	DECLARE @PhoneNumbers		TABLE (PhoneNoData VARCHAR(100))
	DECLARE @Companies			TABLE (CompanyID int, CompanyName VARCHAR(200),REFERENCEID UNIQUEIDENTIFIER)
	DECLARE @InputImportData	TABLE ( FirstName nvarchar(75) NULL, LastName nvarchar(75) NULL, CompanyName nvarchar(75) NULL, Title nvarchar(100) NULL, LeadSource nvarchar(100) NULL, 
										LifecycleStage varchar(50) NULL, PartnerType varchar(50) NULL, DoNotEmail bit NULL, HomePhone varchar(20) NULL, MobilePhone varchar(20) NULL, WorkPhone varchar(20) NULL,
										AccountID int NULL, PrimaryEmail varchar(256) NULL, SecondaryEmails nvarchar(2100) NULL, FacebookUrl nvarchar(2000) NULL, TwitterUrl nvarchar(2000) NULL,
										GooglePlusUrl nvarchar(2000) NULL, LinkedInUrl nvarchar(2000) NULL, BlogUrl nvarchar(2000) NULL, WebSiteUrl nvarchar(2000) NULL, AddressLine1 varchar(95) NULL,
										AddressLine2 varchar(95) NULL, City varchar(35) NULL, State nvarchar(64) NULL, Country nvarchar(64) NULL, ZipCode varchar(11) NULL, IsDefault bit NULL,
										ImportDataId int NULL, LeadAdapterRecordStatusId int NULL, ReferenceId uniqueidentifier NULL, ContactID int NULL, IsDeleted bit NULL,
										BuilderNumber nvarchar(75) NULL, CustomFieldsData nvarchar(max) NULL, ImportDataId1 int NULL, PhoneData nvarchar(2000) NULL,EmailStatus smallint NULL)

	DECLARE @Count			int,
			@recordStatus	int,		
			@LookUpCount	int,
			@ContactID		int,	
			@CommID			int,
			@noOfRecords	int,
			@SuccessRate	int,
			@JobStatus		int,
			@PhoneType		smallint,			
			@DupUpdate		int,
			@DuplicateLogic tinyint,
			@UserID			int,
			@CFInputData	NVARCHAR(MAX),
			@CFieldName		NVARCHAR(100),
			@FieldID		int,
			@CFValue		NVARCHAR(MAX),
			@Index			int,
			@Data			NVARCHAR(MAX),
			@CustCursor		CURSOR,
			@InputData		NVARCHAR(MAX),
			@FieldInputTypeID smallint,
			@CFVID			VARCHAR(2000),
			@LoopCounter	int,
			@AddressID		int,
			@AddressTypeID	int,
			@LeadSouce		varchar(100),
			@LeadSouceID	int,
			@ImportLeadSourceID SMALLINT,
			@OldReferenceID	uniqueidentifier,
			@StateID		varchar(20) = '',
			@State			varchar(100) = '',
			@CountryID		varchar(20) = '',
			@Country		varchar(100) = '',
			@City			varchar(100) = '',
			@AddressLine1	varchar(100) = '',
			@AddressLine2	varchar(100) = '',
			@ZipCode		varchar(20) = '',
			@SourceType		int,
			@LifeCycle		smallint,
			@LeadAdapterServiceStatus smallint,
			@CommunicationID	int,
			@LifecycleStageID	int,
			@PartnerTypeID		int,
			@LifecycleStage		varchar(50),
			@PartnerType		varchar(50),
			@CompanyName		varchar(200),
			@CompanyID			int,
			@PhoneData			nvarchar(2000),
			@IsPrimary			bit,
			@IsPrimaryCount		int,
			@PhoneNumber		nvarchar(2000),
			@PhoneNoTypeID		int,
			@CotactPhoneCount	int,
			@ReferenceID		uniqueidentifier,
			@TaggedBy			int

	IF (@FileUpload = 1)
		BEGIN
			SELECT @AccountID = AccountID, @UserID = ProcessBy, @DupUpdate = UpdateOnDuplicate, 
				@DuplicateLogic = DuplicateLogic, @TaggedBy = ProcessBy
				FROM dbo.ImportDataSettings(NOLOCK)
				WHERE UniqueImportIdentifier = @GUID 

			INSERT INTO @LeadAdapterTags (TagID)
			SELECT TagID FROM dbo.ImportTagMap(NOLOCK)
				WHERE LeadAdapterJobLogID = @LeadAdapterJobLogID
			
			SET @ImportLeadSourceID = (SELECT DropdownValueID FROM dbo.DropdownValues(NOLOCK) WHERE AccountID = @AccountID AND DropdownID = 5 AND DropdownValueTypeID = 40)
		END
	ELSE
		BEGIN
			SELECT @LeadSouceID = LeadSourceType, @TaggedBy = CreatedBy
				FROM dbo.LeadAdapterAndAccountMap (NOLOCK)
				WHERE LeadAdapterAndAccountMapID = @LeadAdapterAndAccountMapID
					
			INSERT INTO @LeadAdapterTags (TagID)
			SELECT TagID FROM dbo.LeadAdapterTagMap(NOLOCK)
				WHERE LeadAdapterID = @LeadAdapterAndAccountMapID
		END


	INSERT INTO @InputImportData ( FirstName, LastName, CompanyName, Title, LeadSource, LifecycleStage, PartnerType, DoNotEmail, HomePhone, MobilePhone, WorkPhone, AccountID, 
			PrimaryEmail, SecondaryEmails, FacebookUrl, TwitterUrl, GooglePlusUrl, LinkedInUrl, BlogUrl, WebSiteUrl, AddressLine1, AddressLine2, City, State, Country, ZipCode,
			IsDefault, ImportDataId, LeadAdapterRecordStatusId, ReferenceId, ContactID, IsDeleted, BuilderNumber, CustomFieldsData, ImportDataId1, PhoneData,EmailStatus)
	SELECT FirstName, LastName, CompanyName, Title, LeadSource, LifecycleStage, PartnerType, DoNotEmail, HomePhone, MobilePhone, WorkPhone, AccountID, PrimaryEmail, SecondaryEmails,
			FacebookUrl, TwitterUrl, GooglePlusUrl, LinkedInUrl, BlogUrl, WebSiteUrl, AddressLine1, AddressLine2, City, State, Country, ZipCode, IsDefault, ImportDataId,
			LeadAdapterRecordStatusId, ReferenceId, ContactID, IsDeleted, BuilderNumber, CustomFieldsData, ROW_NUMBER() OVER(ORDER BY ReferenceId) AS ImportDataId1, PhoneData,EmailStatus
		FROM @ImportData
		    
	

	SET @SourceType = (SELECT LeadAdapterTypeID FROM dbo.LeadAdapterAndAccountMap WHERE LeadAdapterAndAccountMapID = @LeadAdapterAndAccountMapID)
	SET @LifeCycle = (SELECT DropdownValueID FROM dbo.DropdownValues WHERE DropdownID = 3 AND AccountID = @AccountID AND IsDefault = CONVERT(tinyint, 1))

	SET @LoopCounter = 1
	SET @SuccessRate = 0
	SET @noOfRecords = (SELECT COUNT(1) FROM @InputImportData)		

		WHILE (@LoopCounter <= @noOfRecords)
			BEGIN
				BEGIN TRY	
				SET @recordStatus = (SELECT TOP 1 LeadAdapterRecordStatusId FROM @InputImportData WHERE ImportDataId1 = @LoopCounter)
				
				IF(@recordStatus != 1 OR @recordStatus != 3)
					BEGIN
						UPDATE dbo.LeadAdapterJobLogDetails					
							SET RowData = (SELECT RowData FROM @LeadAdapterDetails WHERE ReferenceID = (SELECT ReferenceID FROM @InputImportData WHERE ImportDataId1 = @LoopCounter)),						   
							LeadAdapterRecordStatusID = @recordStatus, CreatedDateTime = GETUTCDATE()
							WHERE LeadAdapterJobLogDetailID = (SELECT TOP 1 LeadAdapterJobLogDetailID FROM dbo.LeadAdapterJobLogDetails WHERE
								 ReferenceID = (SELECT ReferenceID FROM @InputImportData WHERE ImportDataId1 = @LoopCounter) ORDER BY CreatedDateTime DESC)
					END

				SELECT @LifecycleStage = LifecycleStage, @PartnerType = PartnerType, @CompanyName = CompanyName, 
					@ReferenceID = ReferenceID, @CompanyID = NULL, @ContactID = ISNULL(ContactID, 0)
					FROM @InputImportData
					WHERE ImportDataId1 = @LoopCounter

				SET @LifecycleStageID = (SELECT TOP 1 DropdownValueID FROM dbo.DropdownValues 
											WHERE AccountID = @AccountID AND DropdownID = 3 
												AND IsActive = 1 AND DropdownValue = @LifecycleStage)

				IF (ISNULL(@LifecycleStageID, 0) = 0)
					BEGIN
						SET @LifecycleStageID = (SELECT TOP 1 DropdownValueID FROM dbo.DropdownValues 
							WHERE AccountID = @AccountID AND DropdownID = 3 
								AND IsActive = 1 AND IsDefault = 1)	
					END
														
				SET @PartnerTypeID = (SELECT TOP 1 DropdownValueID FROM dbo.DropdownValues (NOLOCK)
											WHERE AccountID = @AccountID AND DropdownID = 4 
												AND IsActive = 1 AND DropdownValue = @PartnerType)
				
				IF (((LEN(@CompanyName) > 0 AND @ContactID > 0 AND @DupUpdate = 1) OR (LEN(@CompanyName) > 0 AND @ContactID = 0)) AND @recordStatus IN (1,3))
					BEGIN

						SET @CompanyID = (SELECT TOP 1 ContactID FROM dbo.Contacts(NOLOCK)
											WHERE AccountID = @AccountID AND Company = @CompanyName AND ContactType = 2 AND IsDeleted = 0)
				
						IF (ISNULL(@CompanyID, 0) = 0)
							BEGIN
								DECLARE @UNIQUECOMPANYID UNIQUEIDENTIFIER = NEWID()

								INSERT INTO dbo.Contacts(Company,ReferenceID, AccountID, ContactType, IsDeleted, LastUpdatedOn, ContactSource, SourceType,FirstContactSource,FirstSourceType, OwnerID) 
								SELECT @CompanyName,@UNIQUECOMPANYID,@AccountID, 2, 0, GETUTCDATE(),
									CASE WHEN @FileUpload = 1 THEN 2
										WHEN @FileUpload = 2 THEN 1 ELSE NULL END, @SourceType,CASE WHEN @FileUpload = 1 THEN 2
										WHEN @FileUpload = 2 THEN 1 ELSE 4 END,@SourceType, @OwnerID 	 
									
								SET @CompanyID = SCOPE_IDENTITY()

								INSERT INTO @Companies (CompanyID, CompanyName, REFERENCEID)
								SELECT @CompanyID, @CompanyName,@UNIQUECOMPANYID

								INSERT INTO dbo.LeadAdapterJobLogDetails (LeadAdapterJobLogID, LeadAdapterRecordStatusID, CreatedBy, CreatedDateTime, RowData, ReferenceID, SubmittedData)
								VALUES (@LeadAdapterJobLogID, 1, @OwnerID, GETUTCDATE(), @CompanyName, @UNIQUECOMPANYID, '')
							END
						ELSE
							BEGIN
								INSERT INTO @Companies (CompanyID, CompanyName, REFERENCEID)
								SELECT ContactID, Company, ReferenceID FROM dbo.Contacts(NOLOCK)
									WHERE AccountID = @AccountID AND Company = @CompanyName AND ContactType = 2 AND IsDeleted = 0
									AND ContactID NOT IN (SELECT CompanyID FROM @Companies)

								--INSERT INTO dbo.LeadAdapterJobLogDetails (LeadAdapterJobLogID, LeadAdapterRecordStatusID, CreatedBy, CreatedDateTime, RowData, ReferenceID, SubmittedData)
								--VALUES (@LeadAdapterJobLogID, 1, @OwnerID, GETUTCDATE(), @CompanyName, @UNIQUECOMPANYID, '')
							END
					END

					IF (@OwnerID = 0)
						BEGIN
							SET @OwnerID = NULL
						END

					/* Insert New Contact Record */
					IF(@recordStatus = 1) 
					BEGIN		
						UPDATE dbo.LeadAdapterJobLogDetails
							SET RowData					= LAD.RowData, 
								LeadAdapterRecordStatusID = 1,
								CreatedDateTime				= GETUTCDATE()			
							FROM dbo.LeadAdapterJobLogDetails LAJD 
							INNER JOIN @LeadAdapterDetails LAD ON LAJD.ReferenceID = LAD.ReferenceID
						WHERE LeadAdapterJobLogDetailID = (SELECT TOP 1 LeadAdapterJobLogDetailID FROM dbo.LeadAdapterJobLogDetails WHERE
								 ReferenceID = (SELECT ReferenceID FROM @InputImportData WHERE ImportDataId1 = @LoopCounter) ORDER BY CreatedDateTime DESC)
														
						INSERT INTO dbo.Communications (SecondaryEmails,FacebookUrl,TwitterUrl,GooglePlusUrl,LinkedInUrl,BlogUrl,WebSiteUrl)
						SELECT SecondaryEmails,FacebookUrl,TwitterUrl,GooglePlusUrl, LinkedInUrl,BlogUrl,WebSiteUrl 
							FROM @InputImportData
							WHERE ImportDataId1 = @LoopCounter 
							AND	( SecondaryEmails  IS NOT NULL 
								OR FacebookUrl IS NOT NULL
								OR TwitterUrl IS NOT NULL
								OR GooglePlusUrl IS NOT NULL
								OR LinkedInUrl IS NOT NULL
								OR BlogUrl IS NOT NULL
								OR WebSiteUrl IS NOT NULL )
									 
						SET @CommunicationID = (SELECT IDENT_CURRENT('Communications'))
					
					INSERT INTO dbo.Contacts(FirstName, LastName, Company, CommunicationID, Title, AccountID, LeadSource, PrimaryEmail, ContactType,
						LifecycleStage, PartnerType, DoNotEmail, IsDeleted, ReferenceId,LastUpdatedOn, ContactSource, SourceType,FirstContactSource,FirstSourceType, OwnerID, CompanyID) 
					SELECT FirstName ,LastName ,CompanyName, @CommunicationID, Title, AccountID, 0, PrimaryEmail, 1, 
						@LifecycleStageID, @PartnerTypeID, DoNotEmail, 0, ReferenceId, GETUTCDATE(),
						CASE WHEN @FileUpload = 1 THEN 2
							 WHEN @FileUpload = 2 THEN 1 ELSE 4 END, @SourceType,
							 CASE WHEN @FileUpload = 1 THEN 2
							 WHEN @FileUpload = 2 THEN 1 ELSE 4 END,
							 @SourceType, @OwnerID, @CompanyID 	 
						FROM @InputImportData
						WHERE ImportDataId1 = @LoopCounter

					SET @ContactID = SCOPE_IDENTITY()
					
					INSERT INTO dbo.ContactEmails (ContactID, Email, EmailStatus, IsPrimary, AccountID,IsDeleted)
					SELECT @ContactID, PrimaryEmail, EmailStatus, 1, AccountID,0
						FROM @InputImportData 
						WHERE ImportDataId1 = @LoopCounter AND LEN(ISNULL(PrimaryEmail, 0)) > 1

                   EXEC dbo.INSERT_UPDATE_Contact_Data
						@ContactID		= @ContactID,
						@ActionFlag		= 1,
						@ReturnFlag		= 1
					
					DELETE FROM @CustomFields
					SET @Index			=	''
					SET @CFInputData	= ''
					SET	@Data			= ''

					/* Contact Custom Fields Imports Data */
					SET @CFInputData = (SELECT CustomFieldsData FROM @InputImportData
										WHERE ImportDataId1 = @LoopCounter AND LEN(CustomFieldsData) > 0 )

					SELECT @Index = 1
						WHILE @Index != 0
					BEGIN
						SELECT @Index = CHARINDEX('~', @CFInputData)					
					IF @Index != 0
						BEGIN
							SELECT @Data = LEFT(@CFInputData, @Index-1)
						END
					ELSE
						BEGIN
							SELECT @Data = @CFInputData
						END
	
					INSERT INTO @CustomFields (InputData)
						VALUES( @Data )	
	
					SELECT @CFInputData = RIGHT(@CFInputData, LEN(@CFInputData) - @Index)
	
					IF LEN(@CFInputData) = 0
					BEGIN BREAK
						END
					END

					SET @CustCursor = CURSOR
						FOR SELECT InputData FROM @CustomFields
					OPEN @CustCursor
					FETCH NEXT FROM @CustCursor INTO @InputData
					WHILE @@Fetch_Status = 0
						BEGIN						 			
							SET @FieldID = 0
							SET @FieldInputTypeID = 0
							SET @CFVID = 0
							SET @CFValue = ''
							SET @Count	= 0

							SELECT @CFieldName	= REPLACE(SUBSTRING(@InputData, 1, CHARINDEX('|', @InputData)-1), ' ', '')
							SELECT @CFValue		= SUBSTRING(@InputData, CHARINDEX('|', @InputData)+1, LEN(@InputData))

							SET @FieldID = REPLACE(SUBSTRING(@CFieldName, 1, CHARINDEX('##$##', @CFieldName)-1), ' ', '')
							SET @FieldInputTypeID = SUBSTRING(@CFieldName, CHARINDEX('##$##', @CFieldName)+5, LEN(@CFieldName))	

							IF (ISNULL(@FieldID,0) != 0 AND @FieldInputTypeID IN (1,6,11,12))
								BEGIN
									IF (@FieldInputTypeID IN (6,11))
										BEGIN
											SET @CFVID = ISNULL(CONVERT(VARCHAR(15),(SELECT CustomFieldValueOptionID FROM dbo.CustomFieldValueOptions
															WHERE CustomFieldID = @FieldID AND Value = @CFValue AND IsDeleted = 0)), '')
								
											SET @Count = ISNULL((SELECT COUNT(1) FROM dbo.ContactCustomFieldMap WHERE ContactID = @ContactID AND CustomFieldID = @FieldID), 0)
											IF (@Count = 0 AND LEN(@CFVID) > 0)
												BEGIN
													INSERT INTO dbo.ContactCustomFieldMap (ContactID, CustomFieldID, Value)
													SELECT @ContactID, @FieldID, @CFVID	
												END
											ELSE
												BEGIN
													UPDATE dbo.ContactCustomFieldMap
														SET Value = @CFVID
														WHERE ContactID = @ContactID AND CustomFieldID = @FieldID
												END
										END
									ELSE IF (@FieldInputTypeID IN (1,12))
										BEGIN
											DECLARE @CFValueList TABLE (ID int IDENTITY(1,1), Value NVARCHAR(MAX))
											DECLARE @I			int = 1,
													@CFNewValue	NVARCHAR(MAX) = '',
													@CFVIDs		NVARCHAR(MAX) = ''										

											INSERT INTO @CFValueList (Value)
											SELECT DISTINCT RTRIM(LTRIM(DataValue)) FROM dbo.Split(@CFValue, ',') WHERE LEN(DataValue) > 0

											DECLARE @LoopValueCount int = (SELECT MAX(ID) FROM @CFValueList)

											WHILE (@I <= @LoopValueCount)
												BEGIN
													SET @CFNewValue = (SELECT Value FROM @CFValueList WHERE ID = @I)

													SET @CFVID = ISNULL(CONVERT(VARCHAR(15),(SELECT CustomFieldValueOptionID FROM dbo.CustomFieldValueOptions
																	WHERE CustomFieldID = @FieldID AND Value = @CFNewValue AND IsDeleted = 0)), '')
													
													SET @CFVIDs = case when @CFVID = '' then @CFVIDs + @CFVID + '' else  @CFVIDs + @CFVID + '|'	end				
													SET @I = @I + 1
												
												END

												SET @Count = ISNULL((SELECT COUNT(1) FROM dbo.ContactCustomFieldMap WHERE ContactID = @ContactID AND CustomFieldID = @FieldID), 0)
													IF (@Count = 0 AND LEN(@CFVIDs) > 0)
														BEGIN
															INSERT INTO dbo.ContactCustomFieldMap (ContactID, CustomFieldID, Value)
															SELECT @ContactID, @FieldID, @CFVIDs	
														END
													ELSE
														BEGIN
															UPDATE dbo.ContactCustomFieldMap
																SET Value = @CFVIDs
																WHERE ContactID = @ContactID AND CustomFieldID = @FieldID
														END
												DELETE FROM  @CFValueList
										END
								END
							ELSE IF (ISNULL(@FieldID,0) != 0 AND @FieldInputTypeID NOT IN (1,6,11,12))
								BEGIN									
									SET @Count = ISNULL((SELECT COUNT(1) FROM dbo.ContactCustomFieldMap WHERE ContactID = @ContactID AND CustomFieldID = @FieldID), 0)
									IF (@Count = 0 AND LEN(@CFValue) > 0)
										BEGIN
											INSERT INTO dbo.ContactCustomFieldMap (ContactID, CustomFieldID, Value)
											SELECT @ContactID, @FieldID, @CFValue
										END
									ELSE
										BEGIN
											UPDATE dbo.ContactCustomFieldMap
												SET Value = @CFValue
												WHERE ContactID = @ContactID AND CustomFieldID = @FieldID
										END
								END

						FETCH NEXT FROM @CustCursor INTO @InputData
					END
					CLOSE @CustCursor
					DEALLOCATE @CustCursor


					/* Adding tags to the contacts */
					INSERT INTO dbo.ContactTagMap (ContactID, TagID, TaggedBy, TaggedOn, AccountID)
					SELECT @ContactID, TagID, @TaggedBy, GETUTCDATE(), @AccountID
						FROM @LeadAdapterTags



					IF (@FileUpload = 1)
						BEGIN
							/* Contact LeadSource Data */
							SET @LeadSouce = (SELECT LeadSource FROM @InputImportData WHERE ImportDataId1 = @LoopCounter AND LEN(LeadSource) > 0)
							SET @LeadSouceID = (SELECT DropdownValueID FROM dbo.DropdownValues (NOLOCK)
													WHERE DropdownID = 5 AND IsActive = 1 AND AccountID = @AccountID AND DropdownValue = @LeadSouce) 

							IF (@LeadSouceID > 0)
								BEGIN	
									INSERT INTO dbo.ContactLeadSourceMap (ContactID, LeadSouceID, IsPrimaryLeadSource)
									VALUES (@ContactID, @LeadSouceID, 1)	
								END
							ELSE
								BEGIN 
									INSERT INTO dbo.ContactLeadSourceMap (ContactID,LeadSouceID,IsPrimaryLeadSource)
									VALUES (@ContactID,@ImportLeadSourceID,1)
								END
						END
					ELSE
						BEGIN								
							IF (@LeadSouceID > 0)
								BEGIN
									INSERT INTO dbo.ContactLeadSourceMap (ContactID, LeadSouceID, IsPrimaryLeadSource)
									VALUES (@ContactID, @LeadSouceID, 1)	
								END
						END
					
					SET @State			= ''
					SET @Country		= ''
					SET @AddressLine1	= ''
					SET	@AddressLine2	= ''
					SET @ZipCode		= ''
					SET @City			= ''


					/* Contact Address Data */
						SELECT @State = [State], @Country = Country, @AddressLine1 = AddressLine1,
							   @AddressLine2 = AddressLine2, @ZipCode = ZipCode, @City = City
							FROM @InputImportData WHERE ImportDataId1 = @LoopCounter
			
					IF((LEN(@State) > 0 AND @State IS NOT NULL) OR (LEN(@Country) > 0 AND @Country IS NOT NULL) 
						OR (LEN(@AddressLine1) > 0 AND @AddressLine1 IS NOT NULL)
						OR (LEN(@AddressLine2) > 0 AND @AddressLine2 IS NOT NULL)
						OR (LEN(@City) > 0 AND @City IS NOT NULL)
						OR (LEN(@ZipCode) > 0) AND @ZipCode IS NOT NULL)
					BEGIN											
						SET @StateID = (SELECT StateID FROM dbo.States WHERE StateID = @State OR StateName = @State OR RIGHT(StateID,LEN(StateID)-3) = @State)
						SET @CountryID = (SELECT CountryID FROM dbo.Countries WHERE CountryID = @Country OR CountryName = @Country)						
					
										
						IF(@StateID IS NOT NULL AND LEN(@StateID) > 0 AND @CountryID IS NULL AND LEN(@CountryID) > 0)
							BEGIN
								SET @CountryID =  (SELECT CountryID FROM dbo.States WHERE StateID = @StateID)								
							END
													
						SET @AddressTypeID = (SELECT TOP 1 DropdownValueID FROM dbo.DropdownValues
												WHERE DropdownID = 2 AND IsActive = 1 AND AccountID = @AccountID AND IsDefault = 1)	

						IF(( (@State IS NULL OR LEN(@State) = 0) AND (@Country IS NULL OR LEN(@Country) = 0) AND (@City IS NULL OR LEN(@City) = 0) AND LEN(@ZipCode) > 0 AND @ZipCode IS NOT NULL))	
							BEGIN													
								SELECT TOP 1 @StateID = CASE WHEN CountryID = 1 THEN 'US-' + [STATECODE]
														   WHEN CountryID = 2 THEN 'CA-'+[STATECODE]
														   ELSE NULL END,
											   @CountryID = CASE WHEN CountryID = 1 THEN 'US'
															WHEN CountryID = 2 THEN 'CA'
															ELSE NULL END,
											   @City = CityName	 FROM dbo.TaxRates
											   WHERE ZIPCode = REPLACE(@ZipCode,' ','') 																			
							END

						IF (ISNULL(@AddressTypeID, 0) = 0)
							BEGIN
								SET @AddressTypeID = (SELECT TOP 1 DropdownValueID FROM dbo.DropdownValues
														WHERE DropdownID = 2 AND IsActive = 1 AND AccountID = @AccountID)	
							END

						INSERT INTO dbo.Addresses (AddressTypeID, AddressLine1, AddressLine2, City, StateID, CountryID, ZipCode, IsDefault) 
						SELECT @AddressTypeID, @AddressLine1, @AddressLine2, @City, LTRIM(RTRIM(@StateID)), LTRIM(RTRIM(@CountryID)), @ZipCode, 1 
							FROM @InputImportData
							WHERE ImportDataId1 = @LoopCounter
						
						SET @AddressID = (SELECT IDENT_CURRENT('Addresses'))

						INSERT INTO dbo.ContactAddressMap (ContactID, AddressID)
						VALUES  (@ContactID, @AddressID) 							
					END
				
					/* Contact Phone Numbers Imports Data */
					SET @PhoneData = (SELECT PhoneData FROM @InputImportData
										WHERE ImportDataId1 = @LoopCounter AND LEN(PhoneData) > 0)
					
					IF (LEN(@PhoneData) > 0)
					BEGIN

						SELECT @Index = 1
							WHILE @Index != 0
						BEGIN
							SELECT @Index = CHARINDEX('~', @PhoneData)					
						IF @Index != 0
							BEGIN
								SELECT @Data = LEFT(@PhoneData, @Index-1)
							END
						ELSE
							BEGIN
								SELECT @Data = @PhoneData
							END
	
						INSERT INTO @PhoneNumbers (PhoneNoData)
							VALUES( @Data )	
	
						SELECT @PhoneData = RIGHT(@PhoneData, LEN(@PhoneData) - @Index)
	
						IF LEN(@PhoneData) = 0
						BEGIN BREAK
							END
						END
				
					SET @IsPrimaryCount = (SELECT COUNT(1) FROM dbo.ContactPhoneNumbers (NOLOCK)
											WHERE ContactID = @ContactID AND IsPrimary = 1 AND IsDeleted = 0)
				
					IF (@IsPrimaryCount = 0)
						BEGIN
							SET @IsPrimary = 1
						END
					ELSE
						BEGIN
							SET @IsPrimary = 0
						END

					INSERT INTO dbo.ContactPhoneNumbers (ContactID, PhoneNumber, PhoneType, IsPrimary, AccountID,IsDeleted)
					SELECT @ContactID, SUBSTRING(PhoneNoData, CHARINDEX('|', PhoneNoData)+1, LEN(PhoneNoData)),
						SUBSTRING(PhoneNoData, 1, CHARINDEX('|', PhoneNoData)-1), @IsPrimary, @AccountID,0
						FROM @PhoneNumbers
						WHERE LEN(PhoneNoData) > 0
				
						DELETE FROM @PhoneNumbers
					END
				END

				ELSE IF(@recordStatus = 3) 
				BEGIN

					IF (LEN(@CompanyName) = 0)
						BEGIN
							SELECT TOP 1 @CompanyID = CompanyID
								FROM dbo.Contacts 
								WHERE ContactID = (SELECT ContactID FROM @InputImportData WHERE ImportDataId1 = @LoopCounter)
						END

					SELECT TOP 1 @CommID = CommunicationID, @OldReferenceID = ReferenceID, @ReferenceID = ReferenceID
						FROM dbo.Contacts (NOLOCK)
						WHERE ContactID = (SELECT ContactID FROM @InputImportData WHERE ImportDataId1 = @LoopCounter)


					UPDATE dbo.LeadAdapterJobLogDetails
						SET ReferenceID = (SELECT ReferenceID FROM dbo.Contacts WHERE ContactID = @ContactID),
							CreatedDateTime = GETUTCDATE(),
							SubmittedData = (SELECT SubmittedData FROM @LeadAdapterDetails WHERE ReferenceID = (SELECT ReferenceID FROM @InputImportData WHERE ImportDataId1 = @LoopCounter)),
							RowData = (SELECT RowData FROM @LeadAdapterDetails WHERE ReferenceID = (SELECT ReferenceID FROM @InputImportData WHERE ImportDataId1 = @LoopCounter)),						   
							LeadAdapterRecordStatusID = 3
							WHERE LeadAdapterJobLogDetailID = (SELECT TOP 1 LeadAdapterJobLogDetailID FROM dbo.LeadAdapterJobLogDetails WHERE
								 ReferenceID = (SELECT ReferenceID FROM @InputImportData WHERE ImportDataId1 = @LoopCounter) ORDER BY CreatedDateTime DESC)
								
					UPDATE dbo.Contacts 
					SET FirstName		= B.FirstName, 
						LastName		= B.LastName,
						Company			= B.CompanyName,
						Title			= B.Title,
						AccountID		= B.AccountID,
						HomePhone		= B.HomePhone,
						WorkPhone		= B.WorkPhone,
						MobilePhone		= B.MobilePhone,
						PrimaryEmail	= B.PrimaryEmail, 
						LifecycleStage  = @LifecycleStageID,
						PartnerType		= @PartnerTypeID,
						DoNotEmail		= B.DoNotEmail,
						IsDeleted		= 0,
						ContactSource	= CASE WHEN @FileUpload = 1 THEN 2
							 WHEN @FileUpload = 2 THEN 1 ELSE NULL END,
						LastUpdatedOn	= GETUTCDATE(),					
						SourceType		= @SourceType,						
						OwnerID			= @OwnerID,
						CompanyID		= @CompanyID
					FROM dbo.Contacts A INNER JOIN @InputImportData B ON A.ContactID = B.ContactID AND A.AccountID = B.AccountID
					WHERE B.ImportDataId1 = @LoopCounter 
						AND A.ContactID = B.ContactID 
					
					SELECT @ContactID = (SELECT ContactID FROM @InputImportData WHERE ImportDataId1 = @LoopCounter)

					  EXEC dbo.INSERT_UPDATE_Contact_Data
							@ContactID		= @ContactID,
							@ActionFlag		= 2,
							@ReturnFlag		= 1

					/* Contact LeadSource Data */
					SET @LeadSouce = (SELECT LeadSource FROM @InputImportData WHERE ImportDataId1 = @LoopCounter AND LEN(LeadSource) > 0)

					SET @LeadSouceID = (SELECT TOP 1 DropdownValueID FROM dbo.DropdownValues 
											WHERE AccountID = @AccountID AND DropdownID = 5 AND IsActive = 1 AND DropdownValue = @LeadSouce) 
					
					/* Adding tags to the contacts */
					INSERT INTO dbo.ContactTagMap (ContactID, TagID, TaggedBy, TaggedOn, AccountID)
					SELECT @ContactID, TagID, @TaggedBy, GETUTCDATE(), @AccountID
						FROM @LeadAdapterTags LT
						WHERE LT.TagID NOT IN(SELECT TagID FROM dbo.ContactTagMap(NOLOCK) WHERE ContactID = @ContactID AND AccountID = @AccountID)
					
					IF (@LeadSouceID > 0)
						BEGIN	
						    IF ((SELECT COUNT(1) FROM dbo.ContactLeadSourceMap(NOLOCK) WHERE ContactID = @ContactID AND LeadSouceID = @LeadSouceID) = 0)
								BEGIN
									INSERT INTO dbo.ContactLeadSourceMap (ContactID, LeadSouceID,IsPrimaryLeadSource)
									VALUES (@ContactID, @LeadSouceID,0)	
								END
						END

					DELETE FROM @CustomFields
					SET @CFInputData	= ''
					SET @Data			= ''
					
					/* Contact Custom Fields Imports Data */
					SET @CFInputData = (SELECT CustomFieldsData FROM @InputImportData
										WHERE ImportDataId1 = @LoopCounter AND LEN(CustomFieldsData) > 0 )

					SELECT @Index = 1
						WHILE @Index != 0
					BEGIN
						SELECT @Index = CHARINDEX('~', @CFInputData)					
					IF @Index != 0
						BEGIN
							SELECT @Data = LEFT(@CFInputData, @Index-1)
						END
					ELSE
						BEGIN
							SELECT @Data = @CFInputData
						END
	
					INSERT INTO @CustomFields (InputData)
						VALUES( @Data )	
	
					SELECT @CFInputData = RIGHT(@CFInputData, LEN(@CFInputData) - @Index)
	
					IF LEN(@CFInputData) = 0
					BEGIN BREAK
						END
					END

					SET @CustCursor = CURSOR
						FOR SELECT InputData FROM @CustomFields
					OPEN @CustCursor
					FETCH NEXT FROM @CustCursor INTO @InputData
					WHILE @@Fetch_Status = 0
						BEGIN						 			
							SET @FieldID = 0
							SET @FieldInputTypeID = 0
							SET @CFVID = 0

							SELECT @CFieldName	= REPLACE(SUBSTRING(@InputData, 1, CHARINDEX('|', @InputData)-1), ' ', '')
							SELECT @CFValue		= SUBSTRING(@InputData, CHARINDEX('|', @InputData)+1, LEN(@InputData))
							
							SET @FieldID = REPLACE(SUBSTRING(@CFieldName, 1, CHARINDEX('##$##', @CFieldName)-1), ' ', '')
							SET @Fieldinputtypeid = SUBSTRING(@CFieldName, CHARINDEX('##$##', @CFieldName)+5, LEN(@CFieldName))	

							IF (ISNULL(@FieldID,0) != 0 AND @FieldInputTypeID IN (1,6,11,12))
								BEGIN
									IF (@FieldInputTypeID IN (6,11))
										BEGIN
											SET @CFVID = ISNULL(CONVERT(VARCHAR(15),(SELECT CustomFieldValueOptionID FROM dbo.CustomFieldValueOptions(NOLOCK)
															WHERE CustomFieldID = @FieldID AND Value = @CFValue AND IsDeleted = 0)), '')
								
											SET @Count = ISNULL((SELECT COUNT(1) FROM dbo.ContactCustomFieldMap WHERE ContactID = @ContactID AND CustomFieldID = @FieldID), 0)
											IF (@Count = 0 AND LEN(@CFVID) > 0)
												BEGIN
													INSERT INTO dbo.ContactCustomFieldMap (ContactID, CustomFieldID, Value)
													SELECT @ContactID, @FieldID, @CFVID	
												END
											ELSE
												BEGIN
													UPDATE dbo.ContactCustomFieldMap
														SET Value = @CFVID
														WHERE ContactID = @ContactID AND CustomFieldID = @FieldID
												END
										END
									ELSE IF (@FieldInputTypeID IN (1,12))
										BEGIN
											DECLARE @CFValueList1 TABLE (ID int IDENTITY(1,1), Value NVARCHAR(MAX))
											DECLARE @I1				int = 1,
													@CFNewValue1	NVARCHAR(MAX) = '',
													@CFVIDs1		NVARCHAR(MAX) = ''										

											INSERT INTO @CFValueList1 (Value)
											SELECT DISTINCT RTRIM(LTRIM(DataValue)) FROM dbo.Split(@CFValue, ',') WHERE LEN(DataValue) > 0

											DECLARE @LoopValueCount1 int = (SELECT MAX(ID) FROM @CFValueList1)

											WHILE (@I1 <= @LoopValueCount1)
												BEGIN
													SET @CFNewValue1 = (SELECT Value FROM @CFValueList1 WHERE ID = @I1)

													SET @CFVID = ISNULL(CONVERT(VARCHAR(15),(SELECT CustomFieldValueOptionID FROM dbo.CustomFieldValueOptions(NOLOCK)
																	WHERE CustomFieldID = @FieldID AND Value = @CFNewValue1 AND IsDeleted = 0)), '')
																									
													IF(@CFVID != '')
														BEGIN																					
															SET @CFVIDs1 = @CFVIDs1 + @CFVID +'|'
														END
													SET @I1 = @I1 + 1												
												END

											SET @Count = ISNULL((SELECT COUNT(1) FROM dbo.ContactCustomFieldMap(NOLOCK) WHERE ContactID = @ContactID AND CustomFieldID = @FieldID), 0)
												IF (@Count = 0 AND LEN(@CFVIDs1) > 0)
													BEGIN
														INSERT INTO dbo.ContactCustomFieldMap (ContactID, CustomFieldID, Value)
														SELECT @ContactID, @FieldID, @CFVIDs1	
													END
												ELSE
													BEGIN
														UPDATE dbo.ContactCustomFieldMap
															SET Value = @CFVIDs1
															WHERE ContactID = @ContactID AND CustomFieldID = @FieldID
													END
										END
										DELETE FROM @CFValueList1
								END
							ELSE IF (ISNULL(@FieldID,0) != 0 AND @FieldInputTypeID NOT IN (1,6,11,12))
								BEGIN									
									SET @Count = ISNULL((SELECT COUNT(1) FROM dbo.ContactCustomFieldMap(NOLOCK) WHERE ContactID = @ContactID AND CustomFieldID = @FieldID), 0)
									IF (@Count = 0 AND LEN(@CFValue) > 0)
										BEGIN
											INSERT INTO dbo.ContactCustomFieldMap (ContactID, CustomFieldID, Value)
											SELECT @ContactID, @FieldID, @CFValue
										END
									ELSE
										BEGIN
											UPDATE dbo.ContactCustomFieldMap
												SET Value = @CFValue
												WHERE ContactID = @ContactID AND CustomFieldID = @FieldID
										END
								END

						FETCH NEXT FROM @CustCursor INTO @InputData
					END
					CLOSE @CustCursor
					DEALLOCATE @CustCursor

					SET @State			= ''
					SET @Country		= ''
					SET @AddressLine1	= ''
					SET	@AddressLine2	= ''
					SET @ZipCode		= ''
					SET @City			= ''

					/* Contact Address Data */
					SELECT @State = [STATE], @Country = [COUNTRY],@AddressLine1 = [ADDRESSLINE1],@AddressLine2 = [ADDRESSLINE2],@City=[CITY],@ZipCode = [ZipCode] FROM @InputImportData WHERE ImportDataId1 = @LoopCounter 
					
					IF((LEN(@State) > 0 AND @State IS NOT NULL) OR (LEN(@Country) > 0 AND @Country IS NOT NULL) 
						OR (LEN(@AddressLine1) > 0 AND @AddressLine1 IS NOT NULL)
						OR (LEN(@AddressLine2) > 0 AND @AddressLine2 IS NOT NULL)
						OR (LEN(@City) > 0 AND @City IS NOT NULL)
						OR (LEN(@ZipCode) > 0) AND @ZipCode IS NOT NULL) 
						BEGIN
						
							SET @StateID = (SELECT StateID FROM dbo.States WHERE StateID = @State OR StateName = @State OR RIGHT(StateID,LEN(StateID)-3) = @State)
							SET @CountryID = (SELECT CountryID FROM dbo.Countries WHERE CountryID = @Country OR CountryName = @Country)							

							IF(@StateID IS NOT NULL AND @CountryID IS NULL)
								BEGIN
									SET @CountryID =  (SELECT CountryID FROM dbo.States WHERE StateID = @StateID)							
								END
						
							SET @AddressTypeID = (SELECT TOP 1 DropdownValueID FROM [dbo].[DropdownValues]
													WHERE DropdownID = 2 AND IsActive = 1 AND AccountID = @AccountID AND IsDefault = 1)

						IF( (@State IS NULL OR LEN(@State) = 0) AND (@Country IS NULL OR LEN(@Country) = 0) AND (@City IS NULL OR LEN(@City) = 0) AND (LEN(@ZipCode) > 0 AND @ZipCode IS NOT NULL))	
								BEGIN
									SELECT TOP 1 @StateID = CASE WHEN CountryID = 1 THEN 'US-' + [STATECODE]
													   WHEN CountryID = 2 THEN 'CA-'+[STATECODE]
													   ELSE NULL END,
										   @CountryID = CASE WHEN CountryID = 1 THEN 'US'
														WHEN CountryID = 2 THEN 'CA'
														ELSE NULL END,
										   @City = CityName	 FROM dbo.TaxRates
										   WHERE ZIPCode = REPLACE(@ZipCode,' ','')
								END

							IF (ISNULL(@AddressTypeID, 0) = 0)
								BEGIN
									SET @AddressTypeID = (SELECT TOP 1 DropdownValueID FROM dbo.DropdownValues(NOLOCK)
															WHERE DropdownID = 2 AND IsActive = 1 AND AccountID = @AccountID)	
								END

					 IF(( SELECT COUNT(1) FROM dbo.Addresses A (NOLOCK)
							INNER JOIN dbo.ContactAddressMap(NOLOCK) CAM ON A.AddressID = CAM.AddressID AND CAM.ContactID = @ContactID 
					      WHERE (AddressTypeID = @AddressTypeID)
							AND (AddressLine1 IS NULL OR AddressLine1 = @AddressLine1 AND AddressLine1 = '') 
							AND (AddressLine2 IS NULL OR AddressLine2 = @AddressLine2 AND AddressLine2 = '')
							AND (City IS NULL OR City = @City OR City = '') 
							AND (StateID IS NULL OR StateID = @StateID OR StateID = '') 
							AND (CountryID IS NULL OR CountryID = @CountryID OR CountryID = '') 
							AND (ZipCode IS NULL OR ZipCode = @ZipCode OR ZipCode = '')) = 0)
					   BEGIN
							INSERT INTO dbo.Addresses (AddressTypeID, AddressLine1, AddressLine2, City, StateID, CountryID, ZipCode, IsDefault) 
							SELECT @AddressTypeID, @AddressLine1, @AddressLine2, @City, LTRIM(RTRIM(@StateID)), LTRIM(RTRIM(@CountryID)), @ZipCode, 1 
								FROM @InputImportData
								WHERE ImportDataId1 = @LoopCounter
						
							SET @AddressID = (SELECT IDENT_CURRENT('Addresses'))
							
							INSERT INTO dbo.ContactAddressMap (ContactID, AddressID)
							VALUES  (@ContactID, @AddressID)
							
							UPDATE dbo.Addresses
								SET IsDefault	= 0
								FROM dbo.Contacts C
									INNER JOIN dbo.ContactAddressMap CAM ON C.ContactID = CAM.ContactID
									INNER JOIN dbo.Addresses A ON A.AddressID = CAM.AddressID
								WHERE C.IsDeleted = 0 AND C.ContactID = @ContactID
									AND A.AddressID NOT IN (SELECT TOP 1 AddressID FROM dbo.ContactAddressMap 
										WHERE ContactID = @ContactID ORDER BY AddressID DESC) 
					   END							
				  END					
				
					/* Contact Phone Numbers Imports Data */
					SET @PhoneData = (SELECT PhoneData FROM @InputImportData
										WHERE ImportDataId1 = @LoopCounter AND LEN(PhoneData) > 0)
										
					IF (LEN(@PhoneData) > 0)
					BEGIN
						SELECT @Index = 1
							WHILE @Index != 0
						BEGIN
							SELECT @Index = CHARINDEX('~', @PhoneData)					
						IF @Index != 0
							BEGIN
								SELECT @Data = LEFT(@PhoneData, @Index-1)
							END
						ELSE
							BEGIN
								SELECT @Data = @PhoneData
							END
	
						INSERT INTO @PhoneNumbers (PhoneNoData)
							VALUES( @Data )	
	
						SELECT @PhoneData = RIGHT(@PhoneData, LEN(@PhoneData) - @Index)
	
						IF LEN(@PhoneData) = 0
						BEGIN BREAK
							END
						END

						SET @CustCursor = CURSOR
							FOR SELECT PhoneNoData FROM @PhoneNumbers
						OPEN @CustCursor
						FETCH NEXT FROM @CustCursor INTO @PhoneNumber
						WHILE @@Fetch_Status = 0
							BEGIN 
								SET @PhoneNoTypeID = SUBSTRING(@PhoneNumber, 1, CHARINDEX('|', @PhoneNumber)-1)
								SET @PhoneNumber = SUBSTRING(@PhoneNumber, CHARINDEX('|', @PhoneNumber)+1, LEN(@PhoneNumber))
								SET @CotactPhoneCount = 0
								SET @IsPrimaryCount = (SELECT COUNT(1) FROM dbo.ContactPhoneNumbers 
												WHERE ContactID = @ContactID AND IsPrimary = 1 AND IsDeleted = 0)
				
								IF (@IsPrimaryCount = 0)
									BEGIN
										SET @IsPrimary = 1
									END
								ELSE
									BEGIN
										SET @IsPrimary = 0
									END

								SET @CotactPhoneCount = (SELECT COUNT(1) FROM dbo.ContactPhoneNumbers 
															WHERE ContactID = @ContactID AND PhoneNumber = @PhoneNumber AND AccountID = @AccountID 
																AND PhoneType = @PhoneNoTypeID AND IsDeleted = 0)
							
								IF (@CotactPhoneCount = 0)
									BEGIN
										INSERT INTO dbo.ContactPhoneNumbers (ContactID, PhoneNumber, PhoneType, IsPrimary, AccountID,IsDeleted)
										VALUES (@ContactID, @PhoneNumber, @PhoneNoTypeID, @IsPrimary, @AccountID,0)
									END
								--ELSE
								--	BEGIN
								--		UPDATE dbo.ContactPhoneNumbers
								--			SET PhoneType = @PhoneNoTypeID
								--			WHERE ContactID = @ContactID AND PhoneNumber = @PhoneNumber AND AccountID = @AccountID AND PhoneType = @PhoneNoTypeID
								--	END	

							FETCH NEXT FROM @CustCursor INTO @PhoneNumber
						END
						CLOSE @CustCursor
						DEALLOCATE @CustCursor

						DELETE FROM @PhoneNumbers
					END
					
					SET @SuccessRate	= @SuccessRate + 1
				END
			END TRY
			BEGIN CATCH 

			   PRINT 'ERROR OCCURED IN CATCH BLOCK AND ERROR MESSAGE IS BELOW: '
			   PRINT ERROR_MESSAGE()

			   PRINT 'ERROR LINE NUMBER IS: '
			   PRINT ERROR_LINE()

				UPDATE dbo.LeadAdapterJobLogDetails 
					SET LeadAdapterRecordStatusID	= 8,
						CreatedDateTime				= GETUTCDATE(), 
						Remarks						= ERROR_MESSAGE() 
					--WHERE ReferenceID = (SELECT TOP 1 ReferenceId FROM @InputImportData WHERE ImportDataId = @LoopCounter)
					WHERE LeadAdapterJobLogDetailID = (SELECT TOP 1 LeadAdapterJobLogDetailID FROM dbo.LeadAdapterJobLogDetails WHERE
								 ReferenceID = (SELECT ReferenceID FROM @InputImportData WHERE ImportDataId1 = @LoopCounter) ORDER BY CreatedDateTime DESC)
			
				SET @LoopCounter = @LoopCounter + 1

				INSERT INTO dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate, ReferenceID, ContactID)
				VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE(), @ReferenceID, @ContactID)

			END CATCH
			SET @LoopCounter	= @LoopCounter + 1
		
		END

		/* Elastic Search Object */
		SELECT ContactID FROM dbo.Contacts
			WHERE ReferenceID IN (SELECT ReferenceId FROM @ImportData)
				 AND AccountID = @AccountID AND IsDeleted = 0 AND
				 ReferenceID != '00000000-0000-0000-0000-000000000000' 
			UNION 
		SELECT ContactID FROM @ImportData WHERE ContactID > 0 
				AND ReferenceId != '00000000-0000-0000-0000-000000000000'
			UNION  
		SELECT CompanyID FROM @Companies WHERE CompanyID > 0
			AND REFERENCEID != '00000000-0000-0000-0000-000000000000'
		

	SET NOCOUNT OFF
END 

/*
	EXEC [dbo].[IMPORT_Contacts_Data]
	 
*/