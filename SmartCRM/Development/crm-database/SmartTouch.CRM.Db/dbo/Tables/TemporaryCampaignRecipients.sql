CREATE TABLE [dbo].[TemporaryCampaignRecipients] (
    [CampaignID]           INT    NOT NULL,
    [ContactID]            INT    NOT NULL,
    [TemporaryRecipientID] BIGINT IDENTITY (1, 1) NOT NULL,
    CONSTRAINT [pk_TemporaryCampaignRecipients_TemporaryRecipientID] PRIMARY KEY CLUSTERED ([TemporaryRecipientID] ASC) WITH (FILLFACTOR = 90)
);
GO
