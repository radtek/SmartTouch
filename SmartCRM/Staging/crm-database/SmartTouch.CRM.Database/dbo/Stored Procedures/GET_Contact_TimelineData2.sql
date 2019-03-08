



/*
		Purpose		: Get Contact Timeline Data
		Input		: ContactID
		Output		: Return Data
		Created By	: Narendra M
		Created On	: Mar 10, 2015
		Modified On	: 
*/


CREATE PROCEDURE [dbo].[GET_Contact_TimelineData2]
	(
		@AccountID		int,
		@ContactID		int
	)
AS
BEGIN
	
	SET NOCOUNT ON
	BEGIN TRY	
		
		SELECT ROW_NUMBER() OVER (ORDER BY ContactID) TimelineID, ContactID, Module, AuditAction, 
			Value, AuditDate, ModuleID, UserName, CreatedBy, AuditStatus
		FROM ( 

		SELECT ContactID, Module, CASE WHEN AuditAction = 'I' THEN 'created'
									WHEN AuditAction = 'D' THEN 'deleted'
									WHEN AuditAction = 'U' THEN 'updated'
									WHEN AuditAction = 'S' THEN 'sent'
									WHEN AuditAction = 'SU' THEN 'submitted'
									WHEN AuditAction = 'C' THEN 'marked as complete'
									WHEN AuditAction = 'IC' THEN 'marked as incomplete'
									WHEN AuditAction = 'IC' THEN 'marked as incomplete'
									WHEN AuditAction = 'CIL' THEN 'created (Lead Adapter)'
									WHEN AuditAction = 'CUL' THEN 'updated (Lead Adapter)'
									WHEN AuditAction = 'CII' THEN 'created (Import)'
									WHEN AuditAction = 'CUI' THEN 'updated (Import)'							
									WHEN AuditAction = 'WV' THEN ''
									--WHEN AuditAction = 'TC' THEN 'marked as incomplete'
									END AuditAction, 
				Value, AuditDate, ModuleID, (U.FirstName+ ' '+ U.LastName) UserName, TIMELINE.CreatedBy, AuditStatus  
		FROM 
		(
			SELECT CA.ContactID, 'Contact' Module, 
				CASE WHEN CA.AuditAction = 'I' AND CA.ContactSource = 1 THEN 'CIL'
					 WHEN CA.AuditAction = 'I' AND CA.ContactSource = 2 THEN 'CII'
					 WHEN CA.AuditAction = 'U' AND CA.ContactSource = 2 THEN 'CUI'
					 WHEN CA.AuditAction = 'U' AND CA.ContactSource = 1 THEN 'CUL'
					 ELSE CA.AuditAction END AuditAction,
				(ISNULL(CA.FirstName, '') +' '+ISNULL(CA.LastName, '')) Value, CA.AuditDate, CA.ContactID ModuleID, 
				'' UserName, CA.LastUpdatedBy CreatedBy,
				CA.AuditStatus   
			FROM dbo.Contacts_Audit CA  
			WHERE (IsLifecycleStageChanged IS NULL OR IsLifecycleStageChanged != 1) 
				AND CA.ContactID = @ContactID AND CA.AccountID = @AccountID

			UNION ALL

			SELECT CA.ContactID, 'Lifecycle' Module, 'U' AuditAction,
				'Lifecycle changed from ' + (SELECT DropdownValue FROM dbo.DropdownValues WHERE DropdownValueID = CA.LifecycleStage)
				+ ' to '+ (SELECT DropdownValue FROM dbo.DropdownValues WHERE DropdownValueID = CA.NewLifecycleStage) Value,
				CA.AuditDate, CA.ContactID ModuleID, '' UserName, CA.LastUpdatedBy CreatedBy, CA.AuditStatus   
				FROM dbo.Contacts_Audit CA 
				WHERE CA.ContactID = @ContactID AND CA.AccountID = @AccountID AND CA.IsLifecycleStageChanged = 1

			--SELECT CA.ContactID, 'Lifecycle' Module, 'U' AuditAction,
			--	'Lifecycle changed from '  Value,
			--	CA.AuditDate, CA.ContactID ModuleID, '' UserName, CA.LastUpdatedBy CreatedBy, CA.AuditStatus   
			--	FROM dbo.Contacts_Audit CA 
			--	WHERE CA.ContactID = @ContactID AND CA.AccountID = @AccountID

			UNION ALL

			SELECT CNMA.ContactID, 'Note' Module, NA.AuditAction, convert(nvarchar(max),NA.NoteDetails) Value, NA.AuditDate,
				   CNMA.NoteID ModuleID, '' UserName, NA.CreatedBy, CNMA.AuditStatus    
				FROM dbo.ContactNoteMap_Audit CNMA 
					INNER JOIN dbo.Notes_Audit NA ON CNMA.NoteID = NA.NoteID 
			WHERE CNMA.ContactID = @ContactID AND NA.AccountID = @AccountID
			
			UNION ALL

			SELECT  CTMA.ContactID, 'Action' Module, AA.AuditAction, ISNULL(AA.ActionDetails,'') Value, AA.AuditDate,
		            AA.ActionID ModuleID, '' UserName, AA.LastUpdatedBy, CTMA.AuditStatus    
	         FROM Actions_Audit AA INNER JOIN ContactActionMap_Audit CTMA ON CTMA.ActionID = AA.ActionID
             WHERE  CTMA.AuditAction in ('I') AND CTMA.ContactID = @ContactID AND AA.AccountID = @AccountID AND AA.AuditDate > CTMA.LastUpdatedOn
          
		    UNION 
         
			SELECT  CAMA.ContactID, 'Action' Module, 
					  CASE WHEN CAMA.AuditAction IN ('U','D') AND CAMA.IsCompleted = 1 THEN 'C'
					  WHEN CAMA.AuditAction IN ('U','D') AND (CAMA.IsCompleted = 0 OR CAMA.IsCompleted IS NULL) THEN 'IC' ELSE CAMA.AuditAction END AuditAction,
					  ISNULL(TAA.ActionDetails,'') Value, CAMA.AuditDate, CAMA.ActionID ModuleID, '' UserName, CAMA.LastUpdatedBy, CAMA.AuditStatus        
			   FROM dbo.ContactActionMap_Audit	CAMA 	
					CROSS APPLY (SELECT TOP 1 * FROM Actions_Audit TAU WHERE CAMA.ActionID = TAU.ActionID and TAU.AuditDate <= CAMA.LastUpdatedOn ORDER BY TAU.AuditId DESC) TAA
			   WHERE CAMA.auditaction NOT IN ('I','D') AND CAMA.ContactID =  @ContactID AND TAA.AccountID = @AccountID



			--SELECT DISTINCT CAM.ContactID, 'Action' Module, AA.AuditAction, AA.ActionDetails Value, AA.AuditDate,
			--		CAM.ActionID ModuleID, '' UserName, AA.CreatedBy, AA.AuditStatus        
			--	FROM dbo.Actions_Audit AA 
			--		INNER JOIN dbo.ContactActionMap_Audit CAM ON AA.ActionID = CAM.ActionID
			--			AND (AA.AuditAction = CAM.AuditAction OR AA.AuditAction = 'U')
			--	WHERE CAM.ContactID = @ContactID AND AA.AccountID = @AccountID
		
			--UNION ALL

			--SELECT DISTINCT CAMA.ContactID, 'Action' Module, 
			--CASE WHEN CAMA.AuditAction IN ('U','D') AND CAMA.IsCompleted = 1 THEN 'C'
			--	 WHEN CAMA.AuditAction IN ('U','D') AND (CAMA.IsCompleted = 0 OR CAMA.IsCompleted IS NULL) THEN 'IC' ELSE CAMA.AuditAction END AuditAction,
			--	 AA.ActionDetails Value, CAMA.AuditDate, CAMA.ActionID ModuleID, '' UserName, AA.CreatedBy, CAMA.AuditStatus        
			--	FROM dbo.ContactActionMap_Audit	CAMA 
			--		INNER JOIN dbo.Actions AA ON AA.ActionID = CAMA.ActionID --AND AA.AuditAction = CAMA.AuditAction
			--	WHERE CAMA.AuditAction IN ('U','D') AND CAMA.ContactID = @ContactID AND AA.AccountID = @AccountID
			
			UNION ALL

			SELECT CRMA.ContactID, 'Relationship' Module, CRMA.AuditAction,
				CASE WHEN C.ContactType = 1 THEN ISNULL(C.FirstName, '') +' '+ ISNULL(C.LastName, '') +', '+ DV.DropdownValue
				ELSE ISNULL(C.Company, '') +', '+ DV.DropdownValue END Value, CRMA.AuditDate, CRMA.ContactRelationshipMapID ModuleId,
					'' UserName, CRMA.CreatedBy CreatedBy, CRMA.AuditStatus      
				FROM dbo.ContactRelationshipMap_Audit  CRMA 
					INNER JOIN dbo.DropdownValues DV ON DV.DropdownValueID = CRMA.RelationshipType
					INNER JOIN dbo.Contacts	C ON C.ContactID =  CRMA.RelatedContactID	 
					--WHERE (CRMA.ContactID = @ContactID OR CRMA.RelatedContactID = @ContactID)  AND C.AccountID = @AccountID
				WHERE CRMA.ContactID = @ContactID AND C.AccountID = @AccountID --AND CRMA.AuditAction = 'I'

		UNION ALL

		SELECT  CTMA.ContactID, 'Tour' Module, TR.AuditAction, ISNULL(TR.TourDetails,'') Value, TR.AuditDate,
			TR.TourID ModuleID, '' UserName, TR.LastUpdatedBy, CTMA.AuditStatus    
			FROM dbo.Tours_Audit TR 
				INNER JOIN dbo.ContactTourMap_Audit CTMA ON CTMA.TourID = TR.TourID
			WHERE CTMA.AuditAction = 'I' AND CTMA.ContactID = @ContactID AND TR.AccountID = @AccountID
		
		UNION 
		SELECT  CTMA.ContactID, 'Tour' Module, 
			CASE WHEN CTMA.AuditAction IN ('U','D') AND CTMA.IsCompleted = 1 THEN 'C'
				 WHEN CTMA.AuditAction IN ('U','D') AND (CTMA.IsCompleted = 0 OR CTMA.IsCompleted IS NULL) THEN 'IC' ELSE CTMA.AuditAction END AuditAction,
			ISNULL(TAA.TourDetails,'') Value, CTMA.AuditDate, CTMA.TourID ModuleID, '' UserName, CTMA.LastUpdatedBy, CTMA.AuditStatus        
			FROM dbo.ContactTourMap_Audit	CTMA 	
				 CROSS APPLY (SELECT TOP 1 * FROM Tours_Audit TAU WHERE CTMA.TourID = TAU.TourID AND TAU.AuditDate <= CTMA.LastUpdatedOn ORDER BY TAU.AuditId DESC) TAA
			WHERE CTMA.AuditAction NOT IN ('I','D') AND CTMA.ContactID = @ContactID
		
		UNION ALL

		SELECT CR.ContactID, 'Campaign' Module, 'S' AuditAction, C.[Name] Value, CR.SentOn AuditDate,
			CR.CampaignID ModuleID, '' UserName, CASE WHEN C.LastUpdatedBy IS NULL THEN C.CreatedBy ELSE C.LastUpdatedBy END, CONVERT(BIT, 1) AuditStatus
			FROM dbo.vCampaignRecipients CR (NOLOCK)
				INNER JOIN dbo.Campaigns C ON CR.CampaignID = C.CampaignID AND C.IsDeleted = 0
			WHERE CR.ContactID = @ContactID AND C.AccountID = @AccountID

		UNION ALL

		SELECT FS.ContactID, 'Form' Module, 'SU' AuditAction, F.[Name] Value, FS.SubmittedOn AuditDate,
			FS.FormSubmissionID ModuleID, '' UserName, CASE WHEN F.LastModifiedBy IS NULL THEN F.CreatedBy ELSE F.LastModifiedBy END, CONVERT(BIT, 1) AuditStatus
			FROM dbo.FormSubmissions FS 
				INNER JOIN dbo.Forms F ON FS.FormID = F.FormID
			WHERE FS.ContactID = @ContactID AND F.AccountID = @AccountID

		UNION ALL

		SELECT D.ContactID, 'Attachment' Module, 'I' AuditAction, (D.OriginalFileName + '~' + D.FilePath) Value,
			D.CreatedDate AuditDate, CONVERT(INT,  D.DocumentID) ModuleID, '' UserName, D.CreatedBy, CONVERT(BIT, 1) AuditStatus
			FROM dbo.Documents D
			WHERE D.ContactID = @ContactID

		UNION ALL

		SELECT C.ContactID, 'Email' Module, 'S' AuditAction, SMD.[Subject] Value, CEA.SentOn AuditDate,
			SMD.SentMailDetailID ModuleID, '' UserName, CEA.SentBy, CONVERT(BIT, 1) AuditStatus
			FROM dbo.ContactEmailAudit CEA
				LEFT JOIN dbo.ContactEmails CE ON CEA.ContactEmailID = CE.ContactEmailID
				LEFT JOIN dbo.Contacts C ON C.ContactID = CE.ContactID
				LEFT JOIN EnterpriseCommunication.dbo.SentMailDetails SMD ON SMD.RequestGuid = CEA.RequestGuid
			WHERE CEA.[Status] = 1 AND C.ContactID = @ContactID AND C.AccountID = @AccountID 
				AND CE.ContactID = @ContactID AND CE.AccountID = @AccountID

		UNION ALL

			select C.ContactID,'Email Received' Module, 'R' AuditAction, rmi.[Subject] Value, rma.ReceivedOn AuditDate, 
			rmi.ReceivedMailID ModuleID,  c.FirstName + ' '+ c.LastName UserName, rma.UserId, CONVERT(BIT, 1) AuditStatus
			from ReceivedMailAudit rma
			left join dbo.users u on rma.userid = u.UserId 
			left join dbo.contacts c on rma.sentbycontactid = c.ContactID
			left join EnterpriseCommunication.dbo.ReceivedMailInfo rmi on rma.referenceid = rmi.referenceid
			where C.ContactID = @ContactID AND C.AccountID = @AccountID 
				


		UNION ALL
		SELECT C.ContactID, 'Text' Module, 'S' AuditAction, TRD.[Message] Value, CEA.SentOn AuditDate, 
			CEA.ContactPhoneNumberID ModuleID, '' UserName, CEA.SentBy, CONVERT(BIT, 1) AuditStatus
			FROM dbo.ContactTextMessageAudit CEA
				LEFT JOIN dbo.ContactPhoneNumbers CE ON CEA.ContactPhoneNumberID = CE.ContactPhoneNumberID
				LEFT JOIN dbo.Contacts C ON C.ContactID = CE.ContactID
				LEFT JOIN EnterpriseCommunication.dbo.TextResponse TR ON TR.RequestGuid = CEA.RequestGuid
				LEFT JOIN EnterpriseCommunication.dbo.TextResponseDetails TRD ON TRD.TextResponseID = TR.TextResponseID
			WHERE CEA.[Status] = 1 AND C.ContactID = @ContactID AND C.AccountID = @AccountID

		UNION ALL

		SELECT C.ContactID, 'Lead Adapter' Module, 'SU' AuditAction, LT.Name Value, LAJD.CreatedDateTime AuditDate,
			LAJD.LeadAdapterJobLogDetailID ModuleID, '' UserName, NULL, CONVERT(BIT, 1) AuditStatus
			FROM dbo.Contacts C 
				INNER JOIN dbo.LeadAdapterJobLogDetails LAJD ON C.ReferenceID = LAJD.ReferenceID
				INNER JOIN dbo.LeadAdapterJobLogs LAJ ON LAJ.LeadAdapterJobLogID = LAJD.LeadAdapterJobLogID
				INNER JOIN dbo.LeadAdapterAndAccountMap LAAM ON LAJ.LeadAdapterAndAccountMapID = LAAM.LeadAdapterAndAccountMapID
				INNER JOIN dbo.LeadAdapterTypes LT ON LT.LeadAdapterTypeID = LAAM.LeadAdapterTypeID
			WHERE LAAM.LeadAdapterTypeID != 11 AND C.ContactID = @ContactID AND C.AccountID = @AccountID
				AND LAAM.AccountID = @AccountID

		UNION ALL

		SELECT CTM.ContactID, 'Tag' Module, 'I' AuditAction, TA.[TagName] Value, CTM.TaggedOn AuditDate,
			   CTM.TagID ModuleID, '' UserName, TA.CreatedBy, CONVERT(BIT, 1) AuditStatus    
			FROM dbo.ContactTagMap CTM 
				INNER JOIN dbo.Tags_Audit TA ON CTM.TagID = TA.TagID
			WHERE CTM.ContactID = @ContactID AND TA.AccountID = @AccountID

		UNION ALL

		SELECT CWV.ContactID, 'Web Visit' Module, 'WV' AuditAction, CWV.PageVisited Value,
				CWV.VisitedOn AuditDate, CWV.ContactWebVisitID ModuleID, 
				(ISNULL(C.FirstName, '') +' '+ISNULL(C.LastName, '')) UserName, '' CreatedBy, CONVERT(BIT, 1) AuditStatus 
			FROM dbo.ContactWebVisits CWV 
				INNER JOIN dbo.Contacts C ON CWV.ContactID = C.ContactID
		WHERE CWV.ContactID = @ContactID AND C.ContactID = @ContactID AND C.AccountID = @AccountID AND CWV.IsVisit = 1

		) TIMELINE LEFT JOIN dbo.Users U ON U.UserID = TIMELINE.CreatedBy
		  
		) C ORDER BY AuditDate DESC

	END TRY
	BEGIN CATCH
		SELECT CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE()

	END CATCH
	SET NOCOUNT OFF

END

/*

SET STATISTICS TIME ON 

	EXEC  [dbo].[GET_Contact_TimelineData]
		@AccountID	= 4218,
		@ContactID = 1052899

SET STATISTICS TIME OFF

*/












