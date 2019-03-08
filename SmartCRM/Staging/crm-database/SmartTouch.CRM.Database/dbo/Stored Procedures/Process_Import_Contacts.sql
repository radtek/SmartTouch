
CREATE PROCEDURE [dbo].[Process_Import_Contacts] 
	@leadAdapterJobLogID INT
	,@accountID INT
	,@ownerId INT
	,@ContactSource TINYINT
	,@SourceType INT
	,@logicId tinyint  = 0
	,@IncludeInReports bit = 1
AS
BEGIN
		DECLARE @ResultID INT
		DECLARE @CreatedByUserID int
		INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		VALUES('Process_Import_Contacts', @AccountID, '@LeadAdapterJobLogID:' + cast(@LeadAdapterJobLogID as varchar(10)))
		SET @ResultID = scope_identity()
		SELECT @CreatedByUserID = value FROM EnvironmentSettings (NOLOCK) WHERE Name = 'CRM User'







;WITH CTE AS (
			SELECT 1 as ImportContactDataID
,FirstName
,LastName
,CompanyName
,Title
,LeadSource
,LifecycleStage
,PartnerType
,DoNotEmail
,HomePhone
,MobilePhone
,WorkPhone
,AccountID
,PrimaryEmail
,SecondaryEmails
,FacebookUrl
,TwitterUrl
,GooglePlusUrl
,LinkedInUrl
,BlogUrl
,WebSiteUrl
,AddressLine1
,AddressLine2
,City
,State
,Country
,ZipCode
,CustomFieldsData
,ContactID
,ContactStatusID
,ReferenceID
,ContactTypeID
,OwnerID
,JobID
,LeadSourceID
,LifecycleStageID
,PartnerTypeID
,LoopID
,CompanyID
,PhoneData
,CommunicationID
,EmailExists
,IsBuilderNumberPass
,LeadAdapterSubmittedData
,LeadAdapterRowData
,ValidEmail
,OrginalRefId
,IsDuplicate
,IsCommunityNumberPass, 0 AS RowNumber FROM dbo.ImportContactData WHERE 1 <> 1
		)
		
		SELECT * INTO #tImportContactData FROM CTE;
		IF @logicId = 1
		BEGIN
			WITH currentData
			AS (SELECT *,ROW_NUMBER() OVER(ORDER BY ImportContactDataID) AS RowNumber FROM dbo.ImportContactData
			WHERE JobID = @leadAdapterJobLogID
			AND AccountId = @accountID
			AND (IsBuilderNumberPass = 1
			OR IsBuilderNumberPass IS NULL)
			AND (IsCommunityNumberPass = 1
			OR IsCommunityNumberPass IS NULL)
			AND LEN(PrimaryEmail) > 0
			AND ValidEmail = 1)
			INSERT INTO #tImportContactData
			SELECT * from currentData;
		END
		ELSE IF @logicId = 2
		BEGIN
			WITH currentData
			AS (SELECT *,ROW_NUMBER() OVER(ORDER BY ImportContactDataID) AS RowNumber FROM dbo.ImportContactData
			WHERE JobID = @leadAdapterJobLogID
			AND AccountId = @accountID
			AND (IsBuilderNumberPass = 1
			OR IsBuilderNumberPass IS NULL)
			AND (IsCommunityNumberPass = 1
			OR IsCommunityNumberPass IS NULL)
			AND ValidEmail = 1
			AND ((LEN(PrimaryEmail) > 0)
			OR (LEN(FirstName) > 0
			AND LEN(LastName) > 0)))
			INSERT INTO #tImportContactData
			SELECT * from currentData;
		END
		ELSE IF @logicId = 3
		BEGIN
			 WITH CurrentData
			AS (SELECT *,ROW_NUMBER() OVER(ORDER BY ImportContactDataID) AS RowNumber FROM dbo.ImportContactData
			WHERE JobID = @leadAdapterJobLogID
			AND AccountId = @AccountID
			AND (IsBuilderNumberPass = 1
			OR IsBuilderNumberPass IS NULL)
			AND ValidEmail = 1
			AND (IsCommunityNumberPass = 1
			OR IsCommunityNumberPass IS NULL)
			AND (LEN(PrimaryEmail) > 0
			OR (LEN(FirstName) > 0
			AND LEN(LastName) > 0)))
			INSERT INTO #tImportContactData
			SELECT * from currentData;
		END
		ELSE IF @logicId = 4
		BEGIN
			 WITH CurrentData
			AS (SELECT *,ROW_NUMBER() OVER(ORDER BY ImportContactDataID) AS RowNumber FROM dbo.ImportContactData
			WHERE JobID = @LeadAdapterJobLogID
			AND AccountId = @AccountID
			AND (IsBuilderNumberPass = 1
			OR IsBuilderNumberPass IS NULL)
			AND (IsCommunityNumberPass = 1
			OR IsCommunityNumberPass IS NULL)
			AND ValidEmail = 1
			AND (LEN(PrimaryEmail) > 0
			OR (LEN(FirstName) > 0
			AND LEN(LastName) > 0)))
			INSERT INTO #tImportContactData
			SELECT * from currentData;
		END

		CREATE INDEX T_IDX_tImportContactData_ ON #tImportContactData(ContactId, AccountId, ContactStatusId);


		declare @counter int = 0;
		declare @rowCount int = 0;
		declare @batchCount int = 1000;
		select @rowCount = Count(1) from #tImportContactData

		--Update Contacts
		WHILE (1 = 1 AND @logicId NOT IN (3,4))
		BEGIN
				INSERT INTO DebugLogs
				SELECT @AccountID, 'Before updating duplicate contacts ' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()
			update C
					SET FirstName = ICD.FirstName,
					LastName = ICD.LastName,
					Company = ICD.CompanyName,
					Title = ICD.Title,
					AccountID = ICD.AccountID,
					HomePhone = ICD.HomePhone,
					WorkPhone = ICD.WorkPhone,
					MobilePhone = ICD.MobilePhone,
					PrimaryEmail = ICD.PrimaryEmail,
					LifecycleStage = ICD.LifecycleStageID,
					PartnerType = ICD.PartnerTypeID,
					DoNotEmail = ICD.DoNotEmail,
					IsDeleted = 0,
					LastUpdatedBy =
                       CASE
                         WHEN ICD.OwnerID > 0 THEN ICD.OwnerID
						 WHEN C.OwnerID > 0 THEN C.OwnerID
                         ELSE @ownerId
                       END,
					LastUpdatedOn = GETUTCDATE(),
					ContactSource = @ContactSource,
					SourceType = @SourceType,
					OwnerID =
							 CASE
							   WHEN ICD.OwnerID > 0 THEN ICD.OwnerID
							   WHEN C.OwnerID > 0 THEN C.OwnerID
							   ELSE @ownerId
							 END,
					CompanyID = ICD.CompanyID,
					IncludeInReports= @IncludeInReports
			FROM Contacts C
			join #tImportContactData ICD ON C.ContactId = ICD.ContactId AND C.AccountId = ICD.AccountId AND C.IsDeleted = 0 AND ICD.ContactStatusId = 3
			where ICD.RowNumber between (@counter * @batchCount + 1) and ((@counter * @batchCount) + @batchCount) AND ICD.IsDuplicate =0;
	 
			IF (@@ROWCOUNT = 0  AND @rowCount < ((@counter * @batchCount) + @batchCount))
			BEGIN
				BREAK
			END
	 
			set @counter = @counter + 1;
		END


		SET @counter = 0;
		--Insert Contacts
		WHILE (1 = 1)
		BEGIN
				INSERT INTO DebugLogs
				SELECT @AccountID, 'Before inserting contacts ' + cast(@LeadAdapterJobLogID as varchar(10)) , GETUTCDATE()
			INSERT INTO Contacts (FirstName, LastName, Company, CommunicationID, Title, AccountID, LeadSource, PrimaryEmail, ContactType, LifecycleStage,
				PartnerType, DoNotEmail, IsDeleted, ReferenceID, LastUpdatedBy, LastUpdatedOn, FirstContactSource, FirstSourceType, OwnerID, CompanyID,IncludeInReports)
			SELECT FirstName, LastName, ICD.CompanyName, NULL, Title, ICD.AccountID, ICD.LeadSourceID, PrimaryEmail, ContactTypeID, LifecycleStageID,
				PartnerTypeID, DoNotEmail, 0, ICD.ReferenceID, CASE
				WHEN ICD.OwnerID > 0 THEN ICD.OwnerID
					ELSE @ownerId
					END
				, GETUTCDATE(),
				@ContactSource, @SourceType,CASE
				WHEN ICD.OwnerID > 0 THEN ICD.OwnerID
				ELSE @ownerId
					END
				, ICD.CompanyID,@IncludeInReports bit
			FROM #tImportContactData ICD 
			WHERE ISNULL(ICD.ContactID,0) = 0 AND ICD.ContactStatusID <> 0 AND ICD.RowNumber between (@counter * @batchCount + 1) and ((@counter * @batchCount) + @batchCount)
			AND ICD.IsDuplicate = 0  ;

			IF (@@ROWCOUNT = 0  AND @rowCount < ((@counter * @batchCount) + @batchCount))
			BEGIN
				BREAK
			END
	 
			set @counter = @counter + 1;
		END

		

		UPDATE	StoreProcExecutionResults
		SET		EndTime = GETDATE(),
				TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
				Status = 'C'
		WHERE	ResultID = @ResultID
END
