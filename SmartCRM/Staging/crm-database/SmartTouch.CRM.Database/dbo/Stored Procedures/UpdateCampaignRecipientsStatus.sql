
CREATE PROCEDURE [dbo].[UpdateCampaignRecipientsStatus]
@campaignRecipientIDs [dbo].[Contact_List] readonly --use this table for Contact Receipient Ids
,@remarks VARCHAR(500)
,@deliveredOn DateTime
,@sentOn DateTime
,@deliveryStatus SmallInt
as
begin

UPDATE CampaignRecipients
SET Remarks = @remarks, DeliveredOn = @deliveredOn, SentOn = @sentOn, DeliveryStatus = @deliveryStatus
WHERE CampaignRecipientID IN (SELECT ContactID FROM @campaignRecipientIDs)

END



