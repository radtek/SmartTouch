CREATE TABLE [dbo].[EmailLinks] (
    [EmailLinkID]      INT            IDENTITY (1, 1) NOT NULL,
    [SentMailDetailID] INT            NOT NULL,
    [LinkURL]          NVARCHAR (MAX) NOT NULL,
    [LinkIndex]        TINYINT        NOT NULL,
    [CreatedOn]        DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([EmailLinkID] ASC)
);

