CREATE TABLE [dbo].[ClassicCampaigns] (
    [ClassicCampaignID] INT            IDENTITY (1, 1) NOT NULL,
    [AID]               DECIMAL (21)   NULL,
    [cid]               DECIMAL (21)   NULL,
    [HTML]              NVARCHAR (MAX) NULL,
    [status]            BIT            DEFAULT ((0)) NULL,
    [Title]             NVARCHAR (255) NULL,
    [AccountID]         INT            NULL,
    [CampaignID]        INT            NULL,
    PRIMARY KEY CLUSTERED ([ClassicCampaignID] ASC) WITH (FILLFACTOR = 90)
);

