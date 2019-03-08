
CREATE PROCEDURE [dbo].[Process_Import_Company] 
	@leadAdapterJobLogID INT
	,@ContactSource INT
	,@ownerId INT
	,@AccountID int
	,@sourceType int
AS
BEGIN
		DECLARE @ResultID INT
		INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		VALUES('Process_Import_Company', @AccountID, '@LeadAdapterJobLogID:' + cast(@LeadAdapterJobLogID as varchar(10)))
		SET @ResultID = scope_identity()

	
		DECLARE @ChangeResult1 TABLE (ChangeType VARCHAR(10), Id INTEGER);
		;WITH currentData
		AS (
			SELECT distinct AccountID, CompanyName FROM dbo.ImportContactData(NOLOCK) WHERE JobID = @LeadAdapterJobLogID AND AccountId = @AccountID AND LEN(CompanyName) > 0 
			AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)  AND IsDuplicate=0 AND ValidEmail = 1
			)
		select CompanyName, ICD.AccountID, ROW_NUMBER() OVER (order by iCD.CompanyName, ICD.AccountId) as RowNumber into #companyData from currentData ICD
		left join Contacts(NOLOCK) C ON C.Company = ICD.CompanyName and C.AccountId = ICD.AccountId and C.IsDeleted =0
		where C.ContactID is null

		declare @counter int = 0;
		declare @rowCount int = 0;
		declare @batchCount int = 1000;
		select @rowCount = Count(1) from #companyData

		WHILE (1 = 1)
		BEGIN
			insert into Contacts (Company,ReferenceID, AccountID, ContactType, IsDeleted, LastUpdatedOn,FirstContactSource,FirstSourceType, OwnerID, ContactSource,LastUpdatedBy)
			SELECT CompanyName, NEWID(), ICD.AccountID, 2, 0, GETUTCDATE(),@ContactSource, @sourceType, @ownerId,@ContactSource,@ownerId 
			FROM #companyData ICD 
			WHERE ICD.RowNumber between (@counter * @batchCount + 1) and ((@counter * @batchCount) + @batchCount);

			IF (@@ROWCOUNT = 0  AND @rowCount < ((@counter * @batchCount) + @batchCount))
			BEGIN
				BREAK
			END
	 
			set @counter = @counter + 1;
		END
		
		select 'Companyname:' + Company as CompnayName, ReferenceID, ROW_NUMBER() Over (order by ReferenceID) as RowNumber into #tLeadAdapterJobLogDetails from Contacts (nolock) where ContactID in 
		(
			select distinct C.ContactID from Contacts C (nolock)
			join dbo.ImportContactData ICD (nolock) ON C.Company = ICD.CompanyName AND C.AccountID = ICD.AccountID and ICD.JobID = @LeadAdapterJobLogID	
			AND JobID = @LeadAdapterJobLogID AND ICD.AccountId = @AccountID AND LEN(CompanyName) > 0 
			AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)  
			AND IsDuplicate=0 AND ValidEmail = 1 AND C.ContactType = 2	
		)
		DELETE   from #tLeadAdapterJobLogDetails where  CompnayName   in (select RowData from LeadAdapterJobLogDetails where  LeadAdapterRecordStatusID = 1)

		set  @counter  = 0;
		set  @rowCount = 0;
		select @rowCount = Count(1) from #tLeadAdapterJobLogDetails

		WHILE (1 = 1)
		BEGIN
			insert into LeadAdapterJobLogDetails (LeadAdapterJobLogID,LeadAdapterRecordStatusID,Remarks,CreatedBy,CreatedDateTime,RowData,ReferenceID,SubmittedData)
			SELECT @LeadAdapterJobLogID, 1, '' , @ownerId, GETUTCDATE(), CompnayName, ReferenceID, NULL
			FROM #tLeadAdapterJobLogDetails ICD 
			WHERE ICD.RowNumber between (@counter * @batchCount + 1) and ((@counter * @batchCount) + @batchCount);

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
GO


