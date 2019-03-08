




CREATE VIEW [dbo].[vCampaignRecipients] with SchemaBinding
as
(
    select CampaignRecipientID,CampaignID,ContactID,CreatedDate,[To],ScheduleTime,SentOn,[GUID],DeliveredOn,DeliveryStatus,LastModifiedOn,OptOutStatus,Remarks
	,ServiceProviderID,HasUnsubscribed,UnsubscribedOn,HasComplained,ComplainedOn,WorkflowID,AccountID from [dbo].[CampaignRecipients_0001]
	UNION ALL
	select CampaignRecipientID,CampaignID,ContactID,CreatedDate,[To],ScheduleTime,SentOn,[GUID],DeliveredOn,DeliveryStatus,LastModifiedOn,OptOutStatus,Remarks
	,ServiceProviderID,HasUnsubscribed,UnsubscribedOn,HasComplained,ComplainedOn,WorkflowID,AccountID from [dbo].[CampaignRecipients_0002]
	UNION ALL
	select CampaignRecipientID,CampaignID,ContactID,CreatedDate,[To],ScheduleTime,SentOn,[GUID],DeliveredOn,DeliveryStatus,LastModifiedOn,OptOutStatus,Remarks
	,ServiceProviderID,HasUnsubscribed,UnsubscribedOn,HasComplained,ComplainedOn,WorkflowID,AccountID from [dbo].[CampaignRecipients_0003]
	UNION ALL
	select CampaignRecipientID,CampaignID,ContactID,CreatedDate,[To],ScheduleTime,SentOn,[GUID],DeliveredOn,DeliveryStatus,LastModifiedOn,OptOutStatus,Remarks
	,ServiceProviderID,HasUnsubscribed,UnsubscribedOn,HasComplained,ComplainedOn,WorkflowID,AccountID from [dbo].[CampaignRecipients_0004]
	UNION ALL
	select CampaignRecipientID,CampaignID,ContactID,CreatedDate,[To],ScheduleTime,SentOn,[GUID],DeliveredOn,DeliveryStatus,LastModifiedOn,OptOutStatus,Remarks
	,ServiceProviderID,HasUnsubscribed,UnsubscribedOn,HasComplained,ComplainedOn,WorkflowID,AccountID from [dbo].[CampaignRecipients_0005]
	UNION ALL
	select CampaignRecipientID,CampaignID,ContactID,CreatedDate,[To],ScheduleTime,SentOn,[GUID],DeliveredOn,DeliveryStatus,LastModifiedOn,OptOutStatus,Remarks
	,ServiceProviderID,HasUnsubscribed,UnsubscribedOn,HasComplained,ComplainedOn,WorkflowID,AccountID from [dbo].[CampaignRecipients_0006]
	
	)



