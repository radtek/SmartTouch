
CREATE PROCEDURE [dbo].[GET_Contact_TimelineData_RowCounts]
	(
		@AccountID		int,
		@ContactID		int,
		@Email			VARCHAR(MAX)
	)
AS
BEGIN

		SELECT @AccountID AccountID, @ContactID ContactID, @Email Email, SUM(RowCounts) RowCounts
		FROM 
		(
			SELECT COUNT(CA.ContactID) RowCounts
			FROM dbo.Contacts_Audit(NOLOCK) CA  
			WHERE (IsLifecycleStageChanged IS NULL OR IsLifecycleStageChanged != 1) 
				AND CA.ContactID = @ContactID AND CA.AccountID = @AccountID

			UNION ALL

			SELECT COUNT(CA.ContactID) RowCounts
				FROM dbo.Contacts_Audit(NOLOCK) CA 
				WHERE CA.ContactID = @ContactID AND CA.AccountID = @AccountID AND CA.IsLifecycleStageChanged = 1

			UNION ALL

			SELECT COUNT(CNMA.ContactID) RowCounts
				FROM dbo.ContactNoteMap_Audit(NOLOCK) CNMA 
					INNER JOIN dbo.Notes_Audit(NOLOCK) NA ON CNMA.NoteID = NA.NoteID 
			WHERE CNMA.ContactID = @ContactID AND NA.AccountID = @AccountID
			
			UNION ALL

			SELECT COUNT(CTMA.ContactID) RowCounts
	         FROM Actions_Audit(NOLOCK) AA INNER JOIN ContactActionMap_Audit(NOLOCK) CTMA ON CTMA.ActionID = AA.ActionID
             WHERE  CTMA.AuditAction in ('I') AND CTMA.ContactID = @ContactID AND AA.AccountID = @AccountID --AND AA.AuditDate > CTMA.LastUpdatedOn
          
		    UNION ALL
         
			SELECT  COUNT(CAMA.ContactID) RowCounts
			   FROM dbo.ContactActionMap_Audit(NOLOCK)	CAMA 	
					CROSS APPLY (SELECT TOP 1 * FROM Actions_Audit(NOLOCK) TAU WHERE CAMA.ActionID = TAU.ActionID and TAU.AuditDate <= CAMA.LastUpdatedOn ORDER BY TAU.AuditId DESC) TAA
			   WHERE CAMA.auditaction NOT IN ('I','D') AND CAMA.ContactID =  @ContactID AND TAA.AccountID = @AccountID
			
			UNION ALL

			SELECT COUNT(CRMA.ContactID) RowCounts  
				FROM dbo.ContactRelationshipMap_Audit(NOLOCK)  CRMA 
					INNER JOIN dbo.DropdownValues(NOLOCK) DV ON DV.DropdownValueID = CRMA.RelationshipType
					INNER JOIN dbo.Contacts(NOLOCK)	C ON C.ContactID =  CRMA.RelatedContactID	 
				WHERE CRMA.ContactID = @ContactID AND C.AccountID = @AccountID 

		UNION ALL

		SELECT  COUNT(CTMA.ContactID) RowCounts  
			FROM dbo.Tours_Audit(NOLOCK) TR 
				INNER JOIN dbo.ContactTourMap_Audit(NOLOCK) CTMA ON CTMA.TourID = TR.TourID
			WHERE CTMA.AuditAction = 'I' AND CTMA.ContactID = @ContactID AND TR.AccountID = @AccountID
		
		UNION ALL 

		SELECT  COUNT(CTMA.ContactID) RowCounts     
			FROM dbo.ContactTourMap_Audit(NOLOCK)	CTMA 	
				 CROSS APPLY (SELECT TOP 1 * FROM Tours_Audit(NOLOCK) TAU WHERE CTMA.TourID = TAU.TourID AND TAU.AuditDate <= CTMA.LastUpdatedOn ORDER BY TAU.AuditId DESC) TAA
			WHERE CTMA.AuditAction NOT IN ('I','D') AND CTMA.ContactID = @ContactID
		
		UNION ALL

		SELECT COUNT(CR.ContactID) RowCounts
			FROM dbo.CampaignRecipients CR (NOLOCK)
				INNER JOIN dbo.Campaigns(NOLOCK) C ON CR.CampaignID = C.CampaignID AND CR.AccountID = C.AccountID AND C.IsDeleted = 0
			WHERE CR.ContactID = @ContactID AND C.AccountID = @AccountID

		UNION ALL

		SELECT COUNT(FS.ContactID) RowCounts
			FROM dbo.FormSubmissions(NOLOCK) FS 
				INNER JOIN dbo.Forms(NOLOCK) F ON FS.FormID = F.FormID
			WHERE FS.ContactID = @ContactID AND F.AccountID = @AccountID

		UNION ALL

		SELECT COUNT(D.ContactID) RowCounts
			FROM dbo.Documents(NOLOCK) D
			WHERE D.ContactID = @ContactID

		UNION ALL

		SELECT COUNT(C.ContactID) RowCounts
			FROM dbo.ContactEmailAudit(NOLOCK) CEA
				LEFT JOIN dbo.ContactEmails(NOLOCK) CE ON CEA.ContactEmailID = CE.ContactEmailID
				LEFT JOIN dbo.Contacts(NOLOCK) C ON C.ContactID = CE.ContactID AND C.AccountID = CE.AccountID
				LEFT JOIN EnterpriseCommunication.dbo.SentMailDetails(NOLOCK) SMD ON SMD.RequestGuid = CEA.RequestGuid
			WHERE CEA.[Status] = 1 AND C.ContactID = @ContactID AND C.AccountID = @AccountID 
				AND CE.ContactID = @ContactID AND CE.AccountID = @AccountID

		UNION ALL

			SELECT COUNT(C.ContactID) RowCounts
			FROM ReceivedMailAudit(NOLOCK) rma
			LEFT JOIN dbo.users(NOLOCK) u ON rma.userid = u.UserId 
			LEFT JOIN dbo.contacts(NOLOCK) c ON rma.sentbycontactid = c.ContactID
			LEFT JOIN EnterpriseCommunication.dbo.ReceivedMailInfo(NOLOCK) rmi ON rma.referenceid = rmi.referenceid
			WHERE C.ContactID = @ContactID AND C.AccountID = @AccountID 
				
		UNION ALL

		SELECT COUNT(C.ContactID) RowCounts
			FROM dbo.ContactTextMessageAudit(NOLOCK) CEA
				LEFT JOIN dbo.ContactPhoneNumbers(NOLOCK) CE ON CEA.ContactPhoneNumberID = CE.ContactPhoneNumberID
				LEFT JOIN dbo.Contacts(NOLOCK) C ON C.ContactID = CE.ContactID AND C.AccountID = CE.AccountID
				LEFT JOIN EnterpriseCommunication.dbo.TextResponse(NOLOCK) TR ON TR.RequestGuid = CEA.RequestGuid
				LEFT JOIN EnterpriseCommunication.dbo.TextResponseDetails(NOLOCK) TRD ON TRD.TextResponseID = TR.TextResponseID
			WHERE CEA.[Status] = 1 AND C.ContactID = @ContactID AND C.AccountID = @AccountID

		UNION ALL

		SELECT COUNT(C.ContactID) RowCounts
			FROM dbo.Contacts(NOLOCK) C 
				INNER JOIN dbo.LeadAdapterJobLogDetails(NOLOCK) LAJD ON C.ReferenceID = LAJD.ReferenceID
				INNER JOIN dbo.LeadAdapterJobLogs(NOLOCK) LAJ ON LAJ.LeadAdapterJobLogID = LAJD.LeadAdapterJobLogID
				INNER JOIN dbo.LeadAdapterAndAccountMap(NOLOCK) LAAM ON LAJ.LeadAdapterAndAccountMapID = LAAM.LeadAdapterAndAccountMapID AND LAAM.AccountID = C.AccountID
				INNER JOIN dbo.LeadAdapterTypes(NOLOCK) LT ON LT.LeadAdapterTypeID = LAAM.LeadAdapterTypeID
			WHERE LAAM.LeadAdapterTypeID != 11 AND C.ContactID = @ContactID AND C.AccountID = @AccountID
				AND LAAM.AccountID = @AccountID

		UNION ALL

		SELECT COUNT(CTM.ContactID) RowCounts  
			FROM dbo.ContactTagMap(NOLOCK) CTM 
				INNER JOIN dbo.Tags_Audit(NOLOCK) TA ON CTM.TagID = TA.TagID
			WHERE CTM.ContactID = @ContactID AND TA.AccountID = @AccountID

		UNION ALL

		SELECT COUNT(CWV.ContactID) RowCounts
			FROM dbo.ContactWebVisits(NOLOCK) CWV 
				INNER JOIN dbo.Contacts(NOLOCK) C ON CWV.ContactID = C.ContactID
		WHERE CWV.ContactID = @ContactID AND C.ContactID = @ContactID AND C.AccountID = @AccountID AND CWV.IsVisit = 1

		) C

END

/*

SET STATISTICS TIME ON 

	EXEC  [dbo].[GET_Contact_TimelineData]
		@AccountID	= 4218,
		@ContactID = 1052899

SET STATISTICS TIME OFF

*/





GO


