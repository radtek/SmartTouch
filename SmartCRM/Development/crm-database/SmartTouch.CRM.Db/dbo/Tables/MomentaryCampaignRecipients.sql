CREATE TABLE [dbo].[MomentaryCampaignRecipients] (
    [CampaignID]           INT    NOT NULL,
    [ContactID]            INT    NOT NULL,
    [MomentaryRecipientID] BIGINT IDENTITY (1, 1) NOT NULL,
    CONSTRAINT [pk_MomentaryCampaignRecipients_MomentaryRecipientID] PRIMARY KEY CLUSTERED ([MomentaryRecipientID] ASC) WITH (FILLFACTOR = 90)
);

