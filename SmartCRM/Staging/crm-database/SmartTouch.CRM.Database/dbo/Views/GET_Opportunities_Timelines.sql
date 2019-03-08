







CREATE VIEW [dbo].[GET_Opportunities_Timelines]
AS

SELECT ROW_NUMBER() OVER (ORDER BY OpportunityID) TimelineID, 
		OpportunityID,  Module, CASE WHEN AuditAction = 'I' THEN 'created'
											WHEN AuditAction = 'D' THEN 'deleted'
											WHEN AuditAction = 'U' THEN 'updated'
											WHEN AuditAction = 'C' THEN 'marked as complete'
									        WHEN AuditAction = 'IC' THEN 'marked as incomplete'
											END AuditAction, Value, AuditDate, ModuleId, UserName, CreatedBy,
CASE WHEN convert(varchar(10), TimeLineDate, 103) = convert(varchar(10), dateadd(day,datediff(day,0,GETUTCDATE()),0), 103) 
		THEN 'Today'
	WHEN convert(varchar(10), TimeLineDate, 103) = convert(varchar(10), dateadd(day,datediff(day,1,GETUTCDATE()),0), 103) 
		then 'Yesterday'
	WHEN (convert(varchar(10), TimeLineDate, 103) = convert(varchar(10), dateadd(day,datediff(day,2,GETUTCDATE()),0), 103)) 
	or (convert(varchar(10), TimeLineDate, 103) = convert(varchar(10), dateadd(day,datediff(day,3,GETUTCDATE()),0), 103))
	or (convert(varchar(10), TimeLineDate, 103) = convert(varchar(10), dateadd(day,datediff(day,4,GETUTCDATE()),0), 103))
	or (convert(varchar(10), TimeLineDate, 103) = convert(varchar(10), dateadd(day,datediff(day,5,GETUTCDATE()),0), 103))
	or (convert(varchar(10), TimeLineDate, 103) = convert(varchar(10), dateadd(day,datediff(day,6,GETUTCDATE()),0), 103))
	or (convert(varchar(10), TimeLineDate, 103) = convert(varchar(10), dateadd(day,datediff(day,7,GETUTCDATE()),0), 103))
	then DATENAME(dw,TimeLineDate)
	ELSE CONVERT(varchar(30), TimeLineDate, 103)
	END TimeLineDate, 
CASE WHEN convert(varchar(10), TimeLineDate, 103) = convert(varchar(10), dateadd(day,datediff(day,0,GETUTCDATE()),0), 103) 
	THEN 
	CASE WHEN Convert(varchar(30), DATEDIFF(HH, TimeLineTime, GETUTCDATE()), 115)>0
		THEN Convert(varchar(30), DATEDIFF(HH, TimeLineTime, GETUTCDATE()), 115)+' hours ago' 
	 --WHEN CAST(DATEPART(minute, AuditDate)as varchar(2))>1
		--THEN CAST(DATEPART(minute, AuditDate)as varchar(2)) +' minutes ago' 
	 ELSE
		'Now'
	 END
	ELSE CONVERT(VARCHAR(5),AuditDate,108) END TimeLineTime, AuditStatus  
