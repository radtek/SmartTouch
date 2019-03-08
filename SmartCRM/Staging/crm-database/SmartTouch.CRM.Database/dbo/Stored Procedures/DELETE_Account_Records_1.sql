


--/*
--		PURPOSE		: Deleting Account Records
--		INPUT		: ACCOUNTID
--		OUTPUT		: RETURN CODE
--		CREATED BY	: SANTOSH KUMAR K
--		CREATED ON	: NOV 21 2015
--		LAST MODIFIED ON	: 
--*/

--CREATE PROCEDURE [dbo].[DELETE_Account_Records_1]
--    (
--		@AccountID INT =	0	
--    )    
--AS 
--BEGIN

--	SET NOCOUNT ON
--	BEGIN TRY
--	  BEGIN TRANSACTION
--			---Here Deletion process doing on below tables hence it have tree relation with child table's --- 
--			 DECLARE @TotalRecordsDeleted int = 1,
--					 @RecordsDeleted int = 1,
--					 @RecordPerBatch int = 5000

--            SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN

--				DELETE	cam
--				FROM	DBO.ContactActionMap AS cam INNER JOIN(
--							SELECT	top (@RecordPerBatch) ContactActionMapID
--							FROM	DBO.ContactActionMap AS ca WITH (NOLOCK) INNER JOIN dbo.Actions AS A ON ca.ActionID=A.ActionID  
--							WHERE	A.AccountID = @accountid  
--						) tmp on tmp.ContactActionMapID = cam.ContactActionMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactActionMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactActionMap'




--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	atm
--				FROM	dbo.ActionTagMap AS atm INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ActionTagMapID
--							FROM dbo.ActionTagMap AS am (NOLOCK)
--							  INNER JOIN dbo.Actions AS A ON am.ActionID=A.ActionID 
--							WHERE	A.AccountID = @accountid 
--						) tmp on tmp.ActionTagMapID = atm.ActionTagMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ActionTagMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ActionTagMap'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	OAM
--				FROM	dbo.OpportunityActionMap  AS OAM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) OpportunityActionMapID
--							FROM dbo.OpportunityActionMap AS OAM (NOLOCK)
--							  INNER JOIN dbo.Actions AS AN ON OAM.ActionID=AN.ActionID
--							WHERE	AN.AccountID = @AccountID 
--						) tmp on tmp.OpportunityActionMapID = OAM.OpportunityActionMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT OpportunityActionMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityActionMap'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CCM
--				FROM	dbo.ContactCommunityMap  AS CCM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactCommunityMapID
--							FROM dbo.ContactCommunityMap AS CCM (NOLOCK)
--							  INNER JOIN dbo.Communities AS C ON CCM.CommunityID=C.CommunityID
--							WHERE	C.AccountID = @AccountID 
--						) tmp on tmp.ContactCommunityMapID = CCM.ContactCommunityMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactCommunityMapcount
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactCommunityMap '


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CTM
--				FROM	dbo.ContactTourMap AS  CTM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactTourMapID
--							FROM dbo.ContactTourMap AS CTM (NOLOCK)
--							  INNER JOIN  dbo.Tours AS T ON CTM.TourID = T.TourID
--							WHERE	T.AccountID = @AccountID
--						) tmp on tmp.ContactTourMapID = CTM.ContactTourMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactTourMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactTourMap '


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	T
--				FROM	dbo.Tours AS T INNER JOIN(
--							SELECT TOP (@RecordPerBatch) TourID
--							FROM dbo.Tours AS T (NOLOCK)
--							  INNER JOIN dbo.Communities AS C ON T.CommunityID=C.CommunityID 
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.TourID = T.TourID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ToursCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Tours'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CEA
--				FROM	dbo.ContactEmailAudit AS CEA INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactEmailAuditID
--							FROM dbo.ContactEmailAudit AS CEA (NOLOCK)
--							  INNER JOIN dbo.ContactEmails AS CE ON CEA.ContactEmailID=CE.ContactEmailID 
--							WHERE	CE.AccountID = @accountid 
--						) tmp on tmp.ContactEmailAuditID = CEA.ContactEmailAuditID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactEmailAuditCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactEmailAudit'

