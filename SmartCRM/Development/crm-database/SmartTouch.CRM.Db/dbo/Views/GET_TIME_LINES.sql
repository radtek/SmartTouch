






CREATE VIEW [dbo].[GET_TIME_LINES]
AS

SELECT ROW_NUMBER() OVER (ORDER BY ContactID) TimelineID, ContactID, Module, AuditAction, 
	Value, AuditDate, ModuleId, UserName, CreatedBy, AuditStatus
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
							WHEN AuditAction = 'CII' THEN 'created (Import)'
							WHEN AuditAction = 'CUI' THEN 'updated (Import)'							
							WHEN AuditAction = 'WV' THEN ''
							--WHEN AuditAction = 'TC' THEN 'marked as incomplete'
							END AuditAction, 
		Value, AuditDate, ModuleId, (U.FirstName+ ' '+ U.LastName) UserName, TIMELINE.CreatedBy, AuditStatus  
FROM 
(
	SELECT CA.ContactID, 'Contact' Module, 
		CASE WHEN CA.AuditAction = 'I' AND CA.ContactSource = 1 THEN 'CIL'
			 WHEN CA.AuditAction = 'I' AND CA.ContactSource = 2 THEN 'CII'
			 WHEN CA.AuditAction = 'U' AND CA.ContactSource = 2 THEN 'CUI'
			 ELSE CA.AuditAction END AuditAction,
		(ISNULL(CA.FirstName, '') +' '+ISNULL(CA.LastName, '')) Value, CA.AuditDate, CA.ContactID ModuleId, 
		'' UserName, CA.LastUpdatedBy CreatedBy,
		CA.AuditStatus   
	FROM dbo.Contacts_Audit CA  
	WHERE IsLifecycleStageChanged IS NULL OR IsLifecycleStageChanged != 1

UNION ALL

SELECT CA.ContactID, 'Lifecycle' Module, 'U' AuditAction,
	'Lifecycle changed from ' + (SELECT DropdownValue FROM dbo.DropdownValues WHERE DropdownValueID = CA.LifecycleStage)
	+ ' to '+ +(SELECT DropdownValue FROM dbo.DropdownValues WHERE DropdownValueID = CA.NewLifecycleStage) Value,
	CA.AuditDate, CA.ContactID ModuleId, '' UserName, CA.LastUpdatedBy CreatedBy, CA.AuditStatus   
	FROM dbo.Contacts_Audit CA 
	WHERE IsLifecycleStageChanged = 1

UNION ALL

SELECT CNMA.ContactID, 'Note' Module, NA.AuditAction, NA.NoteDetails Value, NA.AuditDate,
	   CNMA.NoteID ModuleId, '' UserName, NA.CreatedBy, CNMA.AuditStatus    
	FROM dbo.ContactNoteMap_Audit CNMA 
		INNER JOIN dbo.Notes_Audit NA ON CNMA.NoteID = NA.NoteID
			
UNION ALL

SELECT  CTMA.ContactID, 'Action' Module, AA.AuditAction, ISNULL(AA.ActionDetails,'') Value, AA.AuditDate,
		AA.ActionID ModuleId, '' UserName, AA.LastUpdatedBy, CTMA.AuditStatus    
	    FROM Actions_Audit AA inner join ContactActionMap_Audit CTMA on CTMA.ActionID = AA.ActionID
        where  CTMA.AuditAction in ('I') --and CTMA.ContactID = 1051582
UNION 
SELECT  CAMA.ContactID, 'Action' Module, 
	CASE WHEN CAMA.AuditAction IN ('U','D') AND CAMA.IsCompleted = 1 THEN 'C'
		 WHEN CAMA.AuditAction IN ('U','D') AND (CAMA.IsCompleted = 0 OR CAMA.IsCompleted IS NULL) THEN 'IC' ELSE CAMA.AuditAction END AuditAction,
		 ISNULL(TAA.ActionDetails,'') Value, CAMA.AuditDate, CAMA.ActionID ModuleId, '' UserName, CAMA.LastUpdatedBy, CAMA.AuditStatus        
		 FROM dbo.ContactActionMap_Audit	CAMA 	
	     CROSS APPLY (SELECT TOP 1 * from Actions_Audit TAU where CAMA.ActionID = TAU.ActionID and TAU.AuditDate <= CAMA.LastUpdatedOn order by TAU.AuditId desc) TAA
         where CAMA.auditaction not in ('I','D') --and CAMA.ContactID = 1051582;
/*
UNION ALL

SELECT DISTINCT CAMA.ContactID, 'Action' Module, 
	CASE WHEN CAMA.AuditAction IN ('D') AND CAMA.IsCompleted = 1 THEN 'C'
		 WHEN CAMA.AuditAction IN ('D') AND (CAMA.IsCompleted = 0 OR CAMA.IsCompleted IS NULL) THEN 'IC' ELSE CAMA.AuditAction END AuditAction,
		 AA.ActionDetails Value, MAX(AA.AuditDate) AuditDate, CAMA.ActionID ModuleId, '' UserName, AA.CreatedBy, CAMA.AuditStatus        
		FROM dbo.ContactActionMap_Audit	CAMA 
			INNER JOIN dbo.Actions_Audit AA ON AA.ActionID = CAMA.ActionID AND AA.AuditAction = CAMA.AuditAction
		WHERE CAMA.AuditAction IN ('D')
	GROUP BY CAMA.ContactID, CAMA.AuditAction, CAMA.IsCompleted, AA.ActionDetails, CAMA.ActionID, AA.CreatedBy, CAMA.AuditStatus
*/
UNION ALL

SELECT CRMA.ContactID, 'Relationship' Module, CRMA.AuditAction,
		CASE WHEN C.ContactType = 1 THEN ISNULL(C.FirstName, '') +' '+ ISNULL(C.LastName, '') +', '+ DV.DropdownValue
		ELSE ISNULL(C.Company, '') +', '+ DV.DropdownValue END Value, CRMA.AuditDate, CRMA.ContactRelationshipMapID ModuleId,
			'' UserName, CRMA.CreatedBy CreatedBy, CRMA.AuditStatus      
		FROM dbo.ContactRelationshipMap_Audit  CRMA 
			INNER JOIN dbo.DropdownValues DV ON DV.DropdownValueID = CRMA.RelationshipType
			INNER JOIN dbo.Contacts	C ON C.ContactID = CRMA.RelatedContactID 

UNION ALL

SELECT  CTMA.ContactID, 'Tour' Module, TR.AuditAction, ISNULL(TR.TourDetails,'') Value, TR.AuditDate,
		TR.TourID ModuleId, '' UserName, TR.LastUpdatedBy, CTMA.AuditStatus    
	    FROM Tours_Audit TR inner join ContactTourMap_Audit CTMA on CTMA.TourID = TR.TourID
        where  CTMA.AuditAction in ('I') --and CTMA.ContactID = 1034522
UNION 
SELECT  CTMA.ContactID, 'Tour' Module, 
	CASE WHEN CTMA.AuditAction IN ('U','D') AND CTMA.IsCompleted = 1 THEN 'C'
		 WHEN CTMA.AuditAction IN ('U','D') AND (CTMA.IsCompleted = 0 OR CTMA.IsCompleted IS NULL) THEN 'IC' ELSE CTMA.AuditAction END AuditAction,
		 ISNULL(TAA.TourDetails,'') Value, CTMA.AuditDate, CTMA.TourID ModuleId, '' UserName, CTMA.LastUpdatedBy, CTMA.AuditStatus        
		 FROM dbo.ContactTourMap_Audit	CTMA 	
	     CROSS APPLY (SELECT TOP 1 * from Tours_Audit TAU where CTMA.TourID = TAU.TourID and TAU.AuditDate <= CTMA.LastUpdatedOn order by TAU.AuditId desc) TAA
         where CTMA.auditaction not in ('I','D') --and CTMA.ContactID = 1034522;

UNION ALL

SELECT CR.ContactID, 'Campaign' Module, 'S' AuditAction, C.[Name] Value, CR.SentOn AuditDate,
	CR.CampaignID ModuleId, '' UserName, C.CreatedBy, CONVERT(BIT, 1) AuditStatus
	FROM dbo.CampaignRecipients (NOLOCK) CR 
		INNER JOIN dbo.Campaigns C ON CR.CampaignID = C.CampaignID

UNION ALL

SELECT FS.ContactID, 'Form' Module, 'SU' AuditAction, F.[Name] Value, FS.SubmittedOn AuditDate,
	FS.FormSubmissionID ModuleId, '' UserName, F.CreatedBy, CONVERT(BIT, 1) AuditStatus
	FROM dbo.FormSubmissions FS 
		INNER JOIN dbo.Forms F ON FS.FormID = F.FormID

UNION ALL

SELECT D.ContactID, 'Attachment' Module, 'I' AuditAction, (D.OriginalFileName + '~' + D.FilePath) Value,
	D.CreatedDate AuditDate, CONVERT(INT,  D.DocumentID) ModuleId, '' UserName, D.CreatedBy, CONVERT(BIT, 1) AuditStatus
	FROM dbo.Documents D

UNION ALL

SELECT C.ContactID, 'Email' Module, 'S' AuditAction, SMD.[Subject] Value, CEA.SentOn AuditDate,
	CEA.ContactEmailID ModuleId, '' UserName, CEA.SentBy, CONVERT(BIT, 1) AuditStatus
	FROM dbo.ContactEmailAudit CEA
		LEFT JOIN dbo.ContactEmails CE ON CEA.ContactEmailID = CE.ContactEmailID
		LEFT JOIN dbo.Contacts C ON C.ContactID = CE.ContactID AND C.AccountID = CE.AccountID
		LEFT JOIN [$(EnterpriseCommunicationDb)].dbo.SentMailDetails SMD ON SMD.RequestGuid = CEA.RequestGuid
	WHERE CEA.[Status] = 1

UNION ALL

SELECT C.ContactID, 'Text' Module, 'S' AuditAction, TRD.[Message] Value, CEA.SentOn AuditDate, 
	CEA.ContactPhoneNumberID ModuleId, '' UserName, CEA.SentBy, CONVERT(BIT, 1) AuditStatus
	FROM dbo.ContactTextMessageAudit CEA
		LEFT JOIN dbo.ContactPhoneNumbers CE ON CEA.ContactPhoneNumberID = CE.ContactPhoneNumberID
		LEFT JOIN dbo.Contacts C ON C.ContactID = CE.ContactID AND C.AccountID = CE.AccountID
		LEFT JOIN [$(EnterpriseCommunicationDb)].dbo.TextResponse TR ON TR.RequestGuid = CEA.RequestGuid
		LEFT JOIN [$(EnterpriseCommunicationDb)].dbo.TextResponseDetails TRD ON TRD.TextResponseID = TR.TextResponseID
	WHERE CEA.[Status] = 1

--UNION ALL

--SELECT C.ContactID, 'Lead Adapter' Module, 'SU' AuditAction, LT.Name Value, LAJD.CreatedDateTime AuditDate,
--	LAJD.LeadAdapterJobLogDetailID ModuleId, '' UserName, NULL, CONVERT(BIT, 1) AuditStatus
--	FROM dbo.Contacts C 
--		INNER JOIN dbo.LeadAdapterJobLogDetails LAJD ON C.ReferenceID = LAJD.ReferenceID
--		INNER JOIN dbo.LeadAdapterJobLogs LAJ ON LAJ.LeadAdapterJobLogID = LAJD.LeadAdapterJobLogID
--		INNER JOIN dbo.LeadAdapterAndAccountMap LAAM ON LAJ.LeadAdapterAndAccountMapID = LAAM.LeadAdapterAndAccountMapID
--		INNER JOIN dbo.LeadAdapterTypes LT ON LT.LeadAdapterTypeID = LAAM.LeadAdapterTypeID
--	WHERE LAAM.LeadAdapterTypeID != 11

UNION ALL

SELECT CTM.ContactID, 'Tag' Module, 'I' AuditAction, TA.[TagName] Value, CTM.TaggedOn AuditDate,
	   CTM.TagID ModuleId, '' UserName, TA.CreatedBy, CONVERT(BIT, 1) AuditStatus    
	FROM dbo.ContactTagMap CTM 
		INNER JOIN dbo.Tags_Audit TA ON CTM.TagID = TA.TagID

UNION ALL

SELECT CWV.ContactID, 'Web Visit' Module, 'WV' AuditAction, CWV.PageVisited Value,
		CWV.VisitedOn AuditDate, CWV.ContactWebVisitID ModuleId, 
		(ISNULL(C.FirstName, '') +' '+ISNULL(C.LastName, '')) UserName, '' CreatedBy, CONVERT(BIT, 1) AuditStatus 
	FROM dbo.ContactWebVisits CWV INNER JOIN dbo.Contacts C ON CWV.ContactID = C.ContactID

) TIMELINE LEFT JOIN dbo.Users U ON U.UserID = TIMELINE.CreatedBy

) C











