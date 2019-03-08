CREATE TABLE [dbo].[SuppressedEmails] (
    [SuppressedEmailID] INT           IDENTITY (1, 1) NOT NULL,
    [Email]             VARCHAR (300) NOT NULL,
    [AccountID]         INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([SuppressedEmailID] ASC) WITH (FILLFACTOR = 90)
);