--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	OSG
--				FROM	dbo.OpportunityStageGroups AS OSG INNER JOIN(
--							SELECT TOP (@RecordPerBatch) OpportunityStageGroupID
--							FROM dbo.OpportunityStageGroups AS OSG (NOLOCK)
--							  INNER JOIN dbo.DropdownValues AS DV ON OSG.DropdownValueID=DV.DropdownValueID     
--							WHERE	DV.AccountID = @accountid 
--						) tmp on tmp.OpportunityStageGroupID = OSG.OpportunityStageGroupID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--                SELECT @@ROWCOUNT OpportunityStageGroupsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityStageGroups'

			
--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	sf
--				FROM	[dbo].[SearchFilters] AS sf  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) SearchFilterID
--							FROM [dbo].[SearchFilters] AS sf (NOLOCK)
--							  INNER JOIN [dbo].[DropdownValues] AS dv ON dv.[DropdownValueID]=sf.[DropdownValueID]
--							WHERE	dv.AccountID = @Accountid 
--						) tmp on tmp.SearchFilterID = sf.SearchFilterID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT SearchFiltersCOUNT
--			END
	
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  SearchFilter'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CTMA
--				FROM	dbo.ContactTextMessageAudit AS CTMA INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactTextMessageAuditID
--							FROM dbo.ContactTextMessageAudit AS CTMA (NOLOCK)
--							  INNER JOIN dbo.ContactPhoneNumbers AS C ON CTMA.ContactPhoneNumberID=C.ContactPhoneNumberID 
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactTextMessageAuditID = CTMA.ContactTextMessageAuditID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactTextMessageAuditCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactTextMessageAudit'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CAM
--				FROM	dbo.ContactActionMap AS CAM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactActionMapID
--							FROM dbo.ContactActionMap AS CAM (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CAM.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactActionMapID = CAM.ContactActionMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactActionMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactActionMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CNM
--				FROM	dbo.ContactNoteMap AS CNM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactNoteMapID
--							FROM dbo.ContactNoteMap AS CNM (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CNM.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactNoteMapID = CNM.ContactNoteMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactNoteMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactNoteMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CAM
--				FROM	dbo.ContactActionMap AS CAM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactActionMapID
--							FROM dbo.ContactActionMap AS CAM (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CAM.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactActionMapID = CAM.ContactActionMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactActionMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactActionMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CT
--				FROM	dbo.CommunicationTracker AS CT INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CommunicationTrackerID
--							FROM dbo.CommunicationTracker AS CT (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CT.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CommunicationTrackerID = CT.CommunicationTrackerID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CommunicationTrackerCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CommunicationTracker'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CAM
--				FROM	dbo.ContactAddressMap AS CAM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactAddressMapID
--							FROM dbo.ContactAddressMap AS CAM (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CAM.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactAddressMapID = CAM.ContactAddressMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactAddressMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactAddressMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CCM
--				FROM	dbo.ContactCommunityMap AS CCM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactCommunityMapID
--							FROM dbo.ContactCommunityMap AS CCM (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CCM.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactCommunityMapID = CCM.ContactCommunityMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactCommunityMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactCommunityMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CCFM
--				FROM	dbo.ContactCustomFieldMap AS CCFM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactCustomFieldMapID
--							FROM dbo.ContactCustomFieldMap AS CCFM (NOLOCK)
--							  INNER JOIN dbo.Fields AS C ON CCFM.CustomFieldID=C.FieldID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactCustomFieldMapID = CCFM.ContactCustomFieldMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactCustomFieldMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactCustomFieldMap'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CE
--				FROM	dbo.ContactEmails AS CE INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactEmailID
--							FROM dbo.ContactEmails AS CE (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CE.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactEmailID = CE.ContactEmailID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactEmailsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactEmails'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CA
--				FROM	dbo.ContactIPAddresses AS CA INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactIPAddressID
--							FROM dbo.ContactIPAddresses AS CA (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CA.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactIPAddressID = CA.ContactIPAddressID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactIPAddressesCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactIPAddresses'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CLAM
--				FROM	dbo.ContactLeadAdapterMap AS CLAM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactLeadAdapterMapID
--							FROM dbo.ContactLeadAdapterMap AS CLAM (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CLAM.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactLeadAdapterMapID = CLAM.ContactLeadAdapterMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactLeadAdapterMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactLeadAdapterMap'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CLSM
--				FROM	dbo.ContactLeadSourceMap AS CLSM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactLeadSourceMapID
--							FROM dbo.ContactLeadSourceMap AS CLSM (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CLSM.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactLeadSourceMapID = CLSM.ContactLeadSourceMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactLeadSourceMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactLeadSourceMap '

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CNM
--				FROM	dbo.ContactNoteMap AS CNM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactNoteMapID
--							FROM dbo.ContactNoteMap AS CNM (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CNM.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid
--						) tmp on tmp.ContactNoteMapID = CNM.ContactNoteMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactNoteMapCOUNT
--			END
--          PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactNoteMap '

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CPN
--				FROM	dbo.ContactPhoneNumbers AS CPN INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactPhoneNumberID
--							FROM dbo.ContactPhoneNumbers AS CPN (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CPN.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactPhoneNumberID = CPN.ContactPhoneNumberID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactPhoneNumbersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactPhoneNumbers '


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CRM
--				FROM	dbo.ContactRelationshipMap AS CRM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactRelationshipMapID
--							FROM dbo.ContactRelationshipMap AS CRM (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CRM.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactRelationshipMapID = CRM.ContactRelationshipMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactRelationshipMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactRelationshipMap '


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CTM
--				FROM	dbo.ContactTagMap AS CTM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactTagMapID
--							FROM dbo.ContactTagMap AS CTM (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CTM.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactTagMapID = CTM.ContactTagMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactTagMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactTagMap '


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CTM
--				FROM	dbo.ContactTourMap AS CTM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactTourMapID
--							FROM dbo.ContactTourMap AS CTM (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CTM.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactTourMapID = CTM.ContactTourMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactTourMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactTourMap '


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CWV
--				FROM	dbo.ContactWebVisits AS CWV INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactWebVisitID
--							FROM dbo.ContactWebVisits AS CWV (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CWV.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactWebVisitID = CWV.ContactWebVisitID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactWebVisitsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactWebVisits '


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CWFA
--				FROM	dbo.ContactWorkflowAudit AS CWFA INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactWorkflowAuditID
--							FROM dbo.ContactWorkflowAudit AS CWFA (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON CWFA.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ContactWorkflowAuditID = CWFA.ContactWorkflowAuditID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactWorkflowAuditCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactWorkflowAudit '


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	DR
--				FROM	dbo.DocRepositorys AS DR INNER JOIN(
--							SELECT TOP (@RecordPerBatch) DocumentID
--							FROM dbo.DocRepositorys AS DR (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON DR.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.DocumentID = DR.DocumentID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT DocRepositorysCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from DocRepositorys '

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	D
--				FROM	dbo.Documents AS D INNER JOIN(
--							SELECT TOP (@RecordPerBatch) DocumentID
--							FROM dbo.Documents AS D (NOLOCK) 
--							  INNER JOIN dbo.Contacts AS C ON D.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid
--						) tmp on tmp.DocumentID = D.DocumentID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT DocumentsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Documents '



--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	FS
--				FROM	dbo.FormSubmissions AS FS INNER JOIN(
--							SELECT TOP (@RecordPerBatch) FormSubmissionID
--							FROM dbo.FormSubmissions AS FS (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON FS.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.FormSubmissionID = FS.FormSubmissionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT FormSubmissionsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from FormSubmissions '

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	LS
--				FROM	dbo.LeadScores AS LS INNER JOIN(
--							SELECT TOP (@RecordPerBatch) LeadScoreID
--							FROM dbo.LeadScores AS LS (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON LS.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.LeadScoreID = LS.LeadScoreID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT LeadScoresCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadScores '


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	ORM
--				FROM	dbo.OpportunitiesRelationshipMap AS ORM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) OpportunityRelationshipMapID
--							FROM dbo.OpportunitiesRelationshipMap AS ORM (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON ORM.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.OpportunityRelationshipMapID = ORM.OpportunityRelationshipMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT OpportunitiesRelationshipMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunitiesRelationshipMap '

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	OCM
--				FROM	dbo.OpportunityContactMap AS OCM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) OpportunityContactMapID
--							FROM dbo.OpportunityContactMap AS OCM (NOLOCK)
--							  INNER JOIN dbo.Contacts AS C ON OCM.ContactID=C.ContactID  
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.OpportunityContactMapID = OCM.OpportunityContactMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT OpportunityContactMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityContactMap '

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	SDV
--				FROM	dbo.SubscriptionDefaultDropdownValueMap AS SDV INNER JOIN(
--							SELECT TOP (@RecordPerBatch) SubscriptionDefaultDropdownValueMapID
--							FROM dbo.SubscriptionDefaultDropdownValueMap AS SDV (NOLOCK)
--							  INNER JOIN dbo.DropdownValues AS DV ON SDV.DropdownValueID=DV.DropdownValueID     
--							WHERE	DV.AccountID = @accountid 
--						) tmp on tmp.SubscriptionDefaultDropdownValueMapID = SDV.SubscriptionDefaultDropdownValueMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT SubscriptionDefaultDropdownValueMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from SubscriptionDefaultDropdownValueMap'

			

			

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	FF
--				FROM	dbo.FormFields AS FF INNER JOIN(
--							SELECT TOP (@RecordPerBatch) FormFieldID
--							FROM dbo.FormFields AS FF (NOLOCK)
--							  INNER JOIN dbo.Fields AS F ON FF.FieldID=F.FieldID    
--							WHERE	F.AccountID = @accountid 
--						) tmp on tmp.FormFieldID = FF.FormFieldID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT FormFields_FieldsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from FormFields_Fields'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	SF
--				FROM	dbo.SearchFilters AS SF INNER JOIN(
--							SELECT TOP (@RecordPerBatch) SearchFilterID
--							FROM dbo.SearchFilters AS SF (NOLOCK)
--							  INNER JOIN dbo.Fields AS F ON SF.FieldID=F.FieldID    
--							WHERE	F.AccountID = @accountid 
--						) tmp on tmp.SearchFilterID = SF.SearchFilterID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT SearchFiltersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from SearchFilters'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	FF
--				FROM	dbo.FormFields AS FF INNER JOIN(
--							SELECT TOP (@RecordPerBatch) FormFieldID
--							FROM dbo.FormFields AS FF (NOLOCK)
--							  INNER JOIN dbo.Forms AS F ON FF.FormID=F.FormID   
--							WHERE	F.AccountID = @accountid 
--						) tmp on tmp.FormFieldID = FF.FormFieldID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT FormFields_FormsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from FormFields_Forms'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	FS
--				FROM	dbo.FormSubmissions AS FS INNER JOIN(
--							SELECT TOP (@RecordPerBatch) FormSubmissionID
--							FROM dbo.FormSubmissions AS FS (NOLOCK)
--							  INNER JOIN dbo.Forms AS F ON FS.FormID=F.FormID   
--							WHERE	F.AccountID = @accountid 
--						) tmp on tmp.FormSubmissionID = FS.FormSubmissionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT FormSubmissionsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from FormSubmissions'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	FT
--				FROM	dbo.FormTags AS FT INNER JOIN(
--							SELECT TOP (@RecordPerBatch) FormTagID
--							FROM dbo.FormTags AS FT (NOLOCK)
--							  INNER JOIN dbo.Forms AS F ON FT.FormID=F.FormID   
--							WHERE	F.AccountID = @accountid 
--						) tmp on tmp.FormTagID = FT.FormTagID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT FormTagsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from FormTags'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WT
--				FROM	dbo.WorkflowTriggers AS WT INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkflowTriggerID
--							FROM dbo.WorkflowTriggers AS WT (NOLOCK)
--							  INNER JOIN dbo.Forms AS F ON WT.FormID=F.FormID   
--							WHERE	F.AccountID = @accountid 
--						) tmp on tmp.WorkflowTriggerID = WT.WorkflowTriggerID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowTriggersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTriggers'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WCFA
--				FROM	dbo.WorkflowContactFieldAction AS WCFA INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkflowContactFieldActionID
--							FROM dbo.WorkflowContactFieldAction AS WCFA (NOLOCK)
--							  INNER JOIN dbo.Fields AS F ON WCFA.FieldID=F.FieldID  
--							WHERE	F.AccountID = @accountid
--						) tmp on tmp.WorkflowContactFieldActionID = WCFA.WorkflowContactFieldActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowContactFieldActionCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowContactFieldAction'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	LS
--				FROM	dbo.LeadScores AS LS INNER JOIN(
--							SELECT TOP (@RecordPerBatch) LeadScoreID
--							FROM dbo.LeadScores AS LS (NOLOCK)
--							  INNER JOIN dbo.LeadScoreRules AS LSR ON LS.LeadScoreRuleID=LSR.LeadScoreRuleID  
--							WHERE	LSR.AccountID = @accountid 
--						) tmp on tmp.LeadScoreID = LS.LeadScoreID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT LeadScoresCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadScores'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	D
--				FROM	dbo.Documents AS D INNER JOIN(
--							SELECT TOP (@RecordPerBatch) DocumentID
--							FROM dbo.Documents AS D (NOLOCK)
--							  INNER JOIN dbo.Opportunities AS O ON D.OpportunityID=O.OpportunityID 
--							WHERE	O.AccountID = @accountid 
--						) tmp on tmp.DocumentID = D.DocumentID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT DocumentsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Documents'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	ORM
--				FROM	dbo.OpportunitiesRelationshipMap AS ORM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) OpportunityRelationshipMapID
--							FROM dbo.OpportunitiesRelationshipMap AS ORM (NOLOCK)
--							  INNER JOIN dbo.Opportunities AS O ON ORM.OpportunityID=O.OpportunityID 
--							WHERE	O.AccountID = @accountid 
--						) tmp on tmp.OpportunityRelationshipMapID = ORM.OpportunityRelationshipMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT OpportunitiesRelationshipMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunitiesRelationshipMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	OAM
--				FROM	dbo.OpportunityActionMap AS OAM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) OpportunityActionMapID
--							FROM dbo.OpportunityActionMap AS OAM (NOLOCK) 
--							  INNER JOIN dbo.Opportunities AS O ON OAM.OpportunityID=O.OpportunityID 
--							WHERE	O.AccountID = @accountid
--						) tmp on tmp.OpportunityActionMapID = OAM.OpportunityActionMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT OpportunityActionMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityActionMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	OCM
--				FROM	dbo.OpportunityContactMap AS OCM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) OpportunityContactMapID
--							FROM dbo.OpportunityContactMap AS OCM (NOLOCK)
--							  INNER JOIN dbo.Opportunities AS O ON OCM.OpportunityID=O.OpportunityID 
--							WHERE	O.AccountID = @accountid 
--						) tmp on tmp.OpportunityContactMapID = OCM.OpportunityContactMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT OpportunityContactMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityContactMap'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	ONM
--				FROM	dbo.OpportunityNoteMap AS ONM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) OpportunityNoteMapID
--							FROM dbo.OpportunityNoteMap AS ONM (NOLOCK)
--							  INNER JOIN dbo.Opportunities AS O ON ONM.OpportunityID=O.OpportunityID 
--							WHERE	O.AccountID = @accountid  
--						) tmp on tmp.OpportunityNoteMapID = ONM.OpportunityNoteMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT OpportunityNoteMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityNoteMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	OTM
--				FROM	dbo.OpportunityTagMap AS OTM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) OpportunityTagMapID
--							FROM dbo.OpportunityTagMap AS OTM (NOLOCK)
--							  INNER JOIN dbo.Opportunities AS O ON OTM.OpportunityID=O.OpportunityID 
--							WHERE	O.AccountID = @accountid 
--						) tmp on tmp.OpportunityTagMapID = OTM.OpportunityTagMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT OpportunityTagMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityTagMap'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CSDM
--				FROM	dbo.CampaignSearchDefinitionMap AS CSDM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignSearchDefinitionMapID
--							FROM dbo.CampaignSearchDefinitionMap AS CSDM (NOLOCK)
--							  INNER JOIN dbo.SearchDefinitions AS SD ON CSDM.SearchDefinitionID=SD.SearchDefinitionID 
--							WHERE	SD.AccountID = @accountid 
--						) tmp on tmp.CampaignSearchDefinitionMapID = CSDM.CampaignSearchDefinitionMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignSearchDefinitionMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignSearchDefinitionMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	SDSM
--				FROM	dbo.SearchDefinitionSubscriptionMap AS SDSM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) SearchDefinitionSubscriptionMapID
--							FROM dbo.SearchDefinitionSubscriptionMap AS SDSM (NOLOCK)
--							  INNER JOIN dbo.SearchDefinitions AS SD ON SDSM.SearchDefinitionID=SD.SearchDefinitionID 
--							WHERE	SD.AccountID = @accountid 
--						) tmp on tmp.SearchDefinitionSubscriptionMapID = SDSM.SearchDefinitionSubscriptionMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT SearchDefinitionSubscriptionMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from SearchDefinitionSubscriptionMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	SDTM
--				FROM	dbo.SearchDefinitionTagMap AS SDTM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) SearchDefinitionTagMapID
--							FROM dbo.SearchDefinitionTagMap AS SDTM (NOLOCK)
--							  INNER JOIN dbo.SearchDefinitions AS SD ON SDTM.SearchDefinitionID=SD.SearchDefinitionID 
--							WHERE	SD.AccountID = @accountid 
--						) tmp on tmp.SearchDefinitionTagMapID = SDTM.SearchDefinitionTagMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT SearchDefinitionTagMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from SearchDefinitionTagMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	SF
--				FROM	dbo.SearchFilters AS SF INNER JOIN(
--							SELECT TOP (@RecordPerBatch) SearchFilterID
--							FROM dbo.SearchFilters AS SF (NOLOCK)
--							  INNER JOIN dbo.SearchDefinitions AS SD ON SF.SearchDefinitionID=SD.SearchDefinitionID 
--							WHERE	SD.AccountID = @accountid 
--						) tmp on tmp.SearchFilterID = SF.SearchFilterID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT SearchFiltersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from SearchFilters'


	

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WT
--				FROM	dbo.WorkflowTriggers AS WT INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkflowTriggerID
--							FROM dbo.WorkflowTriggers AS WT (NOLOCK)
--							  INNER JOIN dbo.SearchDefinitions AS SD ON WT.SearchDefinitionID=SD.SearchDefinitionID 
--							WHERE	SD.AccountID = @accountid 
--						) tmp on tmp.WorkflowTriggerID = WT.WorkflowTriggerID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowTriggersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTriggers'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CTM
--				FROM	dbo.ContactTourMap AS CTM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactTourMapID
--							FROM dbo.ContactTourMap AS CTM  (NOLOCK)
--							  INNER JOIN dbo.Tours AS T ON CTM.TourID=T.TourID
--							WHERE	T.AccountID = @accountid 
--						) tmp on tmp.ContactTourMapID = CTM.ContactTourMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactTourMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactTourMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	DUSM
--				FROM	dbo.DashboardUserSettingsMap AS DUSM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) UserSettingsMapID
--							FROM dbo.DashboardUserSettingsMap AS DUSM (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON DUSM.UserID=U.UserID 
--							WHERE	U.AccountID = @accountid 
--						) tmp on tmp.UserSettingsMapID = DUSM.UserSettingsMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT DashboardUserSettingsMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from DashboardUserSettingsMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	LA
--				FROM	dbo.LoginAudit AS LA INNER JOIN(
--							SELECT TOP (@RecordPerBatch) LoginAuditID
--							FROM dbo.LoginAudit AS LA (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON LA.UserID=U.UserID 
--							WHERE	U.AccountID = @accountid 
--						) tmp on tmp.LoginAuditID = LA.LoginAuditID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT LoginAuditCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LoginAudit'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	N
--				FROM	dbo.Notifications AS N INNER JOIN(
--							SELECT TOP (@RecordPerBatch) NotificationID
--							FROM dbo.Notifications AS N (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON N.UserID=U.UserID 
--							WHERE	U.AccountID = @accountid 
--						) tmp on tmp.NotificationID = N.NotificationID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT NotificationsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Notifications'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	UAM
--				FROM	dbo.UserAddressMap AS UAM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) UserAddressMapID
--							FROM dbo.UserAddressMap AS UAM (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON UAM.UserID=U.UserID 
--							WHERE	U.AccountID = @accountid 
--						) tmp on tmp.UserAddressMapID = UAM.UserAddressMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT UserAddressMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from UserAddressMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	UPA
--				FROM	dbo.UserProfileAudit AS UPA INNER JOIN(
--							SELECT TOP (@RecordPerBatch) UserProfileAuditID
--							FROM dbo.UserProfileAudit AS UPA (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON UPA.UserID=U.UserID 
--							WHERE	U.AccountID = @accountid 
--						) tmp on tmp.UserProfileAuditID = UPA.UserProfileAuditID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT UserProfileAuditCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from UserProfileAudit'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	US
--				FROM	dbo.UserSettings AS US INNER JOIN(
--							SELECT TOP (@RecordPerBatch) UserSettingID
--							FROM dbo.UserSettings AS US (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON US.UserID=U.UserID 
--							WHERE	U.AccountID = @accountid 
--						) tmp on tmp.UserSettingID = US.UserSettingID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT UserSettingsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from UserSettings'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	USMP
--				FROM	dbo.UserSocialMediaPosts AS USMP INNER JOIN(
--							SELECT TOP (@RecordPerBatch) UserSocialMediaPostID
--							FROM dbo.UserSocialMediaPosts AS USMP (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON USMP.UserID=U.UserID 
--							WHERE	U.AccountID = @accountid 
--						) tmp on tmp.UserSocialMediaPostID = USMP.UserSocialMediaPostID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT UserSocialMediaPostsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from UserSocialMediaPosts'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WDSEA
--				FROM	dbo.WebVisitDailySummaryEmailAudit AS WDSEA INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WebVisitDailySummaryEmailAuditID
--							FROM dbo.WebVisitDailySummaryEmailAudit AS WDSEA (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON WDSEA.UserID=U.UserID 
--							WHERE	U.AccountID = @accountid 
--						) tmp on tmp.WebVisitDailySummaryEmailAuditID = WDSEA.WebVisitDailySummaryEmailAuditID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WebVisitDailySummaryEmailAuditCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WebVisitDailySummaryEmailAudit'



--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WUNM
--				FROM	dbo.WebVisitUserNotificationMap AS WUNM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WebVisitUserNotificationMapID
--							FROM dbo.WebVisitUserNotificationMap AS WUNM (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON WUNM.UserID=U.UserID 
--							WHERE	U.AccountID = @accountid 
--						) tmp on tmp.WebVisitUserNotificationMapID = WUNM.WebVisitUserNotificationMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WebVisitUserNotificationMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WebVisitUserNotificationMap'



--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WUAA
--				FROM	dbo.WorkFlowUserAssignmentAction AS WUAA INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkFlowUserAssignmentActionID
--							FROM dbo.WorkFlowUserAssignmentAction AS WUAA (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON WUAA.UserID=U.UserID 
--							WHERE	U.AccountID = @accountid 
--						) tmp on tmp.WorkFlowUserAssignmentActionID = WUAA.WorkFlowUserAssignmentActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkFlowUserAssignmentActionCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkFlowUserAssignmentAction'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WUNM
--				FROM	dbo.WebVisitUserNotificationMap AS WUNM INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WebVisitUserNotificationMapID
--							FROM dbo.WebVisitUserNotificationMap AS WUNM (NOLOCK)
--							  INNER JOIN dbo.WebAnalyticsProviders AS WAP ON WUNM.WebAnalyticsProviderID=WAP.WebAnalyticsProviderID  
--							WHERE	WAP.AccountID = @accountid 
--						) tmp on tmp.WebVisitUserNotificationMapID = WUNM.WebVisitUserNotificationMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WebVisitUserNotificationMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WebVisitUserNotificationMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WDSEA
--				FROM	dbo.WebVisitDailySummaryEmailAudit AS WDSEA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WebVisitDailySummaryEmailAuditID
--							FROM dbo.WebVisitDailySummaryEmailAudit AS WDSEA (NOLOCK)
--							  INNER JOIN dbo.WebAnalyticsProviders AS WAP ON WDSEA.WebAnalyticsProviderID=WAP.WebAnalyticsProviderID  
--							WHERE	WAP.AccountID = @accountid 
--						) tmp on tmp.WebVisitDailySummaryEmailAuditID = WDSEA.WebVisitDailySummaryEmailAuditID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WebVisitDailySummaryEmailAuditCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WebVisitDailySummaryEmailAudit'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CWA
--				FROM	dbo.ContactWorkflowAudit AS CWA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactWorkflowAuditID
--							FROM dbo.ContactWorkflowAudit AS CWA (NOLOCK)
--							  INNER JOIN dbo.Workflows AS W ON CWA.WorkflowID=W.WorkflowID   
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.ContactWorkflowAuditID = CWA.ContactWorkflowAuditID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactWorkflowAuditCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactWorkflowAudit'



--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WT
--				FROM	dbo.WorkflowTriggers AS WT  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkflowTriggerID
--							FROM dbo.WorkflowTriggers AS WT (NOLOCK)
--							  INNER JOIN dbo.Workflows AS W ON WT.WorkflowID=W.WorkflowID   
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.WorkflowTriggerID = WT.WorkflowTriggerID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowTriggersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTriggers'
 

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CCTM
--				FROM	dbo.CampaignContactTagMap AS CCTM  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignContactTagMapID
--							FROM dbo.CampaignContactTagMap AS CCTM (NOLOCK)
--							  INNER JOIN dbo.Campaigns AS C ON CCTM.CampaignID=C.CampaignID 
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignContactTagMapID = CCTM.CampaignContactTagMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignContactTagMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignContactTagMap'

 
--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	O
--				FROM	dbo.[Opportunities] AS O  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) OpportunityID
--							FROM dbo.Opportunities AS O (NOLOCK)
--							  INNER JOIN [dbo].[Accounts] AS A ON O.AccountID = A.AccountID  
--							WHERE	A.AccountID = @accountid 
--						) tmp on tmp.OpportunityID = O.OpportunityID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT OpportunitiesCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Opportunities'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	LAJD
--				FROM	dbo.[LeadAdapterJobLogDetails] AS LAJD  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) LeadAdapterJobLogDetailID
--							FROM dbo.LeadAdapterJobLogDetails AS LAJD (NOLOCK)
--							  INNER JOIN [dbo].[LeadAdapterJobLogs] AS LAJL ON LAJD.LeadAdapterJobLogID = LAJL.LeadAdapterJobLogID   
--							  INNER JOIN [dbo].[LeadAdapterAndAccountMap] LAAM ON LAAM.LeadAdapterAndAccountMapID = LAJL.LeadAdapterAndAccountMapID
--						WHERE LAAM.AccountID = @AccountID
--						) tmp on tmp.LeadAdapterJobLogDetailID = LAJD.LeadAdapterJobLogDetailID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT LeadAdapterJobLogDetailsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadAdapterJobLogDetails'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	ITM
--				FROM	dbo.[ImportTagMap] AS ITM  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ImportTagMapID
--							FROM dbo.ImportTagMap AS ITM (NOLOCK)
--							  INNER JOIN [dbo].[LeadAdapterJobLogs] AS LAJL ON ITM.LeadAdapterJobLogID = LAJL.LeadAdapterJobLogID
--							  INNER JOIN [dbo].[LeadAdapterAndAccountMap] LAAM ON LAAM.LeadAdapterAndAccountMapID = LAJL.LeadAdapterAndAccountMapID
--						WHERE LAAM.AccountID = @AccountID
--						) tmp on tmp.ImportTagMapID = ITM.ImportTagMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ImportTagMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ImportTagMap'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	IDS
--				FROM	dbo.ImportDataSettings AS IDS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ImportDataSettingID
--							FROM dbo.ImportDataSettings AS IDS (NOLOCK)
--							  INNER JOIN [dbo].[LeadAdapterJobLogs] AS LAJL ON IDS.LeadAdaperJobID = LAJL.LeadAdapterJobLogID
--							  INNER JOIN [dbo].[LeadAdapterAndAccountMap] LAAM ON LAAM.LeadAdapterAndAccountMapID = LAJL.LeadAdapterAndAccountMapID
--						WHERE LAAM.AccountID = @AccountID
--						) tmp on tmp.ImportDataSettingID = IDS.ImportDataSettingID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ImportDataSettingsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ImportDataSettings'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	LAJL
--				FROM	dbo.LeadAdapterJobLogs AS LAJL  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) LeadAdapterJobLogID
--							FROM dbo.LeadAdapterJobLogs AS LAJL (NOLOCK)
--							  INNER JOIN dbo.LeadAdapterAndAccountMap AS LAAM ON LAJL.LeadAdapterAndAccountMapID=LAAM.LeadAdapterAndAccountMapID
--							WHERE	LAAM.AccountID = @accountid 
--						) tmp on tmp.LeadAdapterJobLogID = LAJL.LeadAdapterJobLogID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT LeadAdapterJobLogsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadAdapterJobLogs'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CR
--				FROM	dbo.CampaignRecipients_0001 AS CR  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0001 AS CR (NOLOCK)
--							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0001COUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0001'

