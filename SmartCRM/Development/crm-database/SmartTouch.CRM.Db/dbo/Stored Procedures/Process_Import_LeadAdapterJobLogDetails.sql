create PROCEDURE [dbo].[Process_Import_LeadAdapterJobLogDetails] 
	@leadAdapterJobLogID INT
	,@AccountID int
	,@DuplicateLogic int
	,@DupUpdate int
	,@OwnerID int
AS
BEGIN
		DECLARE @ResultID INT
		INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		VALUES('Process_Import_LeadAdapterJobLogDetails', @AccountID, '@LeadAdapterJobLogID:' + cast(@LeadAdapterJobLogID as varchar(10)))
		SET @ResultID = scope_identity()

		SELECT @leadAdapterJobLogID as LeadAdapterJobLogID
											, CASE WHEN IsBuilderNumberPass = 0 THEN 6 
				                            WHEN IsCommunityNumberPass = 0 THEN 9
											WHEN ValidEmail = 0 THEN 4 												  
											WHEN ContactStatusID = 0 AND @DupUpdate = 0 THEN 2
											ELSE ContactStatusID END as LeadAdapterRecordStatusID, '' as Remarks, 
			@OwnerID as CreatedBy, GETUTCDATE() as CreatedDateTime, LeadAdapterRowData as RowData, ReferenceID
			, LeadAdapterSubmittedData as SubmittedData
			,Row_Number() over (order by ReferenceID)as RowNumber
			into #tLeadAdapterJobLogDetails 
			FROM dbo.ImportContactData (NOLOCK) 
			WHERE JobID = @leadAdapterJobLogID

		UPDATE dbo.LeadAdapterJobLogDetails
			SET LeadAdapterRecordStatusID = 4
			FROM dbo.LeadAdapterJobLogDetails LAJ
				INNER JOIN dbo.ImportContactData ICD  (NOLOCK) ON LAJ.LeadAdapterJobLogID = ICD.JobID AND IsDuplicate=0
					AND LAJ.ReferenceID = ICD.ReferenceID
			WHERE LAJ.LeadAdapterJobLogID = @leadAdapterJobLogID AND ICD.ValidEmail = 0;

		declare  @counter  int = 0;
		declare  @rowCount int = 0;
		declare @batchCount int = 1000;
		select @rowCount = Count(1) from #tLeadAdapterJobLogDetails

		WHILE (1 = 1)
		BEGIN
			insert into dbo.LeadAdapterJobLogDetails (LeadAdapterJobLogID, LeadAdapterRecordStatusID, Remarks, CreatedBy, CreatedDateTime, RowData, ReferenceID, SubmittedData)
			SELECT LeadAdapterJobLogID, LeadAdapterRecordStatusID, Remarks, CreatedBy, CreatedDateTime, RowData, ReferenceID, SubmittedData
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
