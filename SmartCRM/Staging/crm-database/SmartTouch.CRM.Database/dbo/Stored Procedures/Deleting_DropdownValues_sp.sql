CREATE     PROCEDURE [dbo].[Deleting_DropdownValues_sp]
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
				delete 	sf
				FROM	[dbo].[SearchFilters] AS sf  INNER JOIN(
							SELECT TOP (@RecordPerBatch) SearchFilterID
							FROM [dbo].[SearchFilters] AS s (NOLOCK)
							  INNER JOIN [dbo].[DropdownValues] AS dv ON dv.[DropdownValueID]=s.[DropdownValueID]
							WHERE	dv.AccountID = @Accountid 
						) tmp on tmp.SearchFilterID = sf.SearchFilterID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT SearchFilters_DropdownValuesCOUNT
			END
	PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  SearchFilter_DropdownValues'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	SDV
				FROM	dbo.SubscriptionDefaultDropdownValueMap AS SDV INNER JOIN(
							SELECT TOP (@RecordPerBatch) SubscriptionDefaultDropdownValueMapID
							FROM dbo.SubscriptionDefaultDropdownValueMap AS SDDV (NOLOCK)
							  INNER JOIN dbo.DropdownValues AS DV ON SDDV.DropdownValueID=DV.DropdownValueID     
							WHERE	DV.AccountID = @accountid 
						) tmp on tmp.SubscriptionDefaultDropdownValueMapID = SDV.SubscriptionDefaultDropdownValueMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT SubscriptionDefaultDropdownValueMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from SubscriptionDefaultDropdownValueMap'


			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WCFA
				FROM	[dbo].[WorkflowContactFieldAction] AS WCFA  INNER JOIN(
							SELECT TOP (@RecordPerBatch)WorkflowContactFieldActionID
							FROM [dbo].[WorkflowContactFieldAction] AS WCA (NOLOCK)
							  INNER JOIN [dbo].[DropdownValues] AS dv ON dv.[DropdownValueID]=WCA.[DropdownValueID]
							WHERE	dv.AccountID = @Accountid 
						) tmp on tmp.WorkflowContactFieldActionID = WCFA.WorkflowContactFieldActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowContactFieldAction_DropdownValuesCOUNT
			END
            PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  WorkflowContactFieldAction_DropdownValues'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	OSG
				FROM	dbo.OpportunityStageGroups AS OSG INNER JOIN(
							SELECT TOP (@RecordPerBatch) OpportunityStageGroupID
							FROM dbo.OpportunityStageGroups AS OG (NOLOCK)
							  INNER JOIN dbo.DropdownValues AS DV ON OG.DropdownValueID=DV.DropdownValueID     
							WHERE	DV.AccountID = @accountid 
						) tmp on tmp.OpportunityStageGroupID = OSG.OpportunityStageGroupID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
                SELECT @@ROWCOUNT OpportunityStageGroups_DropdownValuesCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityStageGroups_DropdownValues'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	DVV
				FROM	[dbo].[Opportunities] AS DVV INNER JOIN(
							SELECT TOP (@RecordPerBatch) OpportunityID
							FROM dbo.Opportunities AS DVVS (NOLOCK)
                            INNER JOIN dbo.DropdownValues AS DV ON dv.DropdownValueID=DVvs.StageID
							WHERE	dv.AccountID = @accountid 
						) tmp on tmp.OpportunityID = dvv.OpportunityID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
                SELECT @@ROWCOUNT Opportunities
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Opportunities'



SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	DVV
				FROM	[dbo].[WorkFlowLifeCycleAction]AS DVV INNER JOIN(
							SELECT TOP (@RecordPerBatch) [WorkFlowLifeCycleActionID]
							FROM dbo.WorkFlowLifeCycleAction AS DVVS (NOLOCK)
                            INNER JOIN dbo.DropdownValues AS DV ON dv.DropdownValueID=DVvs.LifecycleDropdownValueID     
							WHERE	dv.AccountID = @accountid 
						) tmp on tmp.WorkFlowLifeCycleActionID = dvv.WorkFlowLifeCycleActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
                SELECT @@ROWCOUNT WorkFlowLifeCycleAction
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkFlowLifeCycleAction'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	DVV
				FROM	[dbo].[WorkflowTriggers]AS DVV INNER JOIN(
							SELECT TOP (@RecordPerBatch) [WorkflowTriggerID]
							FROM dbo.WorkflowTriggers AS DVVS (NOLOCK)
                            INNER JOIN dbo.DropdownValues AS DV ON dv.DropdownValueID=DVvs.LifecycleDropdownValueID     
							WHERE	dv.AccountID = @accountid 
						) tmp on tmp.WorkflowTriggerID = dvv.WorkflowTriggerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
                SELECT @@ROWCOUNT WorkflowTriggers
			END

PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTriggers'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	DVV
				FROM	[dbo].[WorkflowTriggers]AS DVV INNER JOIN(
							SELECT TOP (@RecordPerBatch) [WorkflowTriggerID]
							FROM dbo.WorkflowTriggers AS DVVS (NOLOCK)
                            INNER JOIN dbo.DropdownValues AS DV ON dv.DropdownValueID=DVvs.OpportunityStageID     
							WHERE	dv.AccountID = @accountid 
						) tmp on tmp.WorkflowTriggerID = dvv.WorkflowTriggerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
                SELECT @@ROWCOUNT WorkflowTriggers
			END

PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTriggers'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	DVV
				FROM	[dbo].[Contacts] AS DVV INNER JOIN(
							SELECT TOP (@RecordPerBatch) [ContactID]
							FROM dbo.Contacts AS DVVS (NOLOCK)
                            INNER JOIN dbo.DropdownValues AS DV ON dv.DropdownValueID=DVvs.LifecycleStage     
							WHERE	dv.AccountID = @accountid 
						) tmp on tmp.ContactID = dvv.ContactID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
                SELECT @@ROWCOUNT Contacts
			END

PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Contacts'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	DVV
				FROM	[dbo].[Forms] AS DVV INNER JOIN(
							SELECT TOP (@RecordPerBatch) FormID
							FROM dbo.Forms AS DVVS (NOLOCK)
                            INNER JOIN dbo.DropdownValues AS DV ON dv.DropdownValueID=DVvs.LeadSourceID     
							WHERE	dv.AccountID = @accountid 
						) tmp on tmp.FormID = dvv.FormID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
                SELECT @@ROWCOUNT Forms
			END

PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Forms'
		
SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	DVV
				FROM	[dbo].[Tours] AS DVV INNER JOIN(
							SELECT TOP (@RecordPerBatch) TourID
							FROM dbo.Tours AS DVVS (NOLOCK)
                            INNER JOIN dbo.DropdownValues AS DV ON dv.DropdownValueID=DVvs.CommunityID     
							WHERE	dv.AccountID = @accountid 
						) tmp on tmp.TourID = dvv.TourID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
                SELECT @@ROWCOUNT Tours
			END

PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Tours'	
	
SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	DVV
				FROM	[dbo].[Tours] AS DVV INNER JOIN(
							SELECT TOP (@RecordPerBatch) TourID
							FROM dbo.Tours AS DVVS (NOLOCK)
                            INNER JOIN dbo.DropdownValues AS DV ON dv.DropdownValueID=DVvs.TourType    
							WHERE	dv.AccountID = @accountid 
						) tmp on tmp.TourID = dvv.TourID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
                SELECT @@ROWCOUNT Tours1
			END

PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Tours1'		
	
--	SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	DVV
--				FROM	[dbo].[Addresses] AS DVV INNER JOIN(
--							SELECT TOP (@RecordPerBatch)AddressID
--							FROM dbo.Addresses AS DVVS (NOLOCK)
--                            INNER JOIN dbo.DropdownValues AS DV ON dv.DropdownValueID=DVvs. AddressTypeID   
--							WHERE	dv.AccountID = @accountid 
--						) tmp on tmp.AddressID = dvv.AddressID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--                SELECT @@ROWCOUNT Addresses
--			END

--PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Addresses'

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	DVV
				FROM	[dbo].[LeadAdapterAndAccountMap] AS DVV INNER JOIN(
							SELECT TOP (@RecordPerBatch)LeadAdapterAndAccountMapID
							FROM dbo.LeadAdapterAndAccountMap AS DVVS (NOLOCK)
                            INNER JOIN dbo.DropdownValues AS DV ON dv.DropdownValueID=DVvs.LeadSourceType   
							WHERE	dv.AccountID = @accountid 
						) tmp on tmp.LeadAdapterAndAccountMapID = dvv.LeadAdapterAndAccountMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
                SELECT @@ROWCOUNT LeadAdapterAndAccountMap
			END

PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadAdapterAndAccountMap'

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	DVV
				FROM	[dbo].[ContactCommunityMap]AS DVV INNER JOIN(
							SELECT TOP (@RecordPerBatch)ContactCommunityMapID
							FROM dbo.ContactCommunityMap AS DVVS (NOLOCK)
                            INNER JOIN dbo.DropdownValues AS DV ON dv.DropdownValueID=DVvs.CommunityID   
							WHERE	dv.AccountID = @accountid 
						) tmp on tmp.ContactCommunityMapID = dvv.ContactCommunityMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
                SELECT @@ROWCOUNT ContactCommunityMap
			END

PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactCommunityMap'



			
BEGIN
				DELETE	DV
				FROM	dbo.DropdownValues  AS DV  (NOLOCK)
				WHERE	DV.AccountID = @Accountid
				 SELECT @@ROWCOUNT DropdownValuesCOUNT
			END
			PRINT ' records deleted from  DropdownValues'

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
	EXEC [dbo].[Deleting_DropdownValues_sp]
		@AccountID	= 94

*/