--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CR
--				FROM	dbo.CampaignRecipients_0002 AS CR  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0002 AS CR (NOLOCK)
--							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0002COUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0002'


--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CR
--				FROM	dbo.CampaignRecipients_0003 AS CR  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0003 AS CR (NOLOCK)
--							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0003COUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0003'


--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CR
--				FROM	dbo.CampaignRecipients_0004 AS CR  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0004 AS CR (NOLOCK)
--							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0004COUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0004'


--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CR
--				FROM	dbo.CampaignRecipients_0005 AS CR  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0005 AS CR (NOLOCK)
--							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0005COUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0005'

--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CR
--				FROM	dbo.CampaignRecipients_0006 AS CR  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0006 AS CR (NOLOCK)
--							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0006COUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0006'


--	SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CS
--				FROM	dbo.CampaignStatistics_0001 AS CS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignTrackerID
--							FROM dbo.CampaignStatistics_0001 AS CS (NOLOCK)
--							  INNER JOIN [dbo].vCampaignRecipients AS CR ON CS.CampaignRecipientID = CR.CampaignRecipientID
--							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignTrackerID = CS.CampaignTrackerID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignStatistics_0001COUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0001'


--	SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CS
--				FROM	dbo.CampaignStatistics_0002 AS CS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignTrackerID
--							FROM dbo.CampaignStatistics_0002 AS CS (NOLOCK)
--							  INNER JOIN [dbo].vCampaignRecipients AS CR ON CS.CampaignRecipientID = CR.CampaignRecipientID
--							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignTrackerID = CS.CampaignTrackerID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignStatistics_0002COUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0002'

