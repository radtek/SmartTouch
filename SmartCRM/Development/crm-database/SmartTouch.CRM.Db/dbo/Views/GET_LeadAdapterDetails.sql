







CREATE VIEW [dbo].[GET_LeadAdapterDetails]
AS
 SELECT LAJL.[FileName], LAJL.Remarks, LAJL.CreatedDateTime, LAJL.LeadAdapterJobStatusID, LAJL.LeadAdapterJobLogID, LAJS.Title AS ErrorStatus, LAJL.LeadAdapterAndAccountMapID		
	FROM  [dbo].LeadAdapterAndAccountMap LAAM 
			INNER JOIN [dbo].[LeadAdapterJobLogs] LAJL ON LAJL.LeadAdapterAndAccountMapID = LAAM.LeadAdapterAndAccountMapID
			INNER JOIN [dbo].LeadAdapterTypes LAT ON LAT.LeadAdapterTypeID = LAAM.LeadAdapterTypeID
			INNER JOIN [dbo].[LeadAdapterJobStatus] LAJS ON LAJS.LeadAdapterJobStatusID = LAJL.LeadAdapterJobStatusID
						





