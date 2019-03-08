





CREATE VIEW [dbo].[Get_Import_Data]
AS

	SELECT LeadAdapterJobLogID, LeadAdapterAndAccountMapID, StartDate, EndDate, LeadAdapterJobStatusID, Remarks, [FileName], CreatedBy, CreatedDateTime, RecordCreated, RecordUpdated, (RecordCreated + RecordUpdated) TotalRecords
	FROM (
		SELECT LeadAdapterJobLogID, LeadAdapterAndAccountMapID, StartDate, EndDate, LeadAdapterJobStatusID, Remarks, [FileName], CreatedBy, CreatedDateTime
			,(SELECT count(1) FROM dbo.LeadAdapterJobLogDetails LALD WHERE LALD.LeadAdapterJobLogID	= LAJL.LeadAdapterJobLogID AND LALD.LeadAdapterRecordStatusID = 1) RecordCreated
			,(SELECT count(1) FROM dbo.LeadAdapterJobLogDetails LALD WHERE LALD.LeadAdapterJobLogID = LAJL.LeadAdapterJobLogID AND LALD.LeadAdapterRecordStatusID = 3) RecordUpdated
		  FROM dbo.LeadAdapterJobLogs LAJL 
		) ImportData