--	SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CS
--				FROM	dbo.CampaignStatistics_0003 AS CS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignTrackerID
--							FROM dbo.CampaignStatistics_0003 AS CS (NOLOCK)
--							  INNER JOIN [dbo].vCampaignRecipients AS CR ON CS.CampaignRecipientID = CR.CampaignRecipientID
--							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignTrackerID = CS.CampaignTrackerID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignStatistics_0003COUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0003'


--	SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CS
--				FROM	dbo.CampaignStatistics_0004 AS CS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignTrackerID
--							FROM dbo.CampaignStatistics_0004 AS CS (NOLOCK)
--							  INNER JOIN [dbo].vCampaignRecipients AS CR ON CS.CampaignRecipientID = CR.CampaignRecipientID
--							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignTrackerID = CS.CampaignTrackerID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignStatistics_0004COUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0004'

--	SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CS
--				FROM	dbo.CampaignStatistics_0005 AS CS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignTrackerID
--							FROM dbo.CampaignStatistics_0005 AS CS (NOLOCK)
--							  INNER JOIN [dbo].vCampaignRecipients AS CR ON CS.CampaignRecipientID = CR.CampaignRecipientID
--							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignTrackerID = CS.CampaignTrackerID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignStatistics_0005COUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0005'

--	SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CS
--				FROM	dbo.CampaignStatistics_0006 AS CS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignTrackerID
--							FROM dbo.CampaignStatistics_0006 AS CS (NOLOCK)
--							  INNER JOIN [dbo].vCampaignRecipients AS CR ON CS.CampaignRecipientID = CR.CampaignRecipientID
--							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignTrackerID = CS.CampaignTrackerID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignStatistics_0006COUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0006'



--		SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CR
--				FROM	dbo.CampaignRecipients_0001 AS CR  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0001 AS CR (NOLOCK)
--							 INNER JOIN dbo.ServiceProviders AS SP ON CR.ServiceProviderID=SP.ServiceProviderID 
--							WHERE	SP.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0001_ServiceProvidersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0001_ServiceProviders'




--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CR
--				FROM	dbo.CampaignRecipients_0002 AS CR  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0002 AS CR (NOLOCK)
--						 INNER JOIN dbo.ServiceProviders AS SP ON CR.ServiceProviderID=SP.ServiceProviderID 
--							WHERE	SP.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0002_ServiceProvidersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0002_ServiceProviders'


--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CR
--				FROM	dbo.CampaignRecipients_0003 AS CR  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0003 AS CR (NOLOCK)
--					 INNER JOIN dbo.ServiceProviders AS SP ON CR.ServiceProviderID=SP.ServiceProviderID 
--							WHERE	SP.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0003_ServiceProvidersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0003_ServiceProviders'


--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CR
--				FROM	dbo.CampaignRecipients_0004 AS CR  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0004 AS CR (NOLOCK)
--                           INNER JOIN dbo.ServiceProviders AS SP ON CR.ServiceProviderID=SP.ServiceProviderID 
--							WHERE	SP.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0004_ServiceProvidersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0004_ServiceProviders'


--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CR
--				FROM	dbo.CampaignRecipients_0005 AS CR  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0005 AS CR (NOLOCK)
--	                        INNER JOIN dbo.ServiceProviders AS SP ON CR.ServiceProviderID=SP.ServiceProviderID 
--							WHERE	SP.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0005_ServiceProvidersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0005_ServiceProviders'

--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CR
--				FROM	dbo.CampaignRecipients_0006 AS CR  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0006 AS CR (NOLOCK)
--				                  INNER JOIN dbo.ServiceProviders AS SP ON CR.ServiceProviderID=SP.ServiceProviderID 
--							WHERE	SP.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0006_ServiceProvidersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0006_ServiceProviders'


--		SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CRS
--				FROM	dbo.CampaignRecipients_0001 AS CRS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0001 AS CR (NOLOCK)
--					  INNER JOIN  dbo.Workflows AS W ON CR.WorkflowID=W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CRS.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0001_WorkflowsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0001_Workflows'




--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CRS
--				FROM	dbo.CampaignRecipients_0002 AS CRS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0002 AS CR (NOLOCK)
--			  INNER JOIN  dbo.Workflows AS W ON CR.WorkflowID=W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CRS.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0002_WorkflowsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0002_Workflows'


--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CRS
--				FROM	dbo.CampaignRecipients_0003 AS CRS INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0003 AS CR (NOLOCK)
--				 INNER JOIN  dbo.Workflows AS W ON CR.WorkflowID=W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CRS.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0003_WorkflowsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0003_Workflows'

--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CRS
--				FROM	dbo.CampaignRecipients_0004 AS CRS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0004 AS CR (NOLOCK)
--                 				 INNER JOIN  dbo.Workflows AS W ON CR.WorkflowID=W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CRS.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0004_WorkflowsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0004_Workflows'


--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CRS
--				FROM	dbo.CampaignRecipients_0005 AS CRS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0005 AS CR (NOLOCK)
--                 				 INNER JOIN  dbo.Workflows AS W ON CR.WorkflowID=W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CRS.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0005_WorkflowsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0005_Workflows'

--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CRS
--				FROM	dbo.CampaignRecipients_0006 AS CRS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignRecipientID
--							FROM dbo.CampaignRecipients_0006 AS CR (NOLOCK)
--            				 INNER JOIN  dbo.Workflows AS W ON CR.WorkflowID=W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.CampaignRecipientID = CRS.CampaignRecipientID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignRecipients_0006_WorkflowsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0006_Workflows'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WCAL
--				FROM	dbo.WorkflowCampaignActionLinks AS WCAL  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkflowCampaignLinkID
--							FROM dbo.WorkflowCampaignActionLinks AS WCAL (NOLOCK)
--							  INNER JOIN  [dbo].[WorkflowCampaignActions]  AS WCA ON WCAL.ParentWorkflowActionID = WCA.WorkflowCampaignActionID 
--							  INNER JOIN [dbo].[Campaigns] AS C ON WCA.CampaignID = C.CampaignID
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.WorkflowCampaignLinkID = WCAL.WorkflowCampaignLinkID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowCampaignActionLinksCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowCampaignActionLinks'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WCA
--				FROM	dbo.WorkflowCampaignActions AS WCA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkflowCampaignActionID
--							FROM dbo.WorkflowCampaignActions AS WCA (NOLOCK)
--							  INNER JOIN  [dbo].[Campaigns] AS C ON WCA.CampaignID = C.CampaignID 
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.WorkflowCampaignActionID = WCA.WorkflowCampaignActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowCampaignActionsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowCampaignActions'

--  	SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CS
--				FROM	dbo.CampaignStatistics AS CS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignTrackerID
--							FROM dbo.CampaignStatistics AS CS (NOLOCK)
--							  INNER JOIN  [dbo].CampaignLinkS AS CL ON CS.CampaignID = CL.CampaignID AND CS.CampaignLinkID = CL.CampaignLinkID
--							  INNER JOIN CampaignS C ON CS.CampaignID = C.CampaignID
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignTrackerID = CS.CampaignTrackerID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignLinks_CampaignStatisticsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignLinks_CampaignStatistics'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CLs
--				FROM	dbo.CampaignLinks AS CLs  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignLinkID
--							FROM dbo.CampaignLinks AS CL (NOLOCK)
--							  INNER JOIN  [dbo].[Campaigns] AS C ON CL.CampaignID = C.CampaignID 
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignLinkID = CLs.CampaignLinkID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignLinksCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignLinks'



--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CTM
--				FROM	dbo.CampaignTagMap AS CTM  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignTagMapID
--							FROM dbo.CampaignTagMap AS CTM (NOLOCK)
--							  INNER JOIN  [dbo].[Campaigns] AS C ON CTM.CampaignID = C.CampaignID 
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.CampaignTagMapID = CTM.CampaignTagMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignTagMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignTagMap'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	RC
--				FROM	dbo.ResentCampaigns AS RC  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ResentCampaignID
--							FROM dbo.ResentCampaigns AS RC (NOLOCK)
--							  INNER JOIN  [dbo].[Campaigns] AS C ON RC.CampaignID = C.CampaignID 
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ResentCampaignID = RC.ResentCampaignID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ResentCampaignsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ResentCampaigns'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	C
--				FROM	dbo.Campaigns AS C  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignID
--							FROM dbo.Campaigns AS C (NOLOCK)
--							  INNER JOIN dbo.ServiceProviders AS SP ON C.ServiceProviderID=SP.ServiceProviderID
--							WHERE	SP.AccountID = @accountid 
--						) tmp on tmp.CampaignID = C.CampaignID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Campaigns'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WTA
--				FROM	dbo.WorkflowTimerActions AS WTA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkflowTimerActionID
--							FROM dbo.WorkflowTimerActions AS WTA (NOLOCK)
--							  INNER JOIN [dbo].[WorkflowActions] AS WAS ON WTA.WorkflowActionID = WAS.WorkflowActionID
--							  INNER JOIN  [dbo].Workflows AS W ON WAS.WorkflowID = W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.WorkflowTimerActionID = WTA.WorkflowTimerActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowTimerActionsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTimerActions'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WFSA
--				FROM	dbo.WorkFlowLeadScoreAction AS WFSA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkFlowLeadScoreActionID
--							FROM dbo.WorkFlowLeadScoreAction AS WFSA (NOLOCK)
--							  INNER JOIN [dbo].[WorkflowActions] AS WAS ON WFSA.WorkflowActionID = WAS.WorkflowActionID
--							  INNER JOIN  [dbo].Workflows AS W ON WAS.WorkflowID = W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.WorkFlowLeadScoreActionID = WFSA.WorkFlowLeadScoreActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkFlowLeadScoreActionCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkFlowLeadScoreAction'



