CREATE TABLE [dbo].[EmailStatistics] (
    [EmailTrackID]     INT           IDENTITY (1, 1) NOT NULL,
    [SentMailDetailID] INT           NOT NULL,
    [ContactID]        INT           NULL,
    [EmailLinkID]      INT           NULL,
    [ActivityType]     TINYINT       NOT NULL,
    [ActivityDate]     DATETIME      NOT NULL,
    [IPAddress]        NVARCHAR (50) NULL,
    PRIMARY KEY CLUSTERED ([EmailTrackID] ASC),
    FOREIGN KEY ([EmailLinkID]) REFERENCES [dbo].[EmailLinks] ([EmailLinkID]),
    FOREIGN KEY ([EmailLinkID]) REFERENCES [dbo].[EmailLinks] ([EmailLinkID])
);

