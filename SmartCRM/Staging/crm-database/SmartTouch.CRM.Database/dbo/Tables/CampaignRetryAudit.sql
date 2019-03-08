CREATE TABLE [dbo].[CampaignRetryAudit] (
    [CampaignRetryAuditID] INT             IDENTITY (1, 1) NOT NULL,
    [CampaignID]           INT             NOT NULL,
    [RetriedOn]            DATETIME        NOT NULL,
    [CampaignStatus]       SMALLINT        NOT NULL,
    [Remarks]              NVARCHAR (4000) NULL,
    CONSTRAINT [PK_CampaignRetryAudit] PRIMARY KEY CLUSTERED ([CampaignRetryAuditID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CampaignRetryAudit_Campaigns] FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID]),
    CONSTRAINT [FK_CampaignRetryAudit_Statuses] FOREIGN KEY ([CampaignStatus]) REFERENCES [dbo].[Statuses] ([StatusID])
);