--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WTA
--				FROM	dbo.WorkflowTagAction AS WTA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkflowTagActionID
--							FROM dbo.WorkflowTagAction AS WTA (NOLOCK)
--							  INNER JOIN [dbo].[WorkflowActions] AS WA ON WTA.WorkflowActionID = WA.WorkflowActionID
--							  INNER JOIN  [dbo].Workflows AS W ON WA.WorkflowID = W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.WorkflowTagActionID = WTA.WorkflowTagActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowTagActionCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTagAction'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WLCA
--				FROM	dbo.WorkFlowLifeCycleAction AS WLCA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkFlowLifeCycleActionID
--							FROM dbo.WorkFlowLifeCycleAction AS WLCA (NOLOCK)
--							  INNER JOIN [dbo].[WorkflowActions] AS WA ON WLCA.WorkflowActionID = WA.WorkflowActionID
--							  INNER JOIN  [dbo].Workflows AS W ON WA.WorkflowID = W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.WorkFlowLifeCycleActionID = WLCA.WorkFlowLifeCycleActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkFlowLifeCycleActionCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkFlowLifeCycleAction'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WNUA
--				FROM	dbo.WorkflowNotifyUserAction AS WNUA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkflowNotifyUserActionID
--							FROM dbo.WorkflowNotifyUserAction AS WNUA (NOLOCK)
--							  INNER JOIN [dbo].[WorkflowActions] AS WA ON WNUA.WorkflowActionID = WA.WorkflowActionID
--							  INNER JOIN  [dbo].Workflows AS W ON WA.WorkflowID = W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.WorkflowNotifyUserActionID = WNUA.WorkflowNotifyUserActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowNotifyUserActionCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowNotifyUserAction'



--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WENA
--				FROM	dbo.WorkflowEmailNotificationAction AS WENA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkflowEmailNotificationActionID
--							FROM dbo.WorkflowEmailNotificationAction AS WENA (NOLOCK)
--							  INNER JOIN [dbo].[WorkflowActions] AS WA ON WENA.WorkflowActionID = WA.WorkflowActionID
--							  INNER JOIN  [dbo].Workflows AS W ON WA.WorkflowID = W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.WorkflowEmailNotificationActionID = WENA.WorkflowEmailNotificationActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowEmailNotificationActionCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowEmailNotificationAction'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	TWA
--				FROM	dbo.TriggerWorkflowAction AS TWA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) TriggerWorkflowActionID
--							FROM dbo.TriggerWorkflowAction AS TWA (NOLOCK)
--							  INNER JOIN [dbo].[WorkflowActions] AS WA ON TWA.WorkflowActionID = WA.WorkflowActionID
--							  INNER JOIN  [dbo].Workflows AS W ON WA.WorkflowID = W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.TriggerWorkflowActionID = TWA.TriggerWorkflowActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT TriggerWorkflowActionCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from TriggerWorkflowAction'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WCFA
--				FROM	dbo.WorkflowContactFieldAction AS WCFA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkflowContactFieldActionID
--							FROM dbo.WorkflowContactFieldAction AS WCFA (NOLOCK)
--							  INNER JOIN [dbo].[WorkflowActions] AS WA ON WCFA.WorkflowActionID = WA.WorkflowActionID
--							  INNER JOIN  [dbo].Workflows AS W ON WA.WorkflowID = W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.WorkflowContactFieldActionID = WCFA.WorkflowContactFieldActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowContactFieldActionCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowContactFieldAction'



--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WA
--				FROM	dbo.WorkflowActions AS WA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkflowActionID
--							FROM dbo.WorkflowActions AS WA (NOLOCK)
--							  INNER JOIN dbo.Workflows AS W ON WA.WorkflowID = W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.WorkflowActionID = WA.WorkflowActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowActionsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowActions'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WCA
--				FROM	dbo.WorkflowCampaignActions AS WCA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkflowCampaignActionID
--							FROM dbo.WorkflowCampaignActions AS WCA (NOLOCK)
--							  INNER JOIN [dbo].[Campaigns] AS C ON WCA.CampaignID = C.CampaignID
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.WorkflowCampaignActionID = WCA.WorkflowCampaignActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowCampaignActionsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowCampaignActions'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WTA
--				FROM	dbo.WorkflowTagAction AS WTA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkflowTagActionID
--							FROM dbo.WorkflowTagAction AS WTA (NOLOCK)
--							  INNER JOIN [dbo].[WorkflowActions] AS WfA ON WTA.WorkflowActionID=WfA.WorkflowActionID 
--							INNER JOIN dbo.Workflows AS W ON WfA.WorkflowID = W.WorkflowID 
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.WorkflowTagActionID = WTA.WorkflowTagActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowTagActionCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTagAction'

--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WTAl
--				FROM	[Workflow].[TrackActionLogs] AS WTAl  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) TrackActionLogID
--							FROM [Workflow].[TrackActionLogs] AS WWT  (NOLOCK)
--							  INNER JOIN  Workflow.TrackActions AS WWTA ON WWT.TrackActionID = WWTA.TrackActionID  
--							  INNER JOIN [dbo].[Workflows]  W ON  W.WorkflowID = WWTA.WorkflowID
--							WHERE	W.AccountID = @accountid 
--						) tmp on tmp.TrackActionLogID = WTAl.TrackActionLogID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowTrackActions_WorkflowTrackActionLogsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from [Workflow].[TrackActions]_[Workflow].[TrackActionLogs]'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WWTA
--				FROM	Workflow.TrackActions AS WWTA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) TrackActionID
--							FROM Workflow.TrackActions AS WWTA  (NOLOCK)
--							  INNER JOIN [Workflow].[TrackMessages]  AS WWTM ON WWTA.TrackMessageID = WWTM.TrackMessageID  
--							WHERE	WWTM.AccountID = @accountid 
--						) tmp on tmp.TrackActionID = WWTA.TrackActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowTrackActionsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from [Workflow].[TrackActions]_[Workflow].[TrackMessages]'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WWTM
--				FROM	Workflow.TrackActions AS WWTM  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) TrackActionID
--							FROM Workflow.TrackActions AS WWTM (NOLOCK)
--							  INNER JOIN [dbo].Actions AS C ON WWTM.ActionID = C.ActionID 
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.TrackActionID = WWTM.TrackActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowTrackActions_ActionsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Workflow.TrackActions_Actions'



--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	RMA
--				FROM	dbo.ReceivedMailAudit AS RMA  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ReceivedMailAuditID
--							FROM dbo.ReceivedMailAudit AS RMA (NOLOCK)
--							  INNER JOIN [dbo].[Contacts] AS C ON RMA.SentByContactID = C.ContactID 
--							WHERE	C.AccountID = @accountid 
--						) tmp on tmp.ReceivedMailAuditID = RMA.ReceivedMailAuditID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ReceivedMailAuditCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ReceivedMailAudit'



--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	FF
--				FROM	dbo.FormFields AS FF  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) FormFieldID
--							FROM dbo.FormFields AS FF (NOLOCK)
--							  INNER JOIN [dbo].[Fields] AS F ON FF.[FieldID]= F.[FieldID] 
--							WHERE	F.AccountID = @accountid 
--						) tmp on tmp.FormFieldID = FF.FormFieldID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT FormFields_FieldsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from FormFields_Fields'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CFVO
--				FROM	dbo.CustomFieldValueOptions AS CFVO  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CustomFieldValueOptionID
--							FROM dbo.CustomFieldValueOptions AS CFVO (NOLOCK)
--							  INNER JOIN [dbo].[Fields] AS F ON CFVO.CustomFieldID= F.[FieldID] 
--							WHERE	F.AccountID = @accountid 
--						) tmp on tmp.CustomFieldValueOptionID = CFVO.CustomFieldValueOptionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CustomFieldValueOptionsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CustomFieldValueOptions'



--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CCFM
--				FROM	dbo.ContactCustomFieldMap AS CCFM  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactCustomFieldMapID
--							FROM dbo.ContactCustomFieldMap AS CCFM (NOLOCK)
--							  INNER JOIN [dbo].[Fields] AS F ON CCFM.CustomFieldID= F.FieldID
--							  INNER JOIN dbo.Contacts C ON C.ContactID = CCFM.ContactID
--							WHERE	(F.AccountID = @accountid OR C.AccountID = @accountid)
--						) tmp on tmp.ContactCustomFieldMapID = CCFM.ContactCustomFieldMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactCustomFieldMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactCustomFieldMap'



--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	F
--				FROM	dbo.Fields AS F  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) FieldID
--							FROM dbo.Fields AS FS (NOLOCK)
--							  INNER JOIN [dbo].CustomFieldSections AS CFS  ON FS.CustomFieldSectionID =CFS.CustomFieldSectionID
--							WHERE FS.AccountID = @AccountID
--						) tmp on tmp.FieldID = F.FieldID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT FieldsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Fields'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CFS
--				FROM	dbo.CustomFieldSections AS CFS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CFSS.CustomFieldSectionID
--							FROM dbo.CustomFieldSections AS CFSS (NOLOCK)
--							  INNER JOIN [dbo].CustomFieldTabs AS CFT ON CFSS.TabID= CFT.CustomFieldTabID
--							WHERE	CFT.AccountID = @accountid 
--						) tmp on tmp.CustomFieldSectionID = CFS.CustomFieldSectionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CustomFieldSectionsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CustomFieldSections'



--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CAM
--				FROM	dbo.ContactAddressMap AS CAM  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactAddressMapID
--							FROM dbo.ContactAddressMap AS CAMP (NOLOCK)
--							  INNER JOIN [dbo].[Addresses]	AS A  ON CAMP.AddressID=A.AddressID 
--							  INNER JOIN dbo.Contacts C ON C.ContactID = CAMP.ContactID
--							WHERE C.AccountID = @AccountID

