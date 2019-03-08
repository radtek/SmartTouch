CREATE TABLE [dbo].[SpamKeyWords] (
    [SpamKeyWordID] INT           IDENTITY (1, 1) NOT NULL,
    [Value]         VARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_SpamKeyWords] PRIMARY KEY CLUSTERED ([SpamKeyWordID] ASC) WITH (FILLFACTOR = 90)
);

