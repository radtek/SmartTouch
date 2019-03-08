CREATE     PROCEDURE [dbo].[Deleting_LeadScoreRules_sp]
(
@AccountID INT = 0
)
AS 
BEGIN 
SET NOCOUNT ON
BEGIN TRY
BEGIN TRANSACTION

             DECLARE @TotalRecordsDeleted int = 1,
					 @RecordsDeleted int = 1,
					 @RecordPerBatch int = 5000

SELECT COUNT(*)count FROM WorkflowContactFieldAction   WITH (NOLOCK) WHERE  FieldID IN (SELECT FieldID FROM  Fields WHERE Accountid = @Accountid )

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	LS
				FROM	dbo.LeadScores AS LS INNER JOIN(
							SELECT TOP (@RecordPerBatch) LeadScoreID
							FROM dbo.LeadScores AS LS WITH (NOLOCK)
							  INNER JOIN dbo.LeadScoreRules AS LSR ON LS.LeadScoreRuleID=LSR.LeadScoreRuleID  
							WHERE	LSR.AccountID = @accountid 
						) tmp on tmp.LeadScoreID = LS.LeadScoreID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted as  LeadScores
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadScores'



SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	LS
				FROM	dbo.LeadScoreConditionValues AS LS INNER JOIN(
							SELECT TOP (@RecordPerBatch) LeadScoreConditionValueID
							FROM dbo.LeadScoreConditionValues AS LS WITH (NOLOCK)
							  INNER JOIN dbo.LeadScoreRules AS LSR ON LS.LeadScoreRuleID=LSR.LeadScoreRuleID  
							WHERE	LSR.AccountID = @accountid 
						) tmp on tmp.LeadScoreConditionValueID = LS.LeadScoreConditionValueID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted as  LeadScoreConditionValues
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadScoreConditionValues'

				BEGIN
				DELETE	LSR1
				FROM	dbo.LeadScoreRules  AS LSR1 WITH (NOLOCK)
				WHERE	LSR1.AccountID = @Accountid 
             select @@rowcount  LeadScoreRules1COUNT
			END
			PRINT  ' records deleted from  LeadScoreRules1'
			

SELECT @@ROWCOUNT TotalCount
--successfull execution query-- 
SELECT 'DEL-001' ResultCode 

 Commit TRANSACTION 
	END TRY

BEGIN CATCH
	ROLLBACK TRANSACTION
		--Unsuccessful execution query-- 
		SELECT 'DEL-002' ResultCode 
		--Error blocking statement in between catch --
		INSERT INTO SmartCRM_Test.dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

END CATCH
	SET NOCOUNT OFF
END 


/*
	EXEC [dbo].[Deleting_LeadScoreRules_sp]
		@AccountID	= 94

*/



