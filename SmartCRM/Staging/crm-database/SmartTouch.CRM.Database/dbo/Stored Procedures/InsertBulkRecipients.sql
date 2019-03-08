

CREATE PROCEDURE [dbo].[InsertBulkRecipients] @temporaryRecipients TemporaryRecipients readonly as
BEGIN
	insert into [dbo].TemporaryCampaignRecipients select * from @temporaryRecipients
END

