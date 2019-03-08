CREATE  PROCEDURE [dbo].[Deleting_Tours_sp]
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

SELECT COUNT(*)count   FROM ContactTourMap WHERE TourID IN (SELECT TourID FROM Tours WHERE Accountid = @Accountid )
SELECT COUNT(*)count FROM ContactTourMap_Audit WHERE TourID IN (SELECT TourID FROM Tours WHERE Accountid = @Accountid )
SELECT COUNT(*)count FROM Tours WHERE Accountid = @Accountid
SELECT COUNT(*)count FROM Tours_Audit WHERE Accountid = @Accountid

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CT
				FROM	dbo.ContactTourMap AS  CT INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactTourMapID
							FROM dbo.ContactTourMap AS CTM (NOLOCK)
							  INNER JOIN  dbo.Tours AS T ON CTM.TourID = T.TourID
							WHERE	T.AccountID = @AccountID
						) tmp on tmp.ContactTourMapID = CT.ContactTourMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ContactTourMap_ToursCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactTourMap_Tours '

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CTM
				FROM	dbo.ContactTourMap_Audit AS  CTM INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.ContactTourMap_Audit AS CTMA (NOLOCK)
							  INNER JOIN  dbo.Tours AS T ON CTMA.TourID = T.TourID
							WHERE	T.AccountID = @AccountID
						) tmp on tmp.AuditId = CTM.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ContactTourMap_Audit_ToursCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactTourMap_Audit_Tours '


	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CTM
				FROM	dbo.UserTourMap AS  CTM INNER JOIN(
							SELECT TOP (@RecordPerBatch) UserTourMapId
							FROM dbo.UserTourMap AS CTMA (NOLOCK)
							  INNER JOIN  dbo.Tours AS T ON CTMA.TourID = T.TourID
							WHERE	T.AccountID = @AccountID
						) tmp on tmp.UserTourMapId = CTM.UserTourMapId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT UserTourMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from UserTourMap '



          BEGIN
				DELETE	T1
				FROM	dbo.Tours  AS T1  (NOLOCK)
				WHERE	T1.AccountID = @Accountid 
	         select @@rowcount Tours1COUNT
			END
		  PRINT  ' records deleted from  Tours'


		   BEGIN
				DELETE	T1
				FROM	dbo.Tours_Audit  AS T1  (NOLOCK)
				WHERE	T1.AccountID = @Accountid 
	         select @@rowcount Tours_AuditCOUNT
			END
		  PRINT  ' records deleted from  Tours_Audit'




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
	EXEC [dbo].[Deleting_Tours_sp]
		@AccountID	= 19

*/