--						) tmp on tmp.ContactAddressMapID = CAM.ContactAddressMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactAddressMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactAddressMap'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	AAM
--				FROM	dbo.AccountAddressMap AS AAM  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) AccountAddressMapID
--							FROM dbo.AccountAddressMap AS AAAM (NOLOCK)
--							  INNER JOIN [dbo].[Addresses]	AS A  ON AAAM.AddressID=A.AddressID 
--							  INNER JOIN dbo.Accounts C ON C.AccountID = AAAM.AccountID
--							WHERE C.AccountID = @AccountID
--						) tmp on tmp.AccountAddressMapID = AAM.AccountAddressMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT AccountAddressMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from AccountAddressMap'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	UAM
--				FROM	dbo.UserAddressMap AS UAM  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) UserAddressMapID
--							FROM dbo.UserAddressMap AS UAMP (NOLOCK)
--							  INNER JOIN [dbo].[Addresses]	AS A  ON UAMP.AddressID=A.AddressID 
--							  INNER JOIN dbo.Users C ON C.UserID = UAMP.UserID
--							WHERE C.AccountID = @AccountID
--						) tmp on tmp.UserAddressMapID = UAM.UserAddressMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT UserAddressMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from UserAddressMap'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	A
--				FROM	dbo.Addresses AS A  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) AddressID
--							FROM dbo.Addresses AS ADS (NOLOCK)
--							  INNER JOIN dbo.DropdownValues AS DV ON ADS.AddressTypeID=DV.DropdownValueID
--							WHERE	DV.AccountID = @Accountid 
--						) tmp on tmp.AddressID = A.AddressID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT AddressesCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Addresses'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	T
--				FROM	dbo.Tours AS T  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) TourID
--							FROM dbo.Tours AS TS (NOLOCK)
--							  INNER JOIN dbo.DropdownValues AS DV ON TS.TourType=DV.DropdownValueID
--							WHERE	DV.AccountID = @Accountid
--						) tmp on tmp.TourID = T.TourID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ToursCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Tours'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	F
--				FROM	dbo.Forms AS F  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) FormID
--							FROM dbo.Forms AS FS (NOLOCK)
--							  INNER JOIN dbo.DropdownValues AS DV ON FS.LeadSourceID=DV.DropdownValueID
--							WHERE	DV.AccountID = @Accountid 
--						) tmp on tmp.FormID = F.FormID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT FormsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Forms'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CLM
--				FROM	dbo.ContactLeadSourceMap AS CLM  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactLeadSourceMapID
--							FROM dbo.ContactLeadSourceMap AS CLSM (NOLOCK)
--							  INNER JOIN dbo.DropdownValues AS DV ON CLSM.LeadSouceID=DV.DropdownValueID
--							WHERE	DV.AccountID = @Accountid 
--						) tmp on tmp.ContactLeadSourceMapID = CLM.ContactLeadSourceMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactLeadSourceMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactLeadSourceMap'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	C
--				FROM	dbo.Contacts AS C  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactID
--							FROM dbo.Contacts AS CS (NOLOCK)
--							  INNER JOIN dbo.DropdownValues AS DV ON CS.LifecycleStage=DV.DropdownValueID
--							WHERE	DV.AccountID = @Accountid 
--						) tmp on tmp.ContactID = C.ContactID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ContactsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Contacts'



--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	LATM
--				FROM	dbo.LeadAdapterTagMap AS LATM  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) LeadAdapterTagMapID
--							FROM dbo.LeadAdapterTagMap AS LATM (NOLOCK)
--							  INNER JOIN [dbo].[LeadAdapterAndAccountMap] AS LAAM ON LATM.LeadAdapterID = LAAM.LeadAdapterAndAccountMapID
--							WHERE	LAAM.AccountID = @Accountid 
--						) tmp on tmp.LeadAdapterTagMapID = LATM.LeadAdapterTagMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT LeadAdapterTagMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadAdapterTagMap'


--			--SET @RecordsDeleted = 1
--			--WHILE( @RecordsDeleted > 0 )
--			--BEGIN
--			--	DELETE	CRT
--			--	FROM	dbo.ClientRefreshTokens AS CRT  INNER JOIN(
--			--				SELECT TOP (@RecordPerBatch)  CRT.ID
--			--				FROM dbo.ClientRefreshTokens AS CRT (NOLOCK)
--			--				  INNER JOIN [dbo].[ThirdPartyClients] AS TPC ON CRT.ThirdPartyClientID = TPC.ID
--			--				WHERE	TPC.AccountID = @Accountid 
--			--			) tmp on tmp.ID = CRT.ID

--			--	SET @RecordsDeleted = @@rowcount
--			--	SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--			--	SELECT @@ROWCOUNT ClientRefreshTokensCOUNT
--			--END
--			--PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ClientRefreshTokens'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CRT
--				FROM	dbo.Campaigns AS CRT  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CampaignID
--							FROM dbo.Campaigns AS CRT (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON CRT.CreatedBy=U.UserID 
--							WHERE	U.AccountID = @Accountid 
--						) tmp on tmp.CampaignID = CRT.CampaignID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT Campaigns_UsersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Campaigns'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	LAAM
--				FROM	dbo.LeadAdapterAndAccountMap AS LAAM  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) LeadAdapterAndAccountMapID
--							FROM dbo.LeadAdapterAndAccountMap AS LAAM (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON LAAM.CreatedBy=U.UserID 
--							WHERE	U.AccountID = @Accountid 
--						) tmp on tmp.LeadAdapterAndAccountMapID = LAAM.LeadAdapterAndAccountMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT LeadAdapterAndAccountMapCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadAdapterAndAccountMap'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	ID
--				FROM	dbo.ImageDomains AS ID  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ImageDomainID
--							FROM dbo.ImageDomains AS ID (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON ID.CreatedBy=U.UserID 
--							WHERE	U.AccountID = @Accountid 
--						) tmp on tmp.ImageDomainID = ID.ImageDomainID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ImageDomainsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ImageDomains'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CRMOLS
--				FROM	dbo.CRMOutlookSync AS CRMOLS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) OutlookSyncID
--							FROM dbo.CRMOutlookSync AS CRMOLS (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON CRMOLS.LastSyncedBy=U.UserID 
--							WHERE	U.AccountID = @Accountid 
--						) tmp on tmp.OutlookSyncID = CRMOLS.OutlookSyncID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CRMOutlookSyncCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CRMOutlookSync'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	LSR
--				FROM	dbo.LeadScoreRules AS LSR  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) LeadScoreRuleID
--							FROM dbo.LeadScoreRules AS LSR (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON LSR.ModifiedBy=U.UserID 
--							WHERE	U.AccountID = @Accountid 
--						) tmp on tmp.LeadScoreRuleID = LSR.LeadScoreRuleID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT LeadScoreRulesCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadScoreRules'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WF
--				FROM	dbo.Workflows AS WF  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WorkflowID
--							FROM dbo.Workflows AS WF (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON WF.CreatedBy=U.UserID 
--							WHERE	U.AccountID = @Accountid 
--						) tmp on tmp.WorkflowID = WF.WorkflowID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Workflows'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	TPC
--				FROM	dbo.ThirdPartyClients AS TPC  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ID
--							FROM dbo.ThirdPartyClients AS TPC (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON TPC.LastUpdatedBy=U.UserID 
--							WHERE	U.AccountID = @Accountid   
--						) tmp on tmp.ID = TPC.ID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ThirdPartyClientsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ThirdPartyClients'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	AE
--				FROM	dbo.AccountEmails AS AE  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) EmailID
--							FROM dbo.AccountEmails AS AE (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON AE.UserID=U.UserID 
--							WHERE	U.AccountID = @Accountid 
--						) tmp on tmp.EmailID = AE.EmailID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT AccountEmailsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from AccountEmails'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	A
--				FROM	dbo.Actions AS A  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ActionID
--							FROM dbo.Actions AS A (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON A.CreatedBy=U.UserID 
--							WHERE	U.AccountID = @Accountid 
--						) tmp on tmp.ActionID = A.ActionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ActionsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Actions'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	C
--				FROM	dbo.Contacts AS C  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactID
--							FROM dbo.Contacts AS C (NOLOCK)
--							  INNER JOIN dbo.Users AS U ON C.OwnerID=U.UserID 
--							WHERE	U.AccountID = @Accountid 
--						) tmp on tmp.ContactID = C.ContactID

--				SET @RecordsDeleted = @@rowcount
--               SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--			   SELECT @@ROWCOUNT ContactsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Contacts'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	F
--				FROM	dbo.Forms AS F  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) FormID
--							FROM dbo.Forms AS F  (NOLOCK)
--							  INNER JOIN dbo.Users AS u ON F.CreatedBy=U.UserID
--							WHERE	U.AccountID = @Accountid 
--						) tmp on tmp.FormID = F.FormID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT FormsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Forms'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	IDS
--				FROM	dbo.ImportDataSettings AS IDS  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ImportDataSettingID
--							FROM dbo.ImportDataSettings AS IDS (NOLOCK)
--							  INNER JOIN dbo.Users AS u ON IDS.ProcessBy=U.UserID
--							WHERE	U.AccountID = @Accountid 
--						) tmp on tmp.ImportDataSettingID = IDS.ImportDataSettingID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ImportDataSettingsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ImportDataSettings'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WTM
--				FROM	[Workflow].[TrackMessages]  AS WTM  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) [TrackMessageID]
--							FROM [Workflow].[TrackMessages]  AS WTM (NOLOCK)
--							  INNER JOIN dbo.Users AS u ON WTM.UserID=U.UserID
--							WHERE	U.AccountID = @Accountid 
--						) tmp on tmp.TrackMessageID = WTM.TrackMessageID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowTrackMessages_UsersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from [Workflow].[TrackMessages] '


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WAP
--				FROM	dbo.WebAnalyticsProviders  AS WAP  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) WebAnalyticsProviderID
--							FROM dbo.WebAnalyticsProviders  AS WAP (NOLOCK)
--							  INNER JOIN [dbo].[Accounts] AS A ON WAP.AccountID=A.AccountID
--							WHERE	A.AccountID = @Accountid 
--						) tmp on tmp.WebAnalyticsProviderID = WAP.WebAnalyticsProviderID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WebAnalyticsProvidersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WebAnalyticsProviders'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CFT
--				FROM	dbo.CustomFieldTabs  AS CFT  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) CustomFieldTabID
--							FROM dbo.CustomFieldTabs  AS CFT (NOLOCK)
--							  INNER JOIN [dbo].[Accounts] AS A ON CFT.AccountID=A.AccountID
--							WHERE	A.AccountID = @Accountid 
--						) tmp on tmp.CustomFieldTabID = CFT.CustomFieldTabID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CustomFieldTabsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CustomFieldTabs'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	AE
--				FROM	dbo.AccountEmails  AS AE  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) EmailID
--							FROM dbo.AccountEmails  AS AE (NOLOCK)
--							  INNER JOIN [dbo].[Accounts] AS A ON AE.AccountID=A.AccountID
--							WHERE	A.AccountID = @Accountid 
--						) tmp on tmp.EmailID = AE.EmailID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT AccountEmails_AccountsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from AccountEmails'



--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	U
--				FROM	dbo.Users  AS U  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) UserID
--							FROM dbo.Users  AS U (NOLOCK)
--							  INNER JOIN [dbo].[Accounts] AS A ON U.AccountID=A.AccountID
--							WHERE	A.AccountID = @Accountid 
--						) tmp on tmp.UserID = U.UserID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT UsersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Users'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	SP
--				FROM	dbo.ServiceProviders  AS SP  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ServiceProviderID
--							FROM dbo.ServiceProviders  AS SP (NOLOCK)
--							  INNER JOIN [dbo].[Accounts] AS A ON SP.AccountID=A.AccountID
--							WHERE	A.AccountID = @Accountid 
--						) tmp on tmp.ServiceProviderID = SP.ServiceProviderID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT ServiceProvidersUsersCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ServiceProviders'
	
