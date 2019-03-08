


CREATE PROCEDURE [dbo].[Process_Contact_Imports]	
AS
BEGIN
	declare @counter int = 0;
	declare @rowCount int = 0;
	declare @batchCount int = 1000;

	SET NOCOUNT ON
	BEGIN TRY
		--Search for job to process		
		DECLARE @LeadAdapterJobLogID		int,
				@ContactSource				tinyint,
				@SourceType					int,
				@OwnerID					int,
				@LeadAdapterAndAccountMapID int,
				@DuplicateLogic				tinyint = 2,
				@DupUpdate					tinyint = 1,
				@LeadSourceID				int,
				@AddressTypeID				int,
				@AccountID					int,
				@TrackMessages				tinyint = 1,
				@fileName varchar(500),
				@ResultID INT,
				@NeverBounceRequestID INT,
				@LeadAdapterType INT,
				@IncludeInReports bit = 1

		DECLARE @CustomFieldData TABLE(ContactID INT, LeadSubmissionID INT)

		SELECT TOP 1 @LeadAdapterJobLogID = LeadAdapterJobLogID, @ContactSource = 2, @SourceType = 11, @OwnerID = OwnerID, @fileName = [FileName]
			FROM dbo.LeadAdapterJobLogs(NOLOCK) WHERE LeadAdapterJobStatusID = 7 AND CreatedDateTime < GETUTCDATE() ORDER BY LeadAdapterJobLogID ASC --Read To Process

       	IF (EXISTS(SELECT * FROM Users (NOLOCK) WHERE UserID = @OwnerId AND AccountID = 1) OR 
			NOT EXISTS(SELECT * FROM Users (NOLOCK) WHERE UserID = @OwnerId AND IsDeleted = 0 AND Status = 1))
		BEGIN
			SELECT @OwnerId = value FROM EnvironmentSettings (NOLOCK) WHERE Name = 'CRM User'
		END


		SELECT @AccountID = AccountID,@LeadAdapterType = [LeadAdapterTypeID] FROM LeadAdapterAndAccountMap (NOLOCK) LAM
		INNER JOIN LeadAdapterJobLogs (NOLOCK) LAJ ON LAJ.LeadAdapterAndAccountMapID = LAM.LeadAdapterAndAccountMapID
		WHERE LAJ.LeadAdapterJobLogID = @LeadAdapterJobLogID

		--Log start time
		INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		VALUES('Process_Contact_Imports', @AccountID, '@LeadAdapterJobLogID:' + cast(@LeadAdapterJobLogID as varchar(10)))
		SET @ResultID = scope_identity()

	IF @leadAdapterJobLogID IS NOT NULL
	BEGIN
		INSERT INTO DebugLogs
		SELECT @AccountID, 'Found leadadapter job log ' + cast(@LeadAdapterJobLogID as varchar(10)), GETUTCDATE()
		--Change the Job Status
		UPDATE dbo.LeadAdapterJobLogs SET LeadAdapterJobStatusID = 8 WHERE LeadAdapterJobLogID = @LeadAdapterJobLogID --Processing Contacts

		SELECT @DupUpdate = UpdateOnDuplicate, @DuplicateLogic = DuplicateLogic, @TrackMessages = 2
		,@IncludeInReports = IncludeInReports FROM dbo.ImportDataSettings (NOLOCK) WHERE LeadAdaperJobID = @LeadAdapterJobLogID

		update ImportContactData  set OrginalRefId= cast(ReferenceId as varchar(50)) where JobID = @LeadAdapterJobLogID and OrginalRefId is null
		--Update Loop Id

		declare @start int = 0
		select @start =  MIN(ImportContactDataId) From ImportContactData (nolock) where JobId = @leadAdapterJobLogID and IsDuplicate = 0
	
		IF  @SourceType= 11
			BEGIN
				UPDATE ICD
				SET ICD.LoopID	= 0, ContactID = NULL--, ValidEmail = dbo.EmailValidation(ICD.PrimaryEmail, ICD.FirstName, ICD.LastName)
				FROM dbo.ImportContactData ICD WHERE ICD.IsDuplicate = 1 AND JobId = @leadAdapterJobLogID
			END
		ELSE
			BEGIN
				UPDATE ICD
				SET ICD.LoopID	= 0, ContactID = NULL , ValidEmail = dbo.EmailValidation(ICD.PrimaryEmail, ICD.FirstName, ICD.LastName)
				FROM dbo.ImportContactData ICD WHERE ICD.IsDuplicate = 1 AND JobId = @leadAdapterJobLogID
			END
		
	

		--UPDATE ICD
		--SET ICD.LoopID	= (ICD.ImportContactDataId - @start + 1), ContactID = NULL--, ValidEmail = dbo.EmailValidation(ICD.PrimaryEmail, ICD.FirstName, ICD.LastName)
		--FROM dbo.ImportContactData ICD WHERE ICD.IsDuplicate = 0 AND  JobId = @leadAdapterJobLogID				
		
		INSERT INTO DebugLogs
		SELECT @AccountID, 'updated loop id ' + cast(@LeadAdapterJobLogID as varchar(10)), GETUTCDATE()

		--Pull default Address
		SELECT @AddressTypeID = DDL.DropdownValueID, @AccountID = AM.AccountID, @LeadAdapterAndAccountMapID = AM.LeadAdapterAndAccountMapID,
			@ContactSource = ISNULL(CASE WHEN AM.LeadAdapterTypeID = 11 THEN 2 ELSE 1 END, 2), @SourceType = ISNULL(AM.LeadAdapterTypeID, 11), @LeadSourceID = AM.LeadSourceType
			FROM dbo.LeadAdapterAndAccountMap(NOLOCK) AM
				INNER JOIN dbo.LeadAdapterJobLogs(NOLOCK) JL ON JL.LeadAdapterAndAccountMapID = AM.LeadAdapterAndAccountMapID
				LEFT JOIN dbo.DropdownValues(NOLOCK) DDL ON DDL.AccountID = AM.AccountID and DropdownID = 2 AND IsActive = 1 AND IsDefault = 1  
			WHERE JL.LeadAdapterJobLogID = @LeadAdapterJobLogID AND ddl.IsDeleted = 0
		
		INSERT INTO DebugLogs
		SELECT @AccountID, 'Get Address type id ' + cast(@LeadAdapterJobLogID as varchar(10)), GETUTCDATE()

		--Remove Duplicate Contact PhoneNumbers
		;WITH TempPhoneNumbers (PhoneType, PhoneNumber, ReferenceID, DuplicatecCount)
			AS
			(
			SELECT PhoneType, PhoneNumber, ReferenceID,
				ROW_NUMBER() OVER(PARTITION BY PhoneType, PhoneNumber, ReferenceID ORDER BY ImportPhoneDataID) AS DuplicatecCount
				FROM dbo.ImportPhoneData (NOLOCK)
				WHERE ReferenceID IN (SELECT ReferenceID FROM dbo.ImportContactData (NOLOCK)
				WHERE JobID = @leadAdapterJobLogID AND AccountID = @accountID AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL) AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL) AND IsDuplicate=0) 
			)
				DELETE FROM TempPhoneNumbers WHERE DuplicatecCount > 1

		--Remove Duplicate Contact CustomFields
		;WITH TempCustomFields (FieldID, FieldTypeID, FieldValue, ReferenceID, DuplicatecCount)
			AS
			(
				SELECT FieldID, FieldTypeID, FieldValue, ReferenceID,
				ROW_NUMBER() OVER(PARTITION BY FieldID, FieldTypeID, FieldValue, ReferenceID ORDER BY ImportCustomDataID) AS DuplicatecCount
				FROM dbo.ImportCustomData (NOLOCK)
				WHERE ReferenceID IN (SELECT ReferenceID FROM dbo.ImportContactData(NOLOCK) WHERE JobID = @leadAdapterJobLogID 
				AND AccountID = @accountID AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)  AND IsDuplicate=0)
			)
				DELETE FROM TempCustomFields WHERE DuplicatecCount > 1

		INSERT INTO DebugLogs
		SELECT @AccountID, 'deleted duplicate phone numbers ' + cast(@LeadAdapterJobLogID as varchar(10)), GETUTCDATE()

		IF (ISNULL(@AddressTypeID, 0) = 0)
			BEGIN
				SET @AddressTypeID = (SELECT TOP 1 DropdownValueID FROM dbo.DropdownValues WHERE DropdownID = 2 AND IsActive = 1 AND AccountID = @AccountID and IsDeleted = 0)	
			END

		EXEC [dbo].[Process_Import_Company] @leadAdapterJobLogID ,@ContactSource ,@ownerId ,@AccountID ,@sourceType;

		

		UPDATE ICD SET ICD.CompanyID = C.ContactID
			FROM dbo.ImportContactData ICD
				INNER JOIN dbo.Contacts C (NOLOCK)  ON ICD.AccountID = C.AccountID AND ICD.CompanyName = C.Company and C.IsDeleted =0  AND IsDuplicate=0 and C.ContactType=2
			WHERE ICD.JobID = @LeadAdapterJobLogID AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
		
		INSERT INTO DebugLogs
		SELECT @AccountID, 'Updated Company Id ' + cast(@LeadAdapterJobLogID as varchar(10)), GETUTCDATE()

		UPDATE ICD 
		SET ICD.PartnerTypeID = PDL.DropdownValueID,
			ICD.LifecycleStageID = CASE WHEN LCS.DropdownValueID IS NOT NULL THEN LCS.DropdownValueID ELSE LCSDefault.DropdownValueID END,
			ICD.LeadSourceID = CASE WHEN ICD.LeadSourceID IS NOT NULL AND ICD.LeadSourceID > 0 THEN ICD.LeadSourceID 
								 WHEN @LeadSourceID IS NOT NULL AND @LeadSourceID > 0 THEN @LeadSourceID ELSE LeadSrcTypeDefault.DropdownValueID END,
			ZipCode = CASE  WHEN LEN(ICD.ZipCode) = 3 THEN '00' + ICD.ZipCode WHEN LEN(ICD.ZipCode) = 4 THEN '0'+ ICD.ZipCode ELSE ICD.ZipCode END
		FROM dbo.ImportContactData(NOLOCK) ICD
		LEFT JOIN dbo.DropdownValues PDL  (NOLOCK) ON PDL.AccountID = ICD.AccountID AND PDL.DropdownID = 4 AND PDL.IsActive = 1 AND PDL.DropdownValue = ICD.PartnerType
		LEFT JOIN dbo.DropdownValues LCS (NOLOCK)  ON LCS.AccountID = ICD.AccountID AND LCS.DropdownID = 3 AND LCS.IsActive = 1 AND LCS.DropdownValue = ICD.LifecycleStage	
		LEFT JOIN dbo.DropdownValues LCSDefault  (NOLOCK) ON LCSDefault.AccountID = ICD.AccountID AND LCSDefault.DropdownID = 3 AND LCSDefault.IsActive = 1 AND LCSDefault.IsDefault = 1 and LCSDefault.IsDeleted !=1
		LEFT JOIN dbo.DropdownValues LeadSrcTypeDefault (NOLOCK)  ON LeadSrcTypeDefault.AccountID = ICD.AccountID AND LeadSrcTypeDefault.DropdownID = 5 AND LeadSrcTypeDefault.DropdownValueTypeID = 40
		WHERE ICD.JobID = @leadAdapterJobLogID AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL) AND IsDuplicate=0
		 
		INSERT INTO DebugLogs
		SELECT @AccountID, 'Updated Leadsource Id ' + cast(@LeadAdapterJobLogID as varchar(10)), GETUTCDATE()

		UPDATE ICD 
		SET 
		AddressLine1 = CASE WHEN LEN(ICD.AddressLine1) = 0 OR ICD.AddressLine1 IS NULL THEN '' ELSE ICD.AddressLine1 END
		,AddressLine2 = CASE WHEN LEN(ICD.AddressLine2) = 0 OR ICD.AddressLine2 IS NULL THEN '' ELSE ICD.AddressLine2 END
		,City = CASE WHEN LEN(ICD.City) = 0 OR ICD.City IS NULL THEN TR.CityName ELSE ICD.City END
		,[State] = CASE WHEN LEN(ICD.[State]) = 0 OR ICD.[State] IS NULL THEN TR.[StateCode] ELSE ICD.[State] END
		,Country = CASE WHEN (LEN(ICD.Country) = 0 OR ICD.Country IS NULL) AND TR.CountryID = 1 THEN 'US' 
						WHEN (LEN(ICD.Country) = 0 OR ICD.Country IS NULL) AND TR.CountryID = 2 THEN 'CA' ELSE ICD.Country END 

		FROM dbo.ImportContactData ICD (NOLOCK)
			LEFT JOIN dbo.TaxRates TR (NOLOCK)  ON ICD.ZIPCode = TR.ZIPCode
		WHERE ICD.JobID = @leadAdapterJobLogID AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
			AND ValidEmail = 1  AND IsDuplicate=0

		INSERT INTO DebugLogs
		SELECT @AccountID, 'Updated Address, Zip ' + cast(@LeadAdapterJobLogID as varchar(10)) +': DupLogic' + cast(@DuplicateLogic as varchar(10)) , GETUTCDATE()

		/* Contacts Data Update on Duplicates Logic */
		IF (ISNULL(@DuplicateLogic, 2) = 1)
			BEGIN
				/* Update ContactID and ContactStatusID for Existing Contacts */
				UPDATE ImportContactData
					SET ContactID			= CE.ContactID,
						ContactStatusID		= 3,
						CommunicationID		= C.CommunicationID,
						EmailExists			= 1,
						ReferenceID			= C.ReferenceID
					FROM dbo.ImportContactData ICD				
						INNER JOIN dbo.ContactEmails CE (NOLOCK)  ON ICD.AccountID = CE.AccountID AND ICD.PrimaryEmail = CE.Email
						INNER JOIN dbo.Contacts C (NOLOCK)  ON C.ContactID = CE.ContactID AND CE.AccountID = C.AccountID
					WHERE C.IsDeleted = 0 AND CE.IsDeleted = 0 AND ICD.JobID = @leadAdapterJobLogID AND LEN(ICD.PrimaryEmail) > 0
					AND (ICD.IsBuilderNumberPass = 1 OR ICD.IsBuilderNumberPass IS NULL)  
					AND (ICD.IsCommunityNumberPass = 1 OR ICD.IsCommunityNumberPass IS NULL)
					AND ICD.ValidEmail = 1 AND ICD.IsDuplicate=0
				IF @DupUpdate = 0
				BEGIN
					UPDATE ImportContactData SET ContactStatusID = 0, ContactID = null WHERE ContactStatusID=3 AND JobID = @leadAdapterJobLogID
				END
			END
		ELSE IF (ISNULL(@DuplicateLogic, 2) = 2)
			BEGIN
				/* Update ContactID and ContactStatusID for Existing Contacts */
			
				UPDATE ImportContactData
					SET ContactID			= CE.ContactID,
						ContactStatusID		= 3,
						CommunicationID		= C.CommunicationID,
						EmailExists			= 1,
						ReferenceID			= C.ReferenceID
					FROM dbo.ImportContactData ICD				
						INNER JOIN dbo.ContactEmails CE (NOLOCK)  ON ICD.AccountID = CE.AccountID AND ICD.PrimaryEmail = CE.Email
						INNER JOIN dbo.Contacts C  (NOLOCK) ON C.ContactID = CE.ContactID AND CE.AccountID = C.AccountID
					WHERE C.IsDeleted = 0 AND CE.IsDeleted = 0 AND ICD.JobID = @leadAdapterJobLogID AND LEN(ICD.PrimaryEmail) > 0					
						AND (ICD.IsBuilderNumberPass = 1 OR ICD.IsBuilderNumberPass IS NULL)
						AND (ICD.IsCommunityNumberPass = 1 OR ICD.IsCommunityNumberPass IS NULL)
						AND ICD.ValidEmail = 1 AND ICD.IsDuplicate=0
                

			--UPDATE ImportContactData
			--		SET ContactID			= CE.ContactID,
			--			ContactStatusID		= 3,
			--			CommunicationID		= C.CommunicationID,
			--			EmailExists			= 1,
			--			ReferenceID			= C.ReferenceID
			--FROM dbo.ImportContactData ICD				
			--			INNER JOIN dbo.ContactEmails CE ON ICD.AccountID = CE.AccountID AND ICD.PrimaryEmail = CE.Email
			--			INNER JOIN dbo.Contacts C ON C.ContactID = CE.ContactID AND CE.AccountID = C.AccountID
			--		WHERE C.IsDeleted = 0 AND CE.IsDeleted = 0 AND ICD.JobID = @leadAdapterJobLogID AND LEN(ICD.PrimaryEmail) > 0					
			--			AND (ICD.IsBuilderNumberPass = 1 OR ICD.IsBuilderNumberPass IS NULL)
			--			AND (ICD.IsCommunityNumberPass = 1 OR ICD.IsCommunityNumberPass IS NULL)
			--			AND ICD.ValidEmail = 1 AND ICD.IsDuplicate=1


				/* Update ContactID and ContactStatusID for Existing Contacts */
				--select ICD.* 
				--	FROM dbo.ImportContactData ICD
				--		--INNER JOIN dbo.ContactEmails CE ON ICD.AccountID = CE.AccountID AND ICD.PrimaryEmail = CE.Email
				--		INNER JOIN dbo.Contacts C ON ICD.AccountID = C.AccountID AND ICD.FirstName = C.FirstName AND ICD.LastName = C.LastName
				--	WHERE C.IsDeleted = 0 AND ICD.JobID = 5236 AND ICD.PrimaryEmail = ''
				--	AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)
				--	AND (ValidEmail = 1) AND (ICD.ContactID IS NULL OR ICD.ContactID = 0) 
				--	AND IsDuplicate=0 and (ContactStatusID is null or ContactStatusID <> 3);

				UPDATE ImportContactData
					SET ContactID			= C.ContactID,
						ContactStatusID		= 3,
						CommunicationID		= C.CommunicationID,
						EmailExists			= 1,
						ReferenceID			= C.ReferenceID
					FROM dbo.ImportContactData ICD
					--INNER JOIN dbo.ContactEmails CE ON ICD.AccountID = CE.AccountID AND ICD.PrimaryEmail = CE.Email
					INNER JOIN dbo.Contacts C  (NOLOCK) ON ICD.AccountID = C.AccountID AND ICD.FirstName = C.FirstName AND ICD.LastName = C.LastName
					WHERE C.IsDeleted = 0 AND ICD.JobID = @leadAdapterJobLogID AND  (ICD.PrimaryEmail ='' or  ICD.PrimaryEmail is null)
					AND (ICD.IsBuilderNumberPass = 1 OR ICD.IsBuilderNumberPass IS NULL)
					 AND (ICD.IsCommunityNumberPass = 1 OR ICD.IsCommunityNumberPass IS NULL)
					AND (ICD.ValidEmail = 1) AND (ICD.ContactID IS NULL OR ICD.ContactID = 0) 
					AND ICD.IsDuplicate=0 and (ICD.ContactStatusID is not null and ICD.ContactStatusID <> 3);

				IF @DupUpdate = 0
				BEGIN
					UPDATE ImportContactData SET ContactStatusID = 0, ContactID = null WHERE ContactStatusID=3 AND JobID = @leadAdapterJobLogID AND IsDuplicate=0
				END

				/* Update ContactID and ContactStatusID for Existing Contacts */
				--UPDATE ImportContactData
				--	SET ContactID			= NULL,
				--		ContactStatusID		= 1,
				--		CommunicationID		= C.CommunicationID,
				--		EmailExists			= 0
				--	FROM dbo.ImportContactData ICD
				--		--INNER JOIN dbo.ContactEmails CE ON ICD.AccountID = CE.AccountID AND ICD.PrimaryEmail != CE.Email
				--		INNER JOIN dbo.Contacts C ON ICD.AccountID = C.AccountID AND ICD.FirstName = C.FirstName AND ICD.LastName = C.LastName
				--		INNER JOIN dbo.ContactEmails CE ON C.ContactID = CE.ContactID AND ICD.PrimaryEmail != CE.Email
				--	WHERE C.IsDeleted = 0 AND ICD.JobID = @leadAdapterJobLogID AND EmailExists = 0
				--		AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL) 
				--		AND ValidEmail = 1 AND (ICD.ContactID IS NULL OR ICD.ContactID = 0)
				INSERT INTO DebugLogs
				SELECT @AccountID, 'Before getting list of emails ' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()

				;with newMail
				as
				(
					SELECT PrimaryEmail FROM ImportContactData (nolock) icd where JobID = @leadAdapterJobLogID and ValidEmail=1 AND (ContactID IS NULL OR ContactID = 0) 
					AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL) AND IsDuplicate=0 and len(PrimaryEmail)>0
					AND NOT EXISTS (select 1 
										from Contacts C (nolock)
										JOIN ContactEmails (NOLOCK) CE ON C.ContactID = CE.ContactID AND C.AccountId = CE.AccountId
										WHERE C.IsDeleted =0 and CE.IsPrimary =1 and C.AccountID = @AccountID AND CE.IsDeleted = 0 AND CE.Email = ICD.PrimaryEmail)
					
				)
				UPDATE ImportContactData
					SET ContactID			= NULL,
						ContactStatusID		= 1,
						CommunicationID		= NULL,
						EmailExists			= 0
					FROM dbo.ImportContactData ICD
					JOIN newMail NM ON ICD.PrimaryEmail = NM.PrimaryEmail
					where  ICD.IsDuplicate=0;

					INSERT INTO DebugLogs
				SELECT @AccountID, 'After updating contact status based on email 1 ' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()

				UPDATE ImportContactData
					SET ContactID			= C.ContactID,
						ContactStatusID		= 3,
						CommunicationID		= C.CommunicationID,
						EmailExists			= 1
					FROM dbo.ImportContactData ICD						
						INNER JOIN dbo.Contacts C  (NOLOCK) ON ICD.AccountID = C.AccountID AND ICD.FirstName = C.FirstName AND ICD.LastName = C.LastName
					WHERE C.IsDeleted = 0 AND ICD.JobID = @leadAdapterJobLogID
						AND (ICD.IsBuilderNumberPass = 1 OR ICD.IsBuilderNumberPass IS NULL) 
						 AND (ICD.IsCommunityNumberPass = 1 OR ICD.IsCommunityNumberPass IS NULL)
						AND ICD.ValidEmail = 1 AND (ICD.ContactID IS NULL OR ICD.ContactID = 0) and ICD.ContactStatusID is null AND ICD.IsDuplicate=0
				END

				INSERT INTO DebugLogs
				SELECT @AccountID, 'After updating contact status based on email 2 ' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()
			--Delete duplicate Contacts			
			;with dup
			as
			(
				select ContactId, Max(ImportContactDataID) as ImportContactDataID  from dbo.ImportContactData (NOLOCK)  where JobId = @leadAdapterJobLogID and (ContactId is not null and ContactID != 0)
				AND IsDuplicate=0
				group by ContactId having Count(1)>1
			)
			delete from dbo.ImportContactData where ImportContactDataID in(
			select ICD.ImportContactDataID from dbo.ImportContactData ICD (NOLOCK) 
			join dup on ICD.ContactID = dup.ContactID
			where ICD.ImportContactDataID not in (select ImportContactDataID from dup)
			)

			INSERT INTO DebugLogs
				SELECT @AccountID, 'After deleting duplicate contacts ' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()

		--Update Contact Table		
		IF ((ISNULL(@DupUpdate, 1) = 1 AND ISNULL(@DuplicateLogic, 2) = 1))
			BEGIN
				EXEC [dbo].[Process_Import_Contacts]  @leadAdapterJobLogID, @accountID, @ownerId, @ContactSource, @SourceType, 1,@IncludeInReports;;
			END				
		ELSE IF ((ISNULL(@DupUpdate, 1) = 1 AND ISNULL(@DuplicateLogic, 2) = 2))
			BEGIN
				EXEC [dbo].[Process_Import_Contacts]  @leadAdapterJobLogID, @accountID, @ownerId, @ContactSource, @SourceType, 2,@IncludeInReports;;		
			END 
		ELSE IF ((ISNULL(@DupUpdate, 1) = 0 AND ISNULL(@DuplicateLogic, 2) = 1))
			BEGIN
				EXEC [dbo].[Process_Import_Contacts]  @leadAdapterJobLogID, @accountID, @ownerId, @ContactSource, @SourceType, 3,@IncludeInReports;;
			END
		ELSE IF ((ISNULL(@DupUpdate, 1) = 0 AND ISNULL(@DuplicateLogic, 2) = 2))
			BEGIN
				EXEC [dbo].[Process_Import_Contacts]  @leadAdapterJobLogID, @accountID, @ownerId, @ContactSource, @SourceType, 4,@IncludeInReports;;
			END
		
		INSERT INTO DebugLogs
		SELECT @AccountID, 'After importing contacts ' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()

		--Update Contact Id
		UPDATE dbo.ImportContactData
			SET ContactID = C.ContactID
			FROM dbo.ImportContactData ICD
				INNER JOIN dbo.Contacts C (NOLOCK)  ON ICD.AccountID = C.AccountID AND ICD.ReferenceID = C.ReferenceID 
			WHERE C.IsDeleted = 0 AND ICD.JobID = @LeadAdapterJobLogID 
				AND (ICD.IsBuilderNumberPass = 1 OR ICD.IsBuilderNumberPass IS NULL)  
				AND (ICD.IsCommunityNumberPass = 1 OR ICD.IsCommunityNumberPass IS NULL) 
				AND ICD.ValidEmail = 1 AND ICD.IsDuplicate=0;

		INSERT INTO DebugLogs
		SELECT @AccountID, 'After updating contact ids to importcontactdata' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()



		--Update Lead Soure Map
		IF (@ContactSource = 1)
		  BEGIN 
             EXEC [dbo].[Process_Import_ContactLeadSourceMap] @LeadAdapterJobLogID
          END
        ELSE  
		   BEGIN
		      UPDATE  ImportContactData 
			   SET  LeadSource = case when LeadSource IS NULL THEN (SELECT TOP 1 D.DropdownValue FROM [dbo].[ImportDataSettings] I 
               INNER JOIN dropdownvalues D ON D.DropdownValueID = I.[LeadSourceID]
               WHERE  [LeadAdaperJobID] = @LeadAdapterJobLogID)
			   ELSE LeadSource END  WHERE JobID = @LeadAdapterJobLogID 
		      
             EXEC [dbo].[Process_Import_LeadSourceMap] @LeadAdapterJobLogID
		   END
		
		--Update Contact Emails
		--INSERT INTO dbo.ContactEmails (ContactID, Email, EmailStatus, IsPrimary, AccountID, IsDeleted)
		SELECT ICD.ContactID, ICD.PrimaryEmail as Email
		, CASE WHEN ICES.EmailStatus IS NULL THEN 51 ELSE ICES.EmailStatus END as EmailStatus
		, 1 as IsPrimary, ICD.AccountID, 0 as IsDeleted, Row_Number() over(Order by ImportContactDataID) as RowNumber
		INTO #tEamilStatus FROM dbo.ImportContactData(NOLOCK) ICD
			LEFT JOIN dbo.ImportContactsEmailStatuses(NOLOCK) ICES ON ICD.ReferenceID = ICES.ReferenceID AND  ICES.LeadadapterJobLogID = ICD.JobID
			LEFT JOIN dbo.ContactEmails(NOLOCK) CE ON CE.ContactID = ICD.ContactID AND CE.Email = ICD.PrimaryEmail 
				AND CE.AccountID = ICD.AccountID AND CE.IsDeleted = 0
			LEFT JOIN Contacts(NOLOCK) C on C.ContactID = CE.ContactID
		WHERE ICD.JobID = @LeadAdapterJobLogID AND LEN(ICD.PrimaryEmail) > 0
			AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)
			 AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
			AND ICD.ContactID > 0 AND CE.ContactEmailID IS NULL AND ICD.ValidEmail = 1 and (c.ContactID is null or c.IsDeleted=0)
			 AND IsDuplicate=0;

		INSERT INTO DebugLogs
		SELECT @AccountID, 'After updating email status' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()

		SET @counter = 0;
		select @rowCount = Count(1) from #tEamilStatus;

		WHILE (1 = 1)
		BEGIN
			INSERT INTO dbo.ContactEmails (ContactID, Email, EmailStatus, IsPrimary, AccountID, IsDeleted)
			SELECT ContactID, Email, EmailStatus, IsPrimary, AccountID, IsDeleted
			FROM #tEamilStatus ICD 
			WHERE ICD.RowNumber between (@counter * @batchCount + 1) and ((@counter * @batchCount) + @batchCount);

			IF (@@ROWCOUNT = 0  AND @rowCount < ((@counter * @batchCount) + @batchCount))
			BEGIN
				BREAK
			END
	 
			SET @counter = @counter + 1;
		END

		INSERT INTO DebugLogs
		SELECT @AccountID, 'After inserting emails ' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()

		--Update Audit Info
       select ContactID,FirstName,LastName,Company,CommunicationID,Title,ContactImageUrl,AccountID,LeadScore,LeadSource,HomePhone,WorkPhone,MobilePhone,PrimaryEmail,ContactType,SSN,LifecycleStage
		,PartnerType,DoNotEmail,LastContacted,IsDeleted,ProfileImageKey,ImageID,ReferenceID,LastUpdatedBy,LastUpdatedOn,'U' as AuditAction,AuditStatus,ContactSource,SourceType,CompanyID,OwnerID
		,LastContactedThrough,IsLifecycleStageChanged,NewLifecycleStage,FirstContactSource,FirstSourceType, 1 as RowNumber into #tContactAudit from dbo.Contacts_Audit where 1 <> 1;

              insert into #tContactAudit(ContactID, FirstName, LastName, Company, CommunicationID, Title, ContactImageUrl, AccountID
              , LeadScore, LeadSource, HomePhone, WorkPhone, MobilePhone, PrimaryEmail, ContactType, SSN, LifecycleStage, PartnerType
              , DoNotEmail, LastContacted, IsDeleted, ProfileImageKey, ImageID, ReferenceID, LastUpdatedBy, LastUpdatedOn, 
              OwnerID, ContactSource, SourceType, CompanyID, LastContactedThrough, FirstContactSource, FirstSourceType, AuditAction, AuditStatus, RowNumber)
              SELECT C.ContactID, C.FirstName, C.LastName, C.Company, C.CommunicationID, C.Title, C.ContactImageUrl, C.AccountID
              , C.LeadScore, C.LeadSource, C.HomePhone, C.WorkPhone, C.MobilePhone, C.PrimaryEmail, C.ContactType, C.SSN, C.LifecycleStage, C.PartnerType
              , C.DoNotEmail, C.LastContacted, C.IsDeleted, C.ProfileImageKey, C.ImageID, C.ReferenceID, C.LastUpdatedBy, C.LastUpdatedOn
              , C.OwnerID, C.ContactSource, C.SourceType, C.CompanyID, C.LastContactedThrough, C.FirstContactSource,C. FirstSourceType,        
              CASE WHEN ICD.ContactStatusID = 1 THEN 'I' WHEN ICD.ContactStatusID = 3 THEN 'U' END as AuditAction
              ,1 as AuditStatus
              ,0 as RowNumber            
              FROM dbo.Contacts(NOLOCK) C
              INNER JOIN dbo.ImportContactData ICD (NOLOCK)  ON C.ContactID = ICD.ContactID AND C.AccountID = ICD.AccountID
              WHERE C.IsDeleted = 0 AND ICD.JobID = @leadAdapterJobLogID
                     AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)
                     AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
                     AND ICD.ContactID > 0 AND ICD.ValidEmail = 1 and ContactStatusID <> 0
                     AND IsDuplicate=0
              UNION         
              SELECT C.ContactID, C.FirstName, C.LastName, C.Company, C.CommunicationID, C.Title, C.ContactImageUrl, C.AccountID, C.LeadScore, C.LeadSource, C.HomePhone, C.WorkPhone, C.MobilePhone, C.PrimaryEmail, C.ContactType, C.SSN, C.LifecycleStage, C.PartnerType, C.DoNotEmail, C.LastContacted, C.IsDeleted, C.ProfileImageKey, C.ImageID, C.ReferenceID, C.LastUpdatedBy, C.LastUpdatedOn, C.OwnerID, C.ContactSource, C.SourceType, C.CompanyID, C.LastContactedThrough, C.FirstContactSource
              ,C. FirstSourceType
                , CASE WHEN CL.ContactStatusID = 1 THEN 'I'
			    WHEN cl.EmailExists = 1 or CL.ContactStatusID = 0 THEN 'I'
			   WHEN cl.EmailExists = 1 or CL.ContactStatusID = 3 THEN 'I' ELSE 'U'  END as AuditAction
              , 1 as AuditStatus
              ,0 as RowNumber
              FROM dbo.Contacts(NOLOCK) C
                     INNER JOIN dbo.ImportContactData CL (NOLOCK)  ON C.ContactID = CL.CompanyID AND C.AccountID = CL.AccountID AND IsDuplicate=0
              WHERE C.IsDeleted = 0 AND CL.JobID = @leadAdapterJobLogID
                     AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)
                     AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
                     AND CL.CompanyID > 0 AND CL.ValidEmail = 1 and ContactStatusID >=  0
					 and c.ContactID NOT IN (SELECT Contactid from Contacts_Audit (NOLOCK) WHERE Accountid = @accountid and contacttype = 2 );

				INSERT INTO DebugLogs
				SELECT @AccountID, 'After updating contact audit into temp table ' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()

              update T
              set RowNumber = rn
              from (
              select *,
                           row_number() over(order by (select 1)) as rn
              from #tContactAudit
              ) T;

              SET @counter = 0;
              SELECT @rowCount = Count(1) from #tContactAudit;

              WHILE (1 = 1)
              BEGIN
                     INSERT INTO dbo.Contacts_Audit (ContactID, FirstName, LastName, Company, CommunicationID, Title, ContactImageUrl, AccountID
                     , LeadScore, LeadSource, HomePhone, WorkPhone, MobilePhone, PrimaryEmail, ContactType, SSN, LifecycleStage, PartnerType
                     , DoNotEmail, LastContacted, IsDeleted, ProfileImageKey, ImageID, ReferenceID, LastUpdatedBy, LastUpdatedOn, 
                     OwnerID, ContactSource, SourceType, CompanyID, LastContactedThrough, FirstContactSource, FirstSourceType, AuditAction, AuditStatus,AuditDate) 
                     SELECT ContactID, FirstName, LastName, Company, CommunicationID, Title, ContactImageUrl, AccountID
                     , LeadScore, LeadSource, HomePhone, WorkPhone, MobilePhone, PrimaryEmail, ContactType, SSN, LifecycleStage, PartnerType
                     , DoNotEmail, LastContacted, IsDeleted, ProfileImageKey, ImageID, ReferenceID, LastUpdatedBy, LastUpdatedOn, 
                     OwnerID, ContactSource, SourceType, CompanyID, LastContactedThrough, FirstContactSource, FirstSourceType, AuditAction, AuditStatus, GETUTCDATE()
                     FROM #tContactAudit ICD 
                     WHERE ICD.RowNumber between (@counter * @batchCount + 1) and ((@counter * @batchCount) + @batchCount);

                     IF (@@ROWCOUNT = 0  AND @rowCount < ((@counter * @batchCount) + @batchCount))
                     BEGIN
                           BREAK
                     END
       
                     SET @counter = @counter + 1;
              END

			  INSERT INTO DebugLogs
				SELECT @AccountID, 'After inserting contact audit ' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()

		--Activity Logs
		select cast(1 as bigint) UserActivityLogID
		,EntityID
		,UserID
		,ModuleID
		,UserActivityID
		,LogDate
		,AccountID
		,EntityName, 1 as RowNumber into #tUserActivityLogs from dbo.UserActivityLogs  (NOLOCK) where 1 <> 1;

              INSERT INTO #tUserActivityLogs (EntityID, UserID, ModuleID, UserActivityID, LogDate, AccountID, EntityName, RowNumber)
              SELECT C.ContactID, CASE WHEN C.OwnerID > 0 THEN C.OwnerID ELSE @ownerId END, 3, CASE WHEN ICD.ContactStatusID = 1 THEN 1 WHEN ICD.ContactStatusID = 3 THEN 3 END, GETUTCDATE(), C.AccountID, 
              CASE WHEN C.ContactType = 1 THEN ISNULL(C.FirstName, '')+' '+ISNULL(C.LastName, '') WHEN C.ContactType = 2 THEN C.Company END
              ,ROW_NUMBER() over(order by C.ContactId)
              FROM dbo.Contacts C (NOLOCK) 
                     INNER JOIN dbo.ImportContactData(NOLOCK) ICD ON C.ContactID = ICD.ContactID AND C.AccountID = ICD.AccountID
              WHERE C.IsDeleted = 0 AND ICD.JobID = @leadAdapterJobLogID
                     AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL) AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
                     AND ICD.ContactID > 0 AND ICD.ValidEmail = 1 and ICD.ContactStatusID <> 0 AND IsDuplicate=0

              SET @counter = 0;
              SELECT @rowCount = Count(1) from #tUserActivityLogs;

              WHILE (1 = 1)
              BEGIN
                     INSERT INTO dbo.UserActivityLogs (EntityID, UserID, ModuleID, UserActivityID, LogDate, AccountID, EntityName) 
                     select EntityID, UserID, ModuleID, UserActivityID, LogDate, AccountID, EntityName
                     FROM #tUserActivityLogs ICD 
                     WHERE ICD.RowNumber between (@counter * @batchCount + 1) and ((@counter * @batchCount) + @batchCount);

                     IF (@@ROWCOUNT = 0  AND @rowCount < ((@counter * @batchCount) + @batchCount))
                     BEGIN
                           BREAK
                     END
       
                     SET @counter = @counter + 1;
              END
				
				INSERT INTO DebugLogs
				SELECT @AccountID, 'After inserting user activity log ' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()

		--Update Tag Maps for Lead Adapters
		EXEC [dbo].[Process_Import_ContactTagMap] @leadAdapterJobLogID ,@ContactSource ,@leadAdapterAndAccountMapID, @ownerId 

		---- Communications Data		
		EXEC [dbo].[Process_Import_Communications]  @leadAdapterJobLogID ,@AccountID ,@AddressTypeID
		
		----	Custom Fields and Phone Number 		
		EXEC [dbo].[Process_Import_Phone_Custom_Field]  @leadAdapterJobLogID ,@AccountID

		
		--Lead Adapter Job Log Details
		EXEC [dbo].[Process_Import_LeadAdapterJobLogDetails] @leadAdapterJobLogID ,@AccountID ,@DuplicateLogic ,@DupUpdate ,@OwnerID;

		IF (@ContactSource = 1)
		  BEGIN 

		 	EXEC [dbo].[Process_Import_Contact_SecondaryLeadSourceMap] @leadAdapterJobLogID ,@AccountID ,@LeadAdapterType
      END
		
		IF (@ContactSource = 1)	
		BEGIN
			declare @importContactDataIDs table(ImportContactDataID int)
			insert into @importContactDataIDs
			select ImportContactDataID from ImportContactData  (NOLOCK) where JobID = @LeadAdapterJobLogID AND IsDuplicate=0
			
			INSERT INTO DebugLogs
			SELECT @AccountID, 'in while loop to get data from sql clr' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()

			while exists(select 1 from @importContactDataIDs)
			BEGIN
				declare @currentImportContactDataID1 INT
				declare @submittedData varchar(max)

				select top 1 @currentImportContactDataID1 = ImportContactDataID from @importContactDataIDs
				BEGIN TRY
					DECLARE @serviceUri VARCHAR(1024)
					SELECT @serviceUri = Value FROM EnvironmentSettings (nolock) WHERE Name = 'SmarttouchServiceUri'
					select @submittedData = [dbo].[GetImportRowData](@serviceUri + '/GetImportRowData?newDataId=' + cast(@currentImportContactDataID1 as varchar(50)) + '&oldDataId=0')
				END TRY
				BEGIN CATCH
					set @submittedData = ''
				END CATCH

				UPDATE dbo.LeadAdapterJobLogDetails
					SET SubmittedData = case when @submittedData = '' then LAJ.SubmittedData else @submittedData + '}' end
					FROM dbo.LeadAdapterJobLogDetails LAJ
					INNER JOIN ImportContactData ICD  (NOLOCK) ON LAJ.ReferenceID = ICD.ReferenceID
					WHERE LAJ.LeadAdapterJobLogID = @leadAdapterJobLogID and ImportContactDataID = @currentImportContactDataID1;

					delete from @importContactDataIDs where ImportContactDataID = @currentImportContactDataID1;
			END
		END
		--;with submittedDataQ
		--as
		--(
		--	select ICD.JobID, ICD.ReferenceID

		--	, [dbo].[GetImportRowData]('https://services.smarttouchinteractive.com/GetImportRowData?newDataId=' + cast(ICD.ImportContactDataID as varchar(50)) + '&oldDataId=0') as SubmittedData 

		--	--,'https://services.smarttouchinteractive.com/GetImportRowData?newDataId=' + cast(ICD.ImportContactDataID as varchar(50)) + '&oldDataId=0' + '}' as SubmittedData 

		--	, LAJ.SubmittedData as OrginalSubmittedData
		--	FROM dbo.LeadAdapterJobLogDetails LAJ
		--		INNER JOIN dbo.ImportContactData ICD ON LAJ.LeadAdapterJobLogID = ICD.JobID AND IsDuplicate=0
		--			AND LAJ.ReferenceID = ICD.ReferenceID
		--	WHERE LAJ.LeadAdapterJobLogID = @leadAdapterJobLogID AND ICD.ValidEmail = 1 
		--)
		----select * from submittedDataQ
		--UPDATE dbo.LeadAdapterJobLogDetails
		--	SET SubmittedData = case when ICD.SubmittedData = '' then OrginalSubmittedData else ICD.SubmittedData + '}' end
		--	FROM dbo.LeadAdapterJobLogDetails LAJ
		--		INNER JOIN submittedDataQ ICD ON LAJ.ReferenceID = ICD.ReferenceID
		--	WHERE LAJ.LeadAdapterJobLogID = @leadAdapterJobLogID
		INSERT INTO dbo.IndexData(ReferenceID, EntityID, IndexType, CreatedOn, IndexedOn, [Status])
		SELECT NEWID(), ContactID, 1, GETUTCDATE(), NULL, 1
			FROM dbo.ImportContactData(NOLOCK)
			WHERE JobID = @leadAdapterJobLogID AND ContactID > 0
				AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
				AND ContactID > 0 AND ValidEmail = 1
		UNION ALL
		SELECT NEWID(), CompanyID, 1, GETUTCDATE(), NULL, 1
			FROM dbo.ImportContactData(NOLOCK)
			WHERE JobID = @leadAdapterJobLogID AND CompanyID > 0
			AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL) AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
			AND CompanyID > 0 AND ValidEmail = 1
		INSERT INTO DebugLogs
				SELECT @AccountID, 'After inserting into index data ' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()

		UPDATE LeadAdapterJobLogs SET LeadAdapterJobStatusID=1, EndDate = GETUTCDATE() WHERE LeadAdapterJobLogID = @leadAdapterJobLogID --Processing Contacts
		UPDATE dbo.LeadAdapterAndAccountMap SET LastProcessed = GETUTCDATE(), ModifiedDateTime = GETUTCDATE(), LeadAdapterServiceStatusID = 303, LeadAdapterErrorStatusID = 1 
			WHERE LeadAdapterAndAccountMapID = @leadAdapterAndAccountMapID

	SET @NeverBounceRequestID = (SELECT TOP 1 NR.NeverBounceRequestID FROM NeverBounceMappings (NOLOCK) NM
		JOIN NeverBounceRequests (NOLOCK) NR on NR.NeverBounceRequestID = NM.NeverBounceRequestID
		WHERE NM.NeverBounceEntityType = 1 AND NM.EntityID = @leadAdapterJobLogID AND NR.ServiceStatus = 900)

		IF @NeverBounceRequestID IS NOT NULL
			UPDATE NeverBounceRequests
			SET ServiceStatus = 901
			WHERE NeverBounceRequestID = @NeverBounceRequestID



		INSERT INTO DebugLogs
		SELECT @AccountID, 'after updating leadadapterjoblog for process end date ' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()

		IF @DupUpdate = 0
		BEGIN
			UPDATE ImportContactData SET ContactStatusID = 0, ContactID = null WHERE ContactStatusID=0 AND JobID = @leadAdapterJobLogID AND IsDuplicate=0
			UPDATE A
			SET A.ReferenceID = NEWID()
			FROM LeadAdapterJobLogDetails A
			JOIN ImportContactData B  (NOLOCK) ON A.LeadAdapterJobLogID = B.JobID AND A.ReferenceID = B.ReferenceID AND IsDuplicate=0
			WHERE B.JobID = @leadAdapterJobLogID AND ContactStatusID=0
		END

		IF (@SourceType = 1 OR @SourceType =14)
			BEGIN
			DECLARE @SubmissionFieldID int = 0
			DECLARE @FileNameFieldID int = 0
			select TOP 1 @SubmissionFieldID = FieldID from Fields (NOLOCK)  where accountid = @AccountID and IsLeadAdapterField = 1 and LeadAdapterType = @SourceType and Title like '%Lead Submission ID%'
			select TOP 1 @FileNameFieldID = FieldID from Fields (NOLOCK)  where accountid = @AccountID and IsLeadAdapterField = 1 and LeadAdapterType = @SourceType and Title like '%File Name%'
			INSERT INTO @CustomFieldData(ContactID, LeadSubmissionID)
				select c.ContactID,ld.LeadAdapterJobLogDetailID as LeadSubmissionID from LeadAdapterJobLogDetails ld
				inner join Contacts c (NOLOCK)  on ld.ReferenceID = c.ReferenceID 
				where LeadAdapterJobLogID = @leadAdapterJobLogID AND ld.LeadAdapterJobLogDetailID > 0
			IF (@SubmissionFieldID > 0)
				BEGIN
					INSERT INTO DBO.ContactCustomFieldMap(ContactID, CustomFieldID, Value) SELECT CFD.ContactID, @SubmissionFieldID, CFD.LeadSubmissionID FROM @CustomFieldData CFD
				END
			IF (@FileNameFieldID > 0)
				BEGIN
					INSERT INTO DBO.ContactCustomFieldMap(ContactID, CustomFieldID, Value) SELECT CFD.ContactID, @FileNameFieldID, @fileName FROM @CustomFieldData CFD
				END
			END

		IF (@TrackMessages = 1)
		BEGIN
			INSERT INTO .Workflow.TrackMessages (MessageID, LeadScoreConditionType, EntityID, LinkedEntityID, ContactID, UserID, AccountID, ConditionValue, CreatedOn, MessageProcessStatusID)
			SELECT NEWID(), 22, @LeadAdapterAndAccountMapID, 0, ContactID, @OwnerID, AccountID, NULL, GETUTCDATE(), 701
				FROM dbo.ImportContactData(NOLOCK)
				WHERE JobID = @leadAdapterJobLogID AND ContactID > 0 AND ValidEmail = 1 and ContactStatusID <> 0 AND IsDuplicate=0
		END

		INSERT INTO dbo.LeadScoreMessages (LeadScoreMessageID, LeadScoreConditionType, EntityID, LinkedEntityID, ContactID, UserID, AccountID, ConditionValue, CreatedOn, LeadScoreProcessStatusID)
		SELECT  NEWID(), 8, ContactID,LeadSourceID, ContactID, @OwnerID, AccountID, NULL, GETUTCDATE(), 701
		FROM dbo.ImportContactData(NOLOCK)
				WHERE JobID = @leadAdapterJobLogID AND ContactID > 0 AND ValidEmail = 1 and ContactStatusID <> 0 AND IsDuplicate=0
		UNION 
		SELECT  NEWID(), 8, ContactID,DropDownValueID, ContactID, @OwnerID, ICD.AccountID, NULL, GETUTCDATE(), 701
		FROM dbo.ImportContactData(NOLOCK) ICD
		INNER JOIN DropDownValues D (NOLOCK) ON D.DropDownValue = ICD.LeadSource and D.AccountID = ICD.AccountID
				WHERE JobID = @leadAdapterJobLogID AND ContactID > 0 AND ValidEmail = 1 and ContactStatusID <> 0 AND IsDuplicate=0
				and dropdownid = 5  and d.IsDeleted = 0

		DECLARE @NSubject VARCHAR(100) = '[|A file '+ @FileName +' has been successfully completed|]'
		IF @SourceType = 11
		BEGIN
			INSERT INTO Notifications
			SELECT @leadAdapterJobLogID, @NSubject, @NSubject, GETUTCDATE(), 1 ,@OwnerID,23, NULL,1
		END
		ELSE
		BEGIN
			INSERT INTO Notifications
			SELECT @leadAdapterJobLogID, @NSubject, @NSubject, GETUTCDATE(), 1 ,@OwnerID,19, NULL,1
		END		

		--TODO: fix this
		Update C
		set C.ContactSource = @ContactSource
		from Contacts C
		join ImportContactData (NOLOCK) IDC on C.ContactID = IDC.ContactID
		where JobID = @leadAdapterJobLogID and ContactStatusID =1 and C.ContactSource is null AND IsDuplicate=0
		
		INSERT INTO DebugLogs
		SELECT @AccountID, 'after updating contact source ' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()

		update C
		set C.ContactSource = @ContactSource
		from Contacts_Audit C
		join ImportContactData IDC (nolock) on C.ContactID = IDC.ContactID
		where JobID = @leadAdapterJobLogID and ContactStatusID =1 and C.ContactSource is null and C.AuditAction='I' AND IsDuplicate=0

		INSERT INTO DebugLogs
		SELECT @AccountID, 'after updating contact source in contacts audit ' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()

		IF @ContactSource = 1
		BEGIN
			declare @dupIdList table
			(
				ImportContactDataID int
			);
			insert into @dupIdList
			select ImportContactDataID from ImportContactData (nolock) where JobID = @leadAdapterJobLogID and IsDuplicate = 1
			order by importcontactdataid asc
			DECLARE @dupCounter INT = 1
			while exists(select 1 from @dupIdList)
			 begin
				declare @currentImportContactDataID int
				declare @currentLeadAdapterJobLogID int

				select top 1 @currentImportContactDataID = ImportContactDataID from @dupIdList
				-- Add 5 minutes to the created date time so that duplicate will process after 5 minutes. HDST-4736
				insert into LeadAdapterJobLogs(LeadAdapterAndAccountMapID,StartDate,EndDate,LeadAdapterJobStatusID,Remarks,FileName,CreatedBy,CreatedDateTime,StorageName,OwnerID)
				select LeadAdapterAndAccountMapID,StartDate,EndDate,1,Remarks,FileName,CreatedBy,DATEADD(MINUTE,@dupCounter*5, GETUTCDATE()) ,StorageName,OwnerID from LeadAdapterJobLogs (NOLOCK) 
				where LeadAdapterJobLogID = @LeadAdapterJobLogID;

				select @currentLeadAdapterJobLogID = SCOPE_IDENTITY();
				update  ImportContactData set JobID = @currentLeadAdapterJobLogID, IsDuplicate = 0 where ImportContactDataID = @currentImportContactDataID;

				update LeadAdapterJobLogs set LeadAdapterJobStatusID = 7 where  LeadAdapterJobLogID = @currentLeadAdapterJobLogID;
				delete from @dupIdList where ImportContactDataID = @currentImportContactDataID;
				set @dupCounter = @dupCounter +1
			 end	
		END	

		
	END
	END TRY
	BEGIN CATCH
		
		UPDATE LeadAdapterJobLogs SET LeadAdapterJobStatusID=4, EndDate = GETUTCDATE() WHERE LeadAdapterJobLogID = @leadAdapterJobLogID --Processing Contacts

		INSERT INTO dbo.IndexData(ReferenceID, EntityID, IndexType, CreatedOn, IndexedOn, [Status])
		SELECT NEWID(), ContactID, 1, GETUTCDATE(), NULL, 1
			FROM dbo.ImportContactData(NOLOCK)
			WHERE JobID = @leadAdapterJobLogID AND ContactID > 0
			AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL) AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
			AND ContactID > 0 AND ValidEmail = 1
		UNION ALL
		SELECT NEWID(), CompanyID, 1, GETUTCDATE(), NULL, 1
			FROM dbo.ImportContactData(NOLOCK)
			WHERE JobID = @leadAdapterJobLogID AND CompanyID > 0
			AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL) AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
			AND CompanyID > 0

		INSERT INTO CRMLogs.dbo.CRMDBLogs(UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

	END CATCH
	UPDATE	StoreProcExecutionResults
	SET		EndTime = GETDATE(),
			TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
			Status = 'C'
	WHERE	ResultID = @ResultID
END
