







CREATE VIEW [dbo].[Get_Import_Data]
AS


	-- SELECT LeadAdapterJobLogID, LeadAdapterAndAccountMapID, StartDate, EndDate, LeadAdapterJobStatusID, Remarks, [FileName], CreatedBy, CreatedDateTime, RecordCreated, RecordUpdated, (RecordCreated + RecordUpdated) TotalRecords,CAST(IsValidated AS BIT ) AS IsValidated ,BadEmailsData,NeverBounceRequestID
	--FROM (
	--	SELECT LeadAdapterJobLogID, LeadAdapterAndAccountMapID, StartDate, EndDate, LeadAdapterJobStatusID, Remarks, [FileName], CreatedBy, CreatedDateTime
	--		,(SELECT count(1) FROM dbo.LeadAdapterJobLogDetails LALD WHERE LALD.LeadAdapterJobLogID	= LAJL.LeadAdapterJobLogID AND LALD.LeadAdapterRecordStatusID = 1) RecordCreated
	--		,(SELECT count(1) FROM dbo.LeadAdapterJobLogDetails LALD WHERE LALD.LeadAdapterJobLogID = LAJL.LeadAdapterJobLogID AND LALD.LeadAdapterRecordStatusID = 3) RecordUpdated,
	--		 (CASE  WHEN  EXISTS (SELECT [EntityID] FROM [NeverBounceMappings] WHERE [EntityID] = LAJL.LeadAdapterJobLogID) THEN 1 ELSE 0 END)  IsValidated,
	--		 (isnull( (SELECT cast((round(CAST((SELECT COUNT(1) FROM [dbo].[NeverBounceEmailStatus] WHERE [NeverBounceRequestID] IN (select [NeverBounceRequestID] from [dbo].[NeverBounceMappings] WHERE [EntityID]  = LAJL.LeadAdapterJobLogID group by [NeverBounceRequestID]) AND EmailStatus = 53 )as float)
	--					/cast((SELECT COUNT(1) FROM [dbo].[NeverBounceEmailStatus]) AS float),2))*100 as nvarchar(max))  +'%'+' | '
	--							+''+ CAST((SELECT COUNT(1) FROM [dbo].[NeverBounceEmailStatus] WHERE [NeverBounceRequestID] IN (select [NeverBounceRequestID] from [dbo].[NeverBounceMappings] WHERE [EntityID] = LAJL.LeadAdapterJobLogID group by [NeverBounceRequestID]) AND EmailStatus = 53 ) AS nvarchar(MAX))
	--							FROM [dbo].[NeverBounceEmailStatus] WHERE [NeverBounceRequestID] IN (SELECT [NeverBounceRequestID] from [dbo].[NeverBounceMappings] WHERE [EntityID]  = LAJL.LeadAdapterJobLogID)
	--							group by 
	--							[NeverBounceRequestID]
	--					 ),'') )BadEmailsData
	--				 ,
	--		 isnull( (select [NeverBounceRequestID] from [dbo].[NeverBounceMappings] WHERE [EntityID] = LAJL.LeadAdapterJobLogID AND NeverBounceEntityType = 1)  ,0) AS NeverBounceRequestID

	--	  FROM dbo.LeadAdapterJobLogs LAJL 
	--	) ImportData

	
SELECT LeadAdapterJobLogID, LeadAdapterAndAccountMapID, StartDate, EndDate, LeadAdapterJobStatusID, Remarks, [FileName], OwnerID AS CreatedBy, CreatedDateTime, RecordCreated, 
		RecordUpdated, (RecordCreated + RecordUpdated) TotalRecords,CAST(IsValidated AS BIT ) AS IsValidated 
		,case (IsValidated )  when  0 then '' else cast(ROUND(cast(bade as float) * 100.0 /cast(case (RecordCreated + RecordUpdated) when 0 then 1 else (RecordCreated + RecordUpdated) end as float) , 1) as nvarchar(max) )  +'%'+' | '+ cast( bade as nvarchar(max) ) end AS  BadEmailsData
		,NeverBounceRequestID,
		case (IsValidated )  when  0 then '' else cast(ROUND(cast(GOOD as float) * 100.0 /cast(case (RecordCreated + RecordUpdated) when 0 then 1 else (RecordCreated + RecordUpdated) end as float) , 1) as nvarchar(max) )  +'%'+' | '+ cast( GOOD as nvarchar(max) ) end AS  GoodEmailsData
			FROM (
				SELECT LeadAdapterJobLogID, LeadAdapterAndAccountMapID, StartDate, EndDate, LeadAdapterJobStatusID, Remarks, [FileName], OwnerID, CreatedDateTime
					,(SELECT count(1) FROM dbo.LeadAdapterJobLogDetails LALD WHERE LALD.LeadAdapterJobLogID	= LAJL.LeadAdapterJobLogID AND LALD.LeadAdapterRecordStatusID = 1) RecordCreated
					,(SELECT count(1) FROM dbo.LeadAdapterJobLogDetails LALD WHERE LALD.LeadAdapterJobLogID = LAJL.LeadAdapterJobLogID AND LALD.LeadAdapterRecordStatusID = 3) RecordUpdated,
					 (CASE  WHEN  EXISTS (SELECT [EntityID] FROM [NeverBounceMappings] WHERE [EntityID] = LAJL.LeadAdapterJobLogID AND [NeverBounceRequestID] IN 
					 (SELECT[NeverBounceRequestID] FROM  [dbo].[NeverBounceRequests] WHERE [ServiceStatus] = 907 )) THEN 1 ELSE 0 END)  IsValidated,
					(SELECT COUNT(1) FROM [dbo].[NeverBounceEmailStatus] WHERE [NeverBounceRequestID] IN (select [NeverBounceRequestID] from [dbo].[NeverBounceMappings]
					 WHERE [EntityID]  = LAJL.LeadAdapterJobLogID group by [NeverBounceRequestID]) AND EmailStatus = 53 ) bade,
					 isnull( (select [NeverBounceRequestID] from [dbo].[NeverBounceMappings] WHERE [EntityID] = LAJL.LeadAdapterJobLogID AND NeverBounceEntityType = 1)  ,0) AS NeverBounceRequestID,
                     (SELECT COUNT(1) FROM [dbo].[NeverBounceEmailStatus] WHERE [NeverBounceRequestID] IN (select [NeverBounceRequestID] from [dbo].[NeverBounceMappings]
					 WHERE [EntityID]  = LAJL.LeadAdapterJobLogID group by [NeverBounceRequestID]) AND EmailStatus ! = 53 )GOOD
				  FROM dbo.LeadAdapterJobLogs LAJL 
		) ImportData
		