--	SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	C
--				FROM	dbo.Campaigns AS C  INNER JOIN(
--							SELECT TOP (@RecordPerBatch)cte.CampaignTemplateID
--							FROM dbo.CampaignTemplates AS Cte (NOLOCK)
--							  INNER JOIN dbo.Campaigns AS c ON C.CampaignTemplateID=cte.CampaignTemplateID
--							WHERE	c.AccountID = @accountid 
--						) tmp on tmp.CampaignTemplateID = C.CampaignTemplateID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignTemplates_CampaignsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignTemplates'

	
--	SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	CT
--				FROM	dbo.CampaignTemplates AS CT  INNER JOIN(
--							SELECT TOP (@RecordPerBatch)IM.ImageID
--							FROM dbo.Images AS IM (NOLOCK)
--							  INNER JOIN dbo.CampaignTemplates AS cT ON CT.[ThumbnailImage]=IM.ImageID
--							WHERE	IM.AccountID = @accountid 
--						) tmp on tmp.ImageID = CT.[ThumbnailImage]

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT CampaignTemplates_ImagesCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ImageS'  

--SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	OSG
--				FROM	dbo.OpportunityStageGroups AS OSG 
--				INNER JOIN(
--							SELECT TOP (@RecordPerBatch) OpportunityStageGroupID
--							FROM dbo.OpportunityStageGroups AS OSG (NOLOCK)
--							  INNER JOIN dbo.DropdownValues AS DV ON OSG.DropdownValueID=DV.DropdownValueID     
--							WHERE	DV.AccountID = @accountid 
--						) tmp on tmp.OpportunityStageGroupID = OSG.OpportunityStageGroupID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT OpportunityStageGroupsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityStageGroups'
--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	DV
--				FROM	dbo.DropdownValues  AS DV  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) DropdownValueID
--							FROM dbo.DropdownValues  AS DV (NOLOCK)
--							  INNER JOIN [dbo].[Accounts] AS A ON DV.AccountID=A.AccountID
--							WHERE	A.AccountID = @Accountid 
--						) tmp on tmp.DropdownValueID = DV.DropdownValueID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT DropdownValuesCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from DropdownValues'

--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	C
--				FROM	dbo.Contacts  AS C  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) ContactID
--							FROM dbo.Contacts  AS C (NOLOCK)
--							  INNER JOIN [dbo].[Accounts] AS A ON C.AccountID=A.AccountID
--							WHERE	A.AccountID = @Accountid 
--						) tmp on tmp.ContactID = C.ContactID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT Contacts_AccountsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Contacts'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	F
--				FROM	dbo.Fields  AS F  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) FieldID
--							FROM dbo.Fields  AS F (NOLOCK)
--							  INNER JOIN [dbo].[Accounts] AS A ON  F.AccountID=A.AccountID
--							WHERE	A.AccountID = @Accountid
--						) tmp on tmp.FieldID = F.FieldID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT Fields_AccountsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Fields'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	WTM
--				FROM	Workflow.[TrackMessages]  AS WTM  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) TrackMessageID
--							FROM Workflow.[TrackMessages]  AS WTM (NOLOCK)
--							  INNER JOIN [dbo].[Accounts] AS A ON WTM.AccountID=A.AccountID
--							WHERE	A.AccountID = @Accountid 
--						) tmp on tmp.TrackMessageID = WTM.TrackMessageID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT WorkflowTrackMessages_AccountsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  [Workflow].[TrackMessages]'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	ADAP
--				FROM	dbo.AccountDataAccessPermissions  AS ADAP  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) AccountDataAccessPermissionID
--							FROM dbo.AccountDataAccessPermissions  AS ADAP (NOLOCK)
--							  INNER JOIN [dbo].[Accounts] AS A ON ADAP.AccountID=A.AccountID
--							WHERE	A.AccountID = @Accountid 
--						) tmp on tmp.AccountDataAccessPermissionID = ADAP.AccountDataAccessPermissionID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT AccountDataAccessPermissions_AccountsCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  AccountDataAccessPermissions'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	NTM
--				FROM	dbo.NoteTagMap  AS NTM  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) NoteTagMapID
--							FROM dbo.NoteTagMap  AS NTM (NOLOCK)
--							  INNER JOIN [dbo].[Notes] AS N ON NTM.NoteID=N.NoteID
--							WHERE	N.AccountID = @Accountid 
--						) tmp on tmp.NoteTagMapID = NTM.NoteTagMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT NoteTagMap_NotesCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  NoteTagMap'


--			SET @RecordsDeleted = 1
--			WHILE( @RecordsDeleted > 0 )
--			BEGIN
--				DELETE	RM
--				FROM	dbo.RoleModuleMap  AS RM  INNER JOIN(
--							SELECT TOP (@RecordPerBatch) RoleModuleMapID
--							FROM dbo.RoleModuleMap  AS RM (NOLOCK)
--							  INNER JOIN [dbo].[Roles] AS R ON RM.RoleID=R.RoleID
--							WHERE	R.AccountID = @Accountid 
--						) tmp on tmp.RoleModuleMapID = RM.RoleModuleMapID

--				SET @RecordsDeleted = @@rowcount
--				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
--				SELECT @@ROWCOUNT RoleModuleMap_RolesCOUNT
--			END
--			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  RoleModuleMap'

--/*-------------Here Deletion process running on Account (child table's) Reference table's------------*/	


--			BEGIN
--				DELETE	AS1 
--				FROM	dbo.AccountSettings  AS AS1  (NOLOCK)
--				WHERE	AS1.AccountID = @Accountid 
	
--			END
--			select @@rowcount  AccountSettings1count
--			PRINT  ' records deleted from  AccountSettings1'

--			BEGIN
--				DELETE	AAM1
--				FROM	dbo.AccountAddressMap  AS AAM1   (NOLOCK)
--				WHERE	AAM1.AccountID = @Accountid 
	
--			END
--			select @@rowcount  AccountSettings1count
--			PRINT ' records deleted from  AccountAddressMap1'



--			BEGIN
--				DELETE	ADAP1
--				FROM	dbo.AccountDataAccessPermissions  AS ADAP1   (NOLOCK)
--				WHERE	ADAP1.AccountID = @Accountid
--	          SELECT @@ROWCOUNT AccountDataAccessPermissions1COUNT
--			END
--			PRINT ' records deleted from  AccountDataAccessPermissions1'

--		BEGIN
--				DELETE	ct
--				FROM	dbo.CampaignTemplates  AS ct   (NOLOCK)
--				WHERE	ct.AccountID = @Accountid
--	          SELECT @@ROWCOUNT CampaignTemplates
--			END
--			PRINT ' records deleted from  CampaignTemplates'

--			BEGIN 
--				DELETE	AE1
--				FROM	dbo.AccountEmails  AS AE1   (NOLOCK)
--				WHERE	AE1.AccountID = @Accountid 
--	        SELECT @@ROWCOUNT AccountEmailsCOUNT
--			END
--			PRINT  ' records deleted from  AccountEmails1'


--			BEGIN
--				DELETE	A1
--				FROM	dbo.Actions  AS A1  (NOLOCK)
--				WHERE	A1.AccountID = @Accountid 
--	         SELECT @@ROWCOUNT Actions1COUNT
--			END
--			PRINT  ' records deleted from  Actions1'


--			BEGIN
--				DELETE	ACR1
--				FROM	dbo.AccountCustomReports  AS ACR1   (NOLOCK)
--				WHERE	ACR1.AccountID = @Accountid 
--	          SELECT @@ROWCOUNT  AccountCustomReports1
--			END
--			PRINT  ' records deleted from  AccountCustomReports1'


--			BEGIN
--				DELETE	AA1
--				FROM	dbo.Actions_Audit  AS AA1   (NOLOCK)
--				WHERE	AA1.AccountID = @Accountid 
--	          SELECT @@ROWCOUNT Actions_Audit1
--			END
--			PRINT  ' records deleted from  Actions_Audit1'


--			BEGIN
--				DELETE	AC1
--				FROM	dbo.ActiveContacts  AS AC1   (NOLOCK)
--				WHERE	AC1.AccountID = @Accountid 
--	         SELECT @@ROWCOUNT ActiveContacts1
--			END
--			PRINT  ' records deleted from  ActiveContacts1'


--			BEGIN
--				DELETE	BO1
--				FROM	dbo.BulkOperations  AS BO1   (NOLOCK)
--				WHERE	BO1.AccountID = @Accountid 
--	        SELECT @@ROWCOUNT  BulkOperations1
--			END
--			PRINT ' records deleted from  BulkOperations1'


--			BEGIN
--				DELETE	CC1
--				FROM	dbo.ClassicCampaigns  AS CC1 (NOLOCK) 
--				WHERE	CC1.AccountID = @Accountid 
--	        SELECT @@ROWCOUNT  ClassicCampaigns1
--			END
--			PRINT  ' records deleted from  ClassicCampaigns1'


--			BEGIN
--				DELETE	C1
--				FROM	dbo.Communities  AS C1  (NOLOCK)
--				WHERE	C1.AccountID = @Accountid 
--	        SELECT @@ROWCOUNT  Communities1
--			END
--			PRINT  ' records deleted from  Communities1'


--			BEGIN
--				DELETE	CE1
--				FROM	dbo.ContactEmails  AS CE1  (NOLOCK)
--				WHERE	CE1.AccountID = @Accountid 
--	        SELECT @@ROWCOUNT ContactEmails1
--			END
--			PRINT  ' records deleted from  ContactEmails1'


--			BEGIN
--				DELETE	CEA1
--				FROM	dbo.ContactEmails_Audit  AS CEA1   (NOLOCK)
--				WHERE	CEA1.AccountID = @Accountid 
	
--	         SELECT @@ROWCOUNT ContactEmails_Audit1COUNT
--			END
--			PRINT ' records deleted from  ContactEmails_Audit1'


--			BEGIN
--				DELETE	CPN1
--				FROM	dbo.ContactPhoneNumbers  AS CPN1  (NOLOCK)
--				WHERE	CPN1.AccountID = @Accountid 
--	          SELECT @@ROWCOUNT ContactPhoneNumbers1COUNT
--			END
--			PRINT ' records deleted from  ContactPhoneNumbers1'


--			BEGIN
--				DELETE	C1
--				FROM	dbo.Contacts  AS C1  (NOLOCK)
--				WHERE	C1.AccountID = @Accountid 
--	        SELECT @@ROWCOUNT   Contacts1COUNT
--			END
--			PRINT  ' records deleted from  Contacts1'


--			BEGIN
--				DELETE	CA1
--				FROM	dbo.Contacts_Audit  AS CA1 (NOLOCK) 
--				WHERE	CA1.AccountID = @Accountid 
--	           SELECT @@ROWCOUNT Contacts_Audit1
--			END
--			PRINT  ' records deleted from  Contacts_Audit1'


--			BEGIN
--				DELETE	CFT1
--				FROM	dbo.CustomFieldTabs  AS CFT1  (NOLOCK)
--				WHERE	CFT1.AccountID = @Accountid
--	           SELECT @@ROWCOUNT   CustomFieldTabs1
--			END
--			PRINT  ' records deleted from  CustomFieldTabs1'

