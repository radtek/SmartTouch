/*Added by Ram on 9th May 2018  for Ticket NEXG-3004 */
 /* created new table and stored pro */
 /* inserting the campaign mail sent status flage */
 CREATE PROC UpdateCampaignSentFlagStatus
 @CampaignId int,
 @CampaignSentStatus bit
 AS 
 BEGIN
 INSERT INTO CampaignMailStatus Values(@CampaignId,@CampaignSentStatus,getdate())
 END
