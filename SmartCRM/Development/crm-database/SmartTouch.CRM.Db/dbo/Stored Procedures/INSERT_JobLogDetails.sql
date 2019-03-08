
CREATE PROCEDURE [dbo].[INSERT_JobLogDetails]
	(
		@leadAdapterAndAccountMapID int,
		@remarks					varchar(100),
		@filename					varchar(100),
		@importuniqueidentifier     uniqueidentifier,
		@updateonduplicate          bit,
		@isfromimport				bit,
		@AccountID					int,
		@UserID						int,
		@DuplicateLogic				tinyint,
		@OwnerId					int,
		@LeadAdapterDetails			AS dbo.LeadAdapterDetails readonly,
		@JobLogID					int output
	)
AS
BEGIN
	
	SET NOCOUNT ON 
	BEGIN TRY
	BEGIN TRANSACTION

		DECLARE @LeadAdapterID	int
		
		INSERT INTO [dbo].[LeadAdapterJobLogs] (LeadAdapterAndAccountMapID, StartDate, EndDate, LeadAdapterJobStatusID, [FileName], CreatedBy, CreatedDateTime,OwnerID)
		SELECT @leadAdapterAndAccountMapID, GETUTCDATE(), GETUTCDATE(), 4, @filename, @UserID, GETUTCDATE(),@OwnerId

		SET @LeadAdapterID = (SELECT IDENT_CURRENT('LeadAdapterJobLogs'))
		SET @JobLogID = @LeadAdapterID

		IF (CONVERT(smallint, @isfromimport) = 1)
			BEGIN
				INSERT INTO dbo.ImportDataSettings (UpdateOnDuplicate, UniqueImportIdentifier, AccountID, ProcessBy, ProcessDate, DuplicateLogic, LeadAdaperJobID)
				VALUES (@updateonduplicate, @importuniqueidentifier, @AccountID, @UserID, GETUTCDATE(), @DuplicateLogic, @LeadAdapterID)
			END

		--INSERT INTO [dbo].[LeadAdapterJobLogDetails]([LeadAdapterJobLogID], [LeadAdapterRecordStatusID], [CreatedBy], [CreatedDateTime], [ReferenceID],[RowData],[SubmittedData]) 
		--SELECT @LeadAdapterID, LeadAdapterRecordStatusID, @UserID, GETUTCDATE(), ReferenceID,RowData,SubmittedData FROM @LeadAdapterDetails
		
	COMMIT TRANSACTION
	END TRY
	BEGIN CATCH

		SELECT ErrorNumber = ERROR_NUMBER(), ErrorSeverity = ERROR_SEVERITY(), ErrorState = ERROR_STATE(),
			ErrorProcedure = ERROR_PROCEDURE(), ErrorLine = ERROR_LINE(), ErrorMessage = ERROR_MESSAGE()

	END CATCH
	SET NOCOUNT OFF

END