--			BEGIN
--				DELETE	DV1
--				FROM	dbo.DropdownValues  AS DV1  (NOLOCK)
--				WHERE	DV1.AccountID = @Accountid
--				 SELECT @@ROWCOUNT DropdownValues1COUNT
--			END
--			PRINT ' records deleted from  DropdownValues1'


--			BEGIN
--				DELETE	E1
--				FROM	dbo.Emails  AS E1  (NOLOCK)
--				WHERE	E1.AccountID = @Accountid 
--	         SELECT @@ROWCOUNT  Emails1COUNT
--			END
--			PRINT ' records deleted from  Emails1'



--			BEGIN
--				DELETE	F1
--				FROM	dbo.Fields  AS F1  (NOLOCK)
--				WHERE	F1.AccountID = @Accountid 
--	           SELECT @@ROWCOUNT   Fields1COUNT
--			END
--			PRINT  ' records deleted from  Fields1'


--			BEGIN
--				DELETE	F1
--				FROM	dbo.Forms  AS F1   (NOLOCK)
--				WHERE	F1.AccountID = @Accountid 
--	         SELECT @@ROWCOUNT   Forms1COUNT
--			END
--			PRINT ' records deleted from  Forms1'


--			BEGIN
--				DELETE	IS1
--				FROM	dbo.ImportDataSettings  AS IS1 (NOLOCK) 
--				WHERE	IS1.AccountID = @Accountid 
--	           SELECT @@ROWCOUNT  ImportDataSettings1COUNT
--			END
--			PRINT ' records deleted from  ImportDataSettings1'


--			BEGIN
--				DELETE	LAAAM1
--				FROM	dbo.LeadAdapterAndAccountMap  AS LAAAM1  (NOLOCK)
--				WHERE	LAAAM1.AccountID = @Accountid 
--	          SELECT @@ROWCOUNT LeadAdapterAndAccountMap1COUNT

--			END
--			PRINT  ' records deleted from  LeadAdapterAndAccountMap1'


--			BEGIN
--				DELETE	LSM1
--				FROM	dbo.LeadScoreMessages  AS LSM1 (NOLOCK) 
--				WHERE	LSM1.AccountID = @Accountid 
--	           SELECT @@ROWCOUNT  LeadScoreMessages1COUNT
--			END
--			PRINT  ' records deleted from  LeadScoreMessages1'


--			BEGIN
--				DELETE	LSR1
--				FROM	dbo.LeadScoreRules  AS LSR1  (NOLOCK)
--				WHERE	LSR1.AccountID = @Accountid 
--             select @@rowcount  LeadScoreRules1COUNT
--			END
--			PRINT  ' records deleted from  LeadScoreRules1'


--			BEGIN
--				DELETE	OSG1
--				FROM	dbo.OpportunityStageGroups  AS OSG1  (NOLOCK)
--				WHERE	OSG1.AccountID = @Accountid 
--	          select @@rowcount  OpportunityStageGroups1COUNT
--			END
--			PRINT  ' records deleted from  OpportunityStageGroups1'


--			BEGIN
--				DELETE	R1
--				FROM	dbo.Reports  AS R1  (NOLOCK)
--				WHERE	R1.AccountID = @Accountid 
--	           select @@rowcount  Reports1COUNT
--			END 
--			PRINT  ' records deleted from  Reports1'


--			BEGIN
--				DELETE	R1
--				FROM	dbo.Roles  AS R1  (NOLOCK)
--				WHERE	R1.AccountID = @Accountid 
--	          select @@rowcount  Roles1COUNT
--			END
--			PRINT ' records deleted from  Roles1'


--			BEGIN
--				DELETE	SD1
--				FROM	dbo.SearchDefinitions  AS SD1  (NOLOCK)
--				WHERE	SD1.AccountID = @Accountid 
--	           select @@rowcount SearchDefinitions1COUNT
--			END
--			PRINT  ' records deleted from  SearchDefinitions1'


--			BEGIN
--				DELETE	SP1
--				FROM	dbo.ServiceProviders  AS SP1  (NOLOCK)
--				WHERE	SP1.AccountID = @Accountid 
--	          select @@rowcount  ServiceProviders1COUNT
--			END
--			PRINT  ' records deleted from  ServiceProviders1'


--			BEGIN
--				DELETE	SPER1
--				FROM	dbo.StoreProcExecutionResults  AS SPER1  (NOLOCK)
--				WHERE	SPER1.AccountID = @Accountid 
--	         select @@rowcount  StoreProcExecutionResults1COUNT
--			END
--			PRINT  ' records deleted from  StoreProcExecutionResults1'


--			BEGIN
--				DELETE	SPPL1
--				FROM	dbo.StoreProcParamsList  AS SPPL1 (NOLOCK) 
--				WHERE	SPPL1.AccountID = @Accountid
--	         select @@rowcount  StoreProcParamsList1COUNT
--			END
--			PRINT  ' records deleted from  StoreProcParamsList1'


--			BEGIN
--				DELETE	SMM1
--				FROM	dbo.SubscriptionModuleMap  AS SMM1  (NOLOCK)
--				WHERE	SMM1.AccountID = @Accountid 
--	        select @@rowcount  SubscriptionModuleMap1COUNT
--			END
--	PRINT  ' records deleted from  SubscriptionModuleMap1'


--			BEGIN
--				DELETE	TPC1
--				FROM	dbo.ThirdPartyClients  AS TPC1  (NOLOCK)
--				WHERE	TPC1.AccountID = @Accountid 
--	         select @@rowcount   ThirdPartyClients1COUNT
--			END
--		PRINT ' records deleted from  ThirdPartyClients1'


--			BEGIN
--				DELETE	T1
--				FROM	dbo.Tours  AS T1  (NOLOCK)
--				WHERE	T1.AccountID = @Accountid 
--	         select @@rowcount Tours1COUNT
--			END
--		PRINT  ' records deleted from  Tours1'


--			BEGIN
--				DELETE	U1
--				FROM	dbo.Users  AS U1  (NOLOCK)
--				WHERE	U1.AccountID = @Accountid 
--	         select @@rowcount Users1COUNT
--			END
--			PRINT  ' records deleted from  Users1'


--			BEGIN
--				DELETE	WAP1
--				FROM	dbo.WebAnalyticsProviders  AS WAP1 (NOLOCK) 
--				WHERE	WAP1.AccountID = @Accountid 
--	          select @@rowcount WebAnalyticsProvidersCOUNT
--			END
--		PRINT ' records deleted from  WebAnalyticsProviders1'


--			BEGIN
--				DELETE	WVUN1
--				FROM	dbo.WebVisitUserNotificationMap  AS WVUN1 (NOLOCK) 
--				WHERE	WVUN1.AccountID = @Accountid  
--	        select @@rowcount WebVisitUserNotificationMap1COUNT
--			END
--			PRINT ' records deleted from  '


--			BEGIN
--				DELETE	W1
--				FROM	dbo.Workflows  AS W1  (NOLOCK)
--				WHERE	W1.AccountID = @Accountid  
--	         select @@rowcount Workflows1COUNT
--			END
--		PRINT  ' records deleted from  Workflows1'


--			BEGIN
--				DELETE	I1
--				FROM	dbo.Images  AS I1  (NOLOCK)
--				WHERE	I1.AccountID = @Accountid  
--	         select @@rowcount Images1COUNT
--			END
--		PRINT  ' records deleted from  Images1'


--			BEGIN
--				DELETE	ICD1
--				FROM	dbo.ImportContactData  AS ICD1  (NOLOCK)
--				WHERE	ICD1.AccountID = @Accountid  
--	           select @@rowcount  ImportContactData1COUNT
--			END
--		PRINT  ' records deleted from  ImportContactData1'


--			BEGIN
--				DELETE	O1
--				FROM	dbo.Opportunities  AS O1  (NOLOCK)
--				WHERE	O1.AccountID = @Accountid  
--	         select @@rowcount Opportunities1COUNT
--			END
--			PRINT  ' records deleted from  Opportunities1'


--			BEGIN
--				DELETE	OA1
--				FROM	dbo.Opportunities_Audit  AS OA1  (NOLOCK)
--				WHERE	OA1.AccountID = @Accountid  
--	         select @@rowcount Opportunities_Audit1COUNT
--			END
--		PRINT  ' records deleted from  Opportunities_Audit1'


--			BEGIN
--				DELETE	N1
--				FROM	dbo.Notes  AS N1  (NOLOCK)
--				WHERE	N1.AccountID = @Accountid  
--	        select @@rowcount  Notes1COUNT
--			END
--			PRINT ' records deleted from  Notes1'


--			BEGIN
--				DELETE	NA1
--				FROM	dbo.Notes_Audit  AS NA1  (NOLOCK)
--				WHERE	NA1.AccountID = @Accountid  
--	          select @@rowcount   Notes_Audit1COUNT
--			END
--			PRINT ' records deleted from  Notes_Audit1'


--			BEGIN
--				DELETE	T1
--				FROM	dbo.Tags  AS T1  (NOLOCK)
--				WHERE	T1.AccountID = @Accountid  
--	        select @@rowcount  Tags1
--			END
--		PRINT  ' records deleted from  Tags1'


--			BEGIN
--				DELETE	TA1
--				FROM	dbo.Tags_Audit  AS TA1  (NOLOCK)
--				WHERE	TA1.AccountID = @Accountid  
--	          select @@rowcount  Tags_Audit1
--			END
--			PRINT ' records deleted from  Tags_Audit1'


--			BEGIN
--				DELETE	TA1
--				FROM	dbo.Tours_Audit  AS TA1  (NOLOCK)
--				WHERE	TA1.AccountID = @Accountid  
--	         select @@rowcount  Tours_Audit1COUNT
--			END
--			PRINT  ' records deleted from  Tours_Audit1'


--			BEGIN
--				DELETE	UAL1
--				FROM	dbo.UserActivityLogs  AS UAL1  (NOLOCK)
--				WHERE	UAL1.AccountID = @Accountid 
--	          select @@rowcount UserActivityLogsCOUNT
--			END
--			PRINT  ' records deleted from  UserActivityLogs1'

--			--DELETING RECORD FROM ACCOUNT TABLE--	
           
--		BEGIN
--			DELETE	A1
--				FROM	dbo.Accounts  AS A1  (NOLOCK)
--     			WHERE	A1.AccountID = @Accountid 
--				select @@rowcount AccountsCOUNT
--		END
--	PRINT  ' records deleted from  Accounts1'

		
--			--successfull execution query-- 
--			 SELECT 'DEL-001' ResultCode 
--         Commit TRANSACTION 
--	END TRY

--BEGIN CATCH
--	ROLLBACK TRANSACTION
--		--Unsuccessful execution query-- 
--		SELECT 'DEL-002' ResultCode 
--		--Error blocking statement in between catch --
--		INSERT INTO SmartCRM_Test.dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
--		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

--END CATCH
--	SET NOCOUNT OFF
--END 
  
--/*
--	EXEC [dbo].[DELETE_Account_Records]
--		@AccountID	= 100

--*/






