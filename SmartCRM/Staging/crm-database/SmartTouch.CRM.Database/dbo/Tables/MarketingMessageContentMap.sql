CREATE TABLE [dbo].[MarketingMessageContentMap] (
    [MarketingMessageContentMapID] INT            IDENTITY (1, 1) NOT NULL,
    [MarketingMessageID]           INT            NOT NULL,
    [Content]                      NVARCHAR (MAX) NULL,
    [Subject]                      VARCHAR (128)  NOT NULL,
    [Icon]                         VARCHAR (100)  NOT NULL,
    CONSTRAINT [PK_MarketingMessageContentMap] PRIMARY KEY CLUSTERED ([MarketingMessageContentMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_MarketingMessageContentMap_MarketingMessages] FOREIGN KEY ([MarketingMessageID]) REFERENCES [dbo].[MarketingMessages] ([MarketingMessageID])
);

