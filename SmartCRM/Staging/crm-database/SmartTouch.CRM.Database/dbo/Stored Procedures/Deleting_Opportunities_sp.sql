CREATE    PROCEDURE [dbo].[Deleting_Opportunities_sp]
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
				DELETE	D
				FROM	dbo.Documents AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) DocumentID
							FROM dbo.Documents AS D (NOLOCK)
							  INNER JOIN dbo.Opportunities AS O ON D.OpportunityID=O.OpportunityID 
							WHERE	O.AccountID = @accountid 
						) tmp on tmp.DocumentID = D.DocumentID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT DocumentsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Documents'

					SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	ORM
				FROM	dbo.OpportunitiesRelationshipMap AS ORM INNER JOIN(
							SELECT TOP (@RecordPerBatch) OpportunityRelationshipMapID
							FROM dbo.OpportunitiesRelationshipMap AS ORM (NOLOCK)
							  INNER JOIN dbo.Opportunities AS O ON ORM.OpportunityID=O.OpportunityID 
							WHERE	O.AccountID = @accountid 
						) tmp on tmp.OpportunityRelationshipMapID = ORM.OpportunityRelationshipMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT OpportunitiesRelationshipMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunitiesRelationshipMap'


					SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	ORM
				FROM	dbo.OpportunitiesRelationshipMap_Audit AS ORM INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.OpportunitiesRelationshipMap_Audit AS ORM (NOLOCK)
							  INNER JOIN dbo.Opportunities AS O ON ORM.OpportunityID=O.OpportunityID 
							WHERE	O.AccountID = @accountid 
						) tmp on tmp.AuditId = ORM.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT OpportunitiesRelationshipMap_Audit
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunitiesRelationshipMap_Audit'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	OAM
				FROM	dbo.OpportunityActionMap AS OAM INNER JOIN(
							SELECT TOP (@RecordPerBatch) OpportunityActionMapID
							FROM dbo.OpportunityActionMap AS OAM (NOLOCK) 
							  INNER JOIN dbo.Opportunities AS O ON OAM.OpportunityID=O.OpportunityID 
							WHERE	O.AccountID = @accountid
						) tmp on tmp.OpportunityActionMapID = OAM.OpportunityActionMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT OpportunityActionMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityActionMap'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	OAM
				FROM	dbo.OpportunityActionMap_Audit AS OAM INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.OpportunityActionMap_Audit AS OAM (NOLOCK) 
							  INNER JOIN dbo.Opportunities AS O ON OAM.OpportunityID=O.OpportunityID 
							WHERE	O.AccountID = @accountid
						) tmp on tmp.AuditId = OAM.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT OpportunityActionMap_Audit
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityActionMap_Audit'

					SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	OCM
				FROM	dbo.OpportunityContactMap AS OCM INNER JOIN(
							SELECT TOP (@RecordPerBatch) OpportunityContactMapID
							FROM dbo.OpportunityContactMap AS OCM (NOLOCK)
							  INNER JOIN dbo.Opportunities AS O ON OCM.OpportunityID=O.OpportunityID 
							WHERE	O.AccountID = @accountid 
						) tmp on tmp.OpportunityContactMapID = OCM.OpportunityContactMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT OpportunityContactMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityContactMap'

								SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	OCM
				FROM	dbo.OpportunityContactMap_Audit AS OCM INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.OpportunityContactMap_Audit AS OCM (NOLOCK)
							  INNER JOIN dbo.Opportunities AS O ON OCM.OpportunityID=O.OpportunityID 
							WHERE	O.AccountID = @accountid 
						) tmp on tmp.AuditId = OCM.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT OpportunityContactMap_Audit
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityContactMap_Audit'


				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	ONM
				FROM	dbo.OpportunityNoteMap AS ONM INNER JOIN(
							SELECT TOP (@RecordPerBatch) OpportunityNoteMapID
							FROM dbo.OpportunityNoteMap AS ONM (NOLOCK)
							  INNER JOIN dbo.Opportunities AS O ON ONM.OpportunityID=O.OpportunityID 
							WHERE	O.AccountID = @accountid  
						) tmp on tmp.OpportunityNoteMapID = ONM.OpportunityNoteMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT OpportunityNoteMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityNoteMap'

				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	ONM
				FROM	dbo.OpportunityNoteMap_Audit AS ONM INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.OpportunityNoteMap_Audit AS ONM (NOLOCK)
							  INNER JOIN dbo.Opportunities AS O ON ONM.OpportunityID=O.OpportunityID 
							WHERE	O.AccountID = @accountid  
						) tmp on tmp.AuditId = ONM.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT OpportunityNoteMap_Audit
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityNoteMap_Audit'


			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	OTM
				FROM	dbo.OpportunityTagMap AS OTM INNER JOIN(
							SELECT TOP (@RecordPerBatch) OpportunityTagMapID
							FROM dbo.OpportunityTagMap AS OTM (NOLOCK)
							  INNER JOIN dbo.Opportunities AS O ON OTM.OpportunityID=O.OpportunityID 
							WHERE	O.AccountID = @accountid 
						) tmp on tmp.OpportunityTagMapID = OTM.OpportunityTagMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT OpportunityTagMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityTagMap'


			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	OTM
				FROM	dbo.OpportunityTagMap_Audit AS OTM INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.OpportunityTagMap_Audit AS OTM (NOLOCK)
							  INNER JOIN dbo.Opportunities AS O ON OTM.OpportunityID=O.OpportunityID 
							WHERE	O.AccountID = @accountid 
						) tmp on tmp.AuditId = OTM.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT OpportunityTagMap_Audit
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityTagMap_Audit'

				BEGIN
				DELETE	O
				FROM	dbo.Opportunities  AS O (NOLOCK)
				WHERE	O.AccountID = @Accountid  
	         select @@rowcount Opportunities
			END
			PRINT  ' records deleted from  Opportunities'


			BEGIN
				DELETE	OA
				FROM	dbo.Opportunities_Audit  AS OA  (NOLOCK)
				WHERE	OA.AccountID = @Accountid  
	         select @@rowcount Opportunities_Audit
			END
		PRINT  ' records deleted from  Opportunities_Audit'


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
	EXEC [dbo].[Deleting_Opportunities_sp]
		@AccountID	= 19

*/



