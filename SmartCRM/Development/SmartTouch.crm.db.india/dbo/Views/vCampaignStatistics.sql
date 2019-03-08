




create VIEW [dbo].[vCampaignStatistics] with SchemaBinding
as
(

	select CampaignTrackerID,ContactID,CampaignID,ActivityType,CampaignLinkID,ActivityDate,LinkIndex,CampaignRecipientID,AccountID from [dbo].[CampaignStatistics_0001]
	UNION ALL
	select CampaignTrackerID,ContactID,CampaignID,ActivityType,CampaignLinkID,ActivityDate,LinkIndex,CampaignRecipientID,AccountID from [dbo].[CampaignStatistics_0002]
	UNION ALL
	select CampaignTrackerID,ContactID,CampaignID,ActivityType,CampaignLinkID,ActivityDate,LinkIndex,CampaignRecipientID,AccountID from [dbo].[CampaignStatistics_0003]
	UNION ALL
	select CampaignTrackerID,ContactID,CampaignID,ActivityType,CampaignLinkID,ActivityDate,LinkIndex,CampaignRecipientID,AccountID from [dbo].[CampaignStatistics_0004]
	UNION ALL
	select CampaignTrackerID,ContactID,CampaignID,ActivityType,CampaignLinkID,ActivityDate,LinkIndex,CampaignRecipientID,AccountID from [dbo].[CampaignStatistics_0005]
	UNION ALL
	select CampaignTrackerID,ContactID,CampaignID,ActivityType,CampaignLinkID,ActivityDate,LinkIndex,CampaignRecipientID,AccountID from [dbo].[CampaignStatistics_0006]

)