FROM 
(
	SELECT OP.OpportunityID, 'Opportunity' Module, OP.AuditAction,
	OP.OpportunityName Value, OP.AuditDate, OP.OpportunityID ModuleId, 
	CASE WHEN OP.AuditAction = 'I' THEN (SELECT FirstName+' '+LastName FROM dbo.[Users] WHERE UserID = OP.CreatedBy)
		ELSE (SELECT FirstName+' '+LastName FROM dbo.[Users] WHERE UserID = OP.LastModifiedBy) END UserName,
	CASE WHEN OP.AuditAction = 'I' THEN OP.CreatedBy ELSE OP.LastModifiedBy END CreatedBy,
	OP.AuditDate TimeLineDate, OP.AuditDate TimeLineTime, OP.AuditStatus   
	FROM dbo.Opportunities_Audit OP  

UNION ALL

SELECT  ONMA.OpportunityID, 'Note' Module, NA.AuditAction, 
			--NA.NoteDetails Value,
			 convert(nvarchar(max),NA.NoteDetails +'|'+Convert(varchar(1),na.AddToContactSummary)+'|') Value,
			 NA.AuditDate, ONMA.NoteID ModuleId, (U.FirstName+ ' '+ U.LastName)UserName , NA.CreatedBy, 
			NA.AuditDate TimeLineDate, NA.AuditDate TimeLineTime, ONMA.AuditStatus    
		FROM dbo.OpportunityNoteMap_Audit	ONMA 
			INNER JOIN dbo.Notes_Audit	NA	 ON ONMA.NoteID = NA.NoteID	
			INNER JOIN dbo.Users U ON U.UserID	= NA.CreatedBy
			
UNION ALL


SELECT  OAMA.OpportunityID, 'Action' Module, AA.AuditAction,
		 AA.ActionDetails Value, AA.AuditDate, AA.ActionID ModuleId, (U.FirstName+ ' '+ U.LastName)UserName, AA.CreatedBy,
		 AA.AuditDate TimeLineDate, AA.AuditDate TimeLineTime, OAMA.AuditStatus 
		FROM dbo.OpportunityActionMap_Audit	OAMA 
			INNER JOIN dbo.Actions_Audit AA ON AA.ActionID = OAMA.ActionID --AND AA.AuditAction = 'I'
			INNER JOIN dbo.Users U ON U.UserID  = AA.CreatedBy
		WHERE AA.AuditAction in ('I','U') 
UNION 

SELECT  OAMA.OpportunityID, 'Action' Module,   CASE WHEN OAMA.AuditAction IN ('U','D') AND OAMA.IsCompleted = 1 THEN 'C'
					  WHEN OAMA.AuditAction IN ('U','D') AND (OAMA.IsCompleted = 0 OR OAMA.IsCompleted IS NULL) THEN 'IC' ELSE OAMA.AuditAction END AuditAction,
		 ISNULL(TAA.ActionDetails, '') Value, OAMA.AuditDate, TAA.ActionID ModuleId, (TAA.FirstName+ ' '+ TAA.LastName) UserName, TAA.CreatedBy,
		 TAA.AuditDate TimeLineDate, TAA.AuditDate TimeLineTime, OAMA.AuditStatus        
		FROM dbo.OpportunityActionMap_Audit	OAMA 					
	  CROSS APPLY (SELECT TOP 1 TAU.ActionDetails,TAU.AuditDate,TAU.ActionID,TAU.CreatedBy,U.FirstName,U.LastName FROM Actions_Audit TAU  INNER JOIN dbo.Users U ON U.UserID  = TAU.CreatedBy 
	  WHERE OAMA.ActionID = TAU.ActionID  ORDER BY TAU.AuditId DESC) TAA  where OAMA.auditaction NOT IN ('I','D') 
	  --order by AuditDate asc

UNION 

SELECT  OAMA.OpportunityID, 'Action' Module, AA.AuditAction,
		 AA.ActionDetails Value, AA.AuditDate, AA.ActionID ModuleId, (U.FirstName+ ' '+ U.LastName)UserName, AA.CreatedBy,
		 AA.AuditDate TimeLineDate, AA.AuditDate TimeLineTime, OAMA.AuditStatus 
		FROM dbo.OpportunityActionMap_Audit	OAMA 
			INNER JOIN dbo.Actions_Audit AA ON AA.ActionID = OAMA.ActionID --AND AA.AuditAction = 'I'
			INNER JOIN dbo.Users U ON U.UserID  = AA.CreatedBy
		WHERE AA.AuditAction in ('D') 
       
UNION ALL

SELECT ORMA.OpportunityID, 'Relationship' Module, ORMA.AuditAction,
			DV.DropdownValue Value, ORMA.AuditDate, DV.DropdownValueID ModuleId, 'Admin' UserName, 1 CreatedBy,
			ORMA.AuditDate TimeLineDate, ORMA.AuditDate TimeLineTime, ORMA.AuditStatus      
		FROM dbo.OpportunitiesRelationshipMap_Audit  ORMA 
			INNER JOIN dbo.DropdownValues DV   ON DV.DropdownValueID = ORMA.RelationshipTypeID

UNION ALL

SELECT OTMA.OpportunityID, 'Tour' Module, TA.AuditAction,
		ISNULL(TA.TagName,'') Value, TA.AuditDate, TA.TagID ModuleId, (U.FirstName+ ' '+ U.LastName)UserName, TA.CreatedBy,
			TA.AuditDate TimeLineDate, TA.AuditDate TimeLineTime, OTMA.AuditStatus    
		FROM dbo.OpportunityTagMap_Audit	OTMA 		
			INNER JOIN dbo.Tags_Audit TA ON TA.TagID = OTMA.TagID
			INNER JOIN dbo.Users	   U  ON U.UserID  = TA.CreatedBy


) TIMELINE





