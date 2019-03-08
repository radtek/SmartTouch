CREATE TABLE [dbo].[MarketingMessageAccountMap] (
    [MarketingMessageAccountMapID] INT IDENTITY (1, 1) NOT NULL,
    [MarketingMessageID]           INT NOT NULL,
    [AccountID]                    INT NOT NULL,
    CONSTRAINT [PK_MarketingMessageAccountMap] PRIMARY KEY CLUSTERED ([MarketingMessageAccountMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_MarketingMessageAccountMap_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_MarketingMessageAccountMap_MarketingMessages] FOREIGN KEY ([MarketingMessageID]) REFERENCES [dbo].[MarketingMessages] ([MarketingMessageID])
);

