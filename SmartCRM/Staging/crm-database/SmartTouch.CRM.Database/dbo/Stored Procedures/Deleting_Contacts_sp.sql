CREATE   PROCEDURE [dbo].[Deleting_Contacts_sp]
(
@AccountID INT = 0
)
AS 
BEGIN 
 SET NOCOUNT ON
  BEGIN TRY
    

DECLARE @TotalRecordsDeleted int = 1,
					 @RecordsDeleted int = 1,
					 @RecordPerBatch int = 5000


		SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	ICD
				FROM	dbo.ImportContactData AS ICD INNER JOIN(
							SELECT TOP (@RecordPerBatch) ImportContactDataID
							FROM dbo.ImportContactData AS IC (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON IC.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ImportContactDataID = ICD.ImportContactDataID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ImportContactData_Contacts '

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	ICl
				FROM	dbo.ImportContactLogs AS ICl INNER JOIN(
							SELECT TOP (@RecordPerBatch) ImportContactLogID
							FROM dbo.ImportContactLogs AS IC (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON IC.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ImportContactLogID = ICl.ImportContactLogID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ImportContactLogs_Contacts '

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WT
				FROM	[Workflow].[TrackMessages] AS WT INNER JOIN(
							SELECT TOP (@RecordPerBatch)TrackMessageID
							FROM [Workflow].[TrackMessages] AS WTM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON WTM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.TrackMessageID = WT.TrackMessageID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from [Workflow].[TrackMessages]_Contacts '

	--SET @RecordsDeleted = 1
	--		WHILE( @RecordsDeleted > 0 )
	--		BEGIN
	--			DELETE	T
	--			FROM [dbo].[TrackMessages]	AS T INNER JOIN(
	--						SELECT TOP (@RecordPerBatch)TrackMessageID
	--						FROM [dbo].[TrackMessages] AS TM (NOLOCK)
	--						  INNER JOIN dbo.Contacts AS C ON TM.ContactID=C.ContactID  
	--						WHERE	C.AccountID = @accountid 
	--					) tmp on tmp.TrackMessageID = T.TrackMessageID

	--			SET @RecordsDeleted = @@rowcount
	--			SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
	--			SELECT @TotalRecordsDeleted
	--		END
	--		PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from [TrackMessages]_Contacts '


		SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	ORM
				FROM	dbo.OpportunitiesRelationshipMap AS ORM INNER JOIN(
							SELECT TOP (@RecordPerBatch) OpportunityRelationshipMapID
							FROM dbo.OpportunitiesRelationshipMap AS ORMS (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON ORMS.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.OpportunityRelationshipMapID = ORM.OpportunityRelationshipMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunitiesRelationshipMap_Contacts '

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	ORMA
				FROM	dbo.OpportunitiesRelationshipMap_Audit AS ORMA INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.OpportunitiesRelationshipMap_Audit AS ORMS (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON ORMS.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.AuditId = ORMA.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunitiesRelationshipMap_Audit_Contacts '


	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	LSM
				FROM	[dbo].[LeadScoreMessages] AS LSM INNER JOIN(
							SELECT TOP (@RecordPerBatch)LeadScoreMessageID
							FROM [dbo].[LeadScoreMessages] AS LM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON LM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.LeadScoreMessageID = LSM.LeadScoreMessageID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from [LeadScoreMessages]_Contacts '

						SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CAM
				FROM	dbo.ContactActionMap AS CAM INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactActionMapID
							FROM dbo.ContactActionMap AS CM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON CM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ContactActionMapID = CAM.ContactActionMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactActionMap_Contacts'

				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CAM
				FROM	dbo.ContactActionMap_Audit AS CAM INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.ContactActionMap_Audit AS CM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON CM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.AuditId = CAM.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactActionMap_Audit_Contacts'

						SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CT
				FROM	dbo.CommunicationTracker AS CT INNER JOIN(
							SELECT TOP (@RecordPerBatch) CommunicationTrackerID
							FROM dbo.CommunicationTracker AS CTS (NOLOCK)
							  INNER JOIN dbo.Contacts AS C ON CTS.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CommunicationTrackerID = CT.CommunicationTrackerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CommunicationTracker_Contacts'

						SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CN
				FROM	dbo.ContactNoteMap AS CN INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactNoteMapID
							FROM dbo.ContactNoteMap AS CNM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK)  ON CNM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ContactNoteMapID = CN.ContactNoteMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactNoteMap_Contacts'

						SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CN
				FROM	dbo.ContactNoteMap_Audit AS CN INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.ContactNoteMap_Audit AS CNM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON CNM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.AuditId = CN.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactNoteMap_Audit_Contacts'

				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	OCM
				FROM	dbo.OpportunityContactMap AS OCM INNER JOIN(
							SELECT TOP (@RecordPerBatch) OpportunityContactMapID
							FROM dbo.OpportunityContactMap AS OM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON OM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.OpportunityContactMapID = OCM.OpportunityContactMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityContactMap '

 			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	OCM
				FROM	dbo.OpportunityContactMap_Audit AS OCM INNER JOIN(
							SELECT TOP (@RecordPerBatch)AuditId
							FROM dbo.OpportunityContactMap_Audit AS OM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON OM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.AuditId = OCM.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityContactMap_Audit'

				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CT
				FROM	dbo.ContactTourMap AS CT INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactTourMapID
							FROM dbo.ContactTourMap AS CTM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON CTM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ContactTourMapID = CT.ContactTourMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactTourMap '

						SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CT
				FROM	dbo.ContactTourMap_Audit AS CT INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.ContactTourMap_Audit AS CTM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON CTM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.AuditId = CT.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactTourMap_Audit '


				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	BD
				FROM	[dbo].[BulkContactData] AS BD INNER JOIN(
							SELECT TOP (@RecordPerBatch) BulkContactDataID
							FROM [dbo].[BulkContactData] AS BCD (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON BCD.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.BulkContactDataID = BD.BulkContactDataID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from [BulkContactData] ' 

				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CRM
				FROM	dbo.ContactRelationshipMap AS CRM INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactRelationshipMapID
							FROM dbo.ContactRelationshipMap AS CRM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C with (NOLOCK) ON CRM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ContactRelationshipMapID = CRM.ContactRelationshipMapID
SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactRelationshipMap '

			
				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CRMA
				FROM	dbo.ContactRelationshipMap_Audit AS CRMA INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.ContactRelationshipMap_Audit AS CRM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C with (NOLOCK) ON CRM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.AuditId = CRMA.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactRelationshipMap_Audit '

			
			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	F
				FROM	dbo.FormSubmissions AS F INNER JOIN(
							SELECT TOP (@RecordPerBatch) FormSubmissionID
							FROM dbo.FormSubmissions AS FS (NOLOCK)
							  INNER JOIN dbo.Contacts AS C  with (NOLOCK) ON FS.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.FormSubmissionID = F.FormSubmissionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from FormSubmissions '

					SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CE
				FROM	dbo.ContactEmails AS CE INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactEmailID
							FROM dbo.ContactEmails AS CES (NOLOCK)
							  INNER JOIN dbo.Contacts AS C  with (NOLOCK) ON CES.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ContactEmailID = CE.ContactEmailID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactEmails'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.ContactEmails_Audit AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.ContactEmails_Audit AS CES (NOLOCK)
							  INNER JOIN dbo.Contacts AS C with (NOLOCK) ON CES.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.AuditId = CEA.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactEmails_Audit'


			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	dbo.Documents AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) DocumentID
							FROM dbo.Documents AS DS (NOLOCK) 
							  INNER JOIN dbo.Contacts AS C with (NOLOCK) ON DS.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid
						) tmp on tmp.DocumentID = D.DocumentID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Documents '


	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	L
				FROM	dbo.LeadScores AS L INNER JOIN(
							SELECT TOP (@RecordPerBatch) LeadScoreID
							FROM dbo.LeadScores AS LS (NOLOCK)
							  INNER JOIN dbo.Contacts AS C with (NOLOCK) ON LS.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.LeadScoreID = L.LeadScoreID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadScores '

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CCM
				FROM	dbo.ContactCommunityMap AS CCM INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactCommunityMapID
							FROM dbo.ContactCommunityMap AS CCMP (NOLOCK)
							  INNER JOIN dbo.Contacts AS C with (NOLOCK)  ON CCMP.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ContactCommunityMapID = CCM.ContactCommunityMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactCommunityMap'


	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	MCR
				FROM	dbo.MomentaryCampaignRecipients AS MCR INNER JOIN(
							SELECT TOP (@RecordPerBatch) MomentaryRecipientID
							FROM dbo.MomentaryCampaignRecipients AS MC (NOLOCK)
							  INNER JOIN dbo.Contacts AS C  with (NOLOCK) ON MC.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.MomentaryRecipientID = MCR.MomentaryRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from MomentaryCampaignRecipients'

				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CIA
				FROM	dbo.ContactIPAddresses AS CIA INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactIPAddressID
							FROM dbo.ContactIPAddresses AS CA (NOLOCK)
							  INNER JOIN dbo.Contacts AS C with (NOLOCK) ON CA.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ContactIPAddressID = CIA.ContactIPAddressID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactIPAddresses'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CLM
				FROM	dbo.ContactLeadAdapterMap AS CLM INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactLeadAdapterMapID
							FROM dbo.ContactLeadAdapterMap AS CLAM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C with (NOLOCK) ON CLAM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ContactLeadAdapterMapID = CLM.ContactLeadAdapterMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactLeadAdapterMap'

--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	AC
--				FROM	dbo.ActiveContacts AS AC INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactLeadAdapterMapID
--							FROM dbo.ActiveContacts AS A (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON A.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactLeadAdapterMapID = CLM.ContactLeadAdapterMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ActiveContactsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ActiveContacts'

		SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CM
				FROM	dbo.ContactAddressMap AS CM INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactAddressMapID
							FROM dbo.ContactAddressMap AS CAM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON CAM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ContactAddressMapID = CM.ContactAddressMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactAddressMap'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CPN
				FROM	dbo.ContactPhoneNumbers AS CPN INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactPhoneNumberID
							FROM dbo.ContactPhoneNumbers AS CN (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK)  ON CN.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ContactPhoneNumberID = CPN.ContactPhoneNumberID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactPhoneNumbers '


	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CFM
				FROM	dbo.ContactCustomFieldMap AS CFM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactCustomFieldMapID
							FROM dbo.ContactCustomFieldMap AS CCFM (NOLOCK)
							  INNER JOIN [dbo].[Fields] AS F WITH (NOLOCK) ON CCFM.CustomFieldID= F.FieldID
							  INNER JOIN dbo.Contacts C ON C.ContactID = CCFM.ContactID
							WHERE	(F.AccountID = @accountid OR C.AccountID = @accountid)
						) tmp on tmp.ContactCustomFieldMapID = CFM.ContactCustomFieldMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactCustomFieldMap'

				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CV
				FROM	dbo.ContactWebVisits AS CV INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactWebVisitID
							FROM dbo.ContactWebVisits AS CWV (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON CWV.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ContactWebVisitID = CV.ContactWebVisitID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactWebVisits '

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CT
				FROM	dbo.ContactTagMap AS CT INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactTagMapID
							FROM dbo.ContactTagMap AS CTM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON CTM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ContactTagMapID = CT.ContactTagMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactTagMap '

					SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CLM
				FROM	dbo.ContactLeadSourceMap AS CLM INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactLeadSourceMapID
							FROM dbo.ContactLeadSourceMap AS CLSM (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON CLSM.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ContactLeadSourceMapID = CLM.ContactLeadSourceMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactLeadSourceMap '

				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	dbo.DocRepositorys AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) DocumentID
							FROM dbo.DocRepositorys AS DR (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON DR.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.DocumentID = D.DocumentID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from DocRepositorys '



			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CWfA
				FROM	dbo.ContactWorkflowAudit AS CWFA INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactWorkflowAuditID
							FROM dbo.ContactWorkflowAudit AS CWA (NOLOCK)
							  INNER JOIN dbo.Contacts AS C WITH (NOLOCK) ON CWA.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ContactWorkflowAuditID = CWFA.ContactWorkflowAuditID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactWorkflowAudit '

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CWfA
				FROM	[dbo].[ReceivedMailAudit] AS CWFA INNER JOIN(
							SELECT TOP (@RecordPerBatch) ReceivedMailAuditID
							FROM dbo.ReceivedMailAudit AS CWA (NOLOCK)
							  INNER JOIN dbo.Contacts AS C  WITH (NOLOCK)ON CWA.SentByContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ReceivedMailAuditID = CWFA.ReceivedMailAuditID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactWorkflowAudit '


			
			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CWfA
				FROM	[dbo].[WorkflowUserAssignmentAudit] AS CWFA INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowUserAssignmentAuditID
							FROM dbo.WorkflowUserAssignmentAudit AS CWA (NOLOCK)
							  INNER JOIN dbo.Contacts AS C  WITH (NOLOCK)ON CWA.ContactID=C.ContactID  
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.WorkflowUserAssignmentAuditID = CWFA.WorkflowUserAssignmentAuditID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowUserAssignmentAudit '


BEGIN
				DELETE	C
				FROM	dbo.Contacts  AS C  (NOLOCK)
				WHERE	C.AccountID = @Accountid 
	        SELECT @@ROWCOUNT   Contacts1COUNT
			END
			PRINT  ' records deleted from  Contacts'

			BEGIN
				DELETE	C
				FROM	dbo.Contacts_Audit  AS C  (NOLOCK)
				WHERE	C.AccountID = @Accountid 
	        SELECT @@ROWCOUNT  Contacts_AuditCOUNT
			END
			PRINT  ' records deleted from  Contacts_Audit'

SELECT @@ROWCOUNT TotalCount
--successfull execution query-- 
SELECT 'DEL-001' ResultCode 


	END TRY

BEGIN CATCH
	
		--Unsuccessful execution query-- 
		SELECT 'DEL-002' ResultCode 
		--Error blocking statement in between catch --
		INSERT INTO SmartCRM_Test.dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

END CATCH
	SET NOCOUNT OFF
END 


/*
	EXEC [dbo].[Deleting_Contacts_sp]
		@AccountID	= 19

*/

/*
	SELECT COUNT(*) FROM ContactTextMessageAudit WITH (NOLOCK) WHERE ContactPhoneNumberID IN (SELECT ContactPhoneNumberID FROM ContactPhoneNumbers WHERE Accountid = 19)
	SELECT COUNT(*) FROM ContactPhoneNumbers WITH (NOLOCK) where Accountid = 22
*/

