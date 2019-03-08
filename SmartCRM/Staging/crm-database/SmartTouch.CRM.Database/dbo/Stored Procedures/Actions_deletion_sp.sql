
CREATE PROCEDURE [dbo].[Actions_deletion_sp]
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


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN

				DELETE	cam
				FROM	DBO.ContactActionMap AS cam INNER JOIN(
							SELECT	top (@RecordPerBatch) ContactActionMapID
							FROM	DBO.ContactActionMap AS ca WITH (NOLOCK) INNER JOIN dbo.Actions AS A ON ca.ActionID=A.ActionID  
							WHERE	A.AccountID = @accountid  
						) tmp on tmp.ContactActionMapID = cam.ContactActionMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ContactActionMap_ActionsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactActionMap_Actions'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN

				DELETE	camA
				FROM	DBO.ContactActionMap_Audit AS camA INNER JOIN(
							SELECT	top (@RecordPerBatch) ContactActionMapID
							FROM	DBO.ContactActionMap_Audit AS ca WITH (NOLOCK) INNER JOIN dbo.Actions AS A ON ca.ActionID=A.ActionID  
							WHERE	A.AccountID = @accountid  
						) tmp on tmp.ContactActionMapID = camA.ContactActionMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ContactActionMap_Audit_ActionsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactActionMap_Audit_Actions'
   
			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	atm
				FROM	dbo.ActionTagMap AS atm INNER JOIN(
							SELECT TOP (@RecordPerBatch) ActionTagMapID
							FROM dbo.ActionTagMap AS am (NOLOCK)
							  INNER JOIN dbo.Actions AS A ON am.ActionID=A.ActionID 
							WHERE	A.AccountID = @accountid 
						) tmp on tmp.ActionTagMapID = atm.ActionTagMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ActionTagMap_ActionsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ActionTagMap_Actions'


			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	OAM
				FROM	dbo.OpportunityActionMap  AS OAM INNER JOIN(
							SELECT TOP (@RecordPerBatch) OpportunityActionMapID
							FROM dbo.OpportunityActionMap AS OAM (NOLOCK)
							  INNER JOIN dbo.Actions AS AN ON OAM.ActionID=AN.ActionID
							WHERE	AN.AccountID = @AccountID 
						) tmp on tmp.OpportunityActionMapID = OAM.OpportunityActionMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ActionTagMap_ActionsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityActionMap_Actions'


			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	OAMA
				FROM	dbo.OpportunityActionMap_Audit  AS OAMA INNER JOIN(
							SELECT TOP (@RecordPerBatch) OpportunityActionMapID
							FROM dbo.OpportunityActionMap_Audit AS OAM (NOLOCK)
							  INNER JOIN dbo.Actions AS AN ON OAM.ActionID=AN.ActionID
							WHERE	AN.AccountID = @AccountID 
						) tmp on tmp.OpportunityActionMapID = OAMA.OpportunityActionMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ActionTagMap_Audit_ActionsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityActionMap_Audit_Actions'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	TA
				FROM	dbo.TrackActions  AS TA INNER JOIN(
							SELECT TOP (@RecordPerBatch) TrackActionID
							FROM dbo.TrackActions AS TAS (NOLOCK)
							  INNER JOIN dbo.Actions AS A ON TAS.ActionID=A.ActionID
							WHERE	A.AccountID = @AccountID 
						) tmp on tmp.TrackActionID = TA.TrackActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT TrackActions_ActionsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from TrackActions_Actions'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WFTA
				FROM	Workflow.TrackActions AS WFTA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) TrackActionID
							FROM Workflow.TrackActions AS WTA (NOLOCK)
							  INNER JOIN [dbo].Actions AS A ON WTA.ActionID = A.ActionID 
							WHERE	A.AccountID = @accountid 
						) tmp on tmp.TrackActionID = WFTA.TrackActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowTrackActions_ActionsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Workflow.TrackActions_Actions'

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	A
				FROM	dbo.Actions AS A  INNER JOIN(
							SELECT TOP (@RecordPerBatch) ActionID
							FROM dbo.Actions AS A (NOLOCK)
							  INNER JOIN dbo.Users AS U ON A.CreatedBy=U.UserID 
							WHERE	U.AccountID = @Accountid 
						) tmp on tmp.ActionID = A.ActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT Actions_UsersCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Actions_Users'

BEGIN
			DELETE	A1
			FROM	dbo.Actions  AS A1  (NOLOCK)
			WHERE	A1.AccountID = @Accountid 
	        SELECT @@ROWCOUNT Actions1COUNT
END
			PRINT  ' records deleted from  Actions'

BEGIN
			DELETE	AA1
			FROM	dbo.Actions_Audit  AS AA1   (NOLOCK)
			WHERE	AA1.AccountID = @Accountid 
	        SELECT @@ROWCOUNT Actions_Audit1
END
			PRINT  ' records deleted from  Actions_Audit'

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
	EXEC [dbo].[Actions_deletion_sp]
		@AccountID	= 100

*/

