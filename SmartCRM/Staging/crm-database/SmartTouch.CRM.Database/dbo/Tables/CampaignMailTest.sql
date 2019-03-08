CREATE TABLE [dbo].[CampaignMailTest] (
    [CampaignMailTestID] INT              IDENTITY (1, 1) NOT NULL,
    [CampaignID]         INT              NOT NULL,
    [UniqueID]           UNIQUEIDENTIFIER NOT NULL,
    [Status]             INT              NOT NULL,
    [CreatedOn]          DATETIME         NOT NULL,
    [LastUpdatedOn]      DATETIME         NULL,
    [RawData]            NVARCHAR (MAX)   NULL,
    [CreatedBy]          INT              NOT NULL,
    PRIMARY KEY CLUSTERED ([CampaignMailTestID] ASC),
    FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID]),
    FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID])
);

