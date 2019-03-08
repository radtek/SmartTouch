CREATE TABLE [dbo].[ContactSource] (
    [SourceID]    TINYINT     NOT NULL,
    [FirstSource] NCHAR (100) NOT NULL,
    CONSTRAINT [PK_ContactSource] PRIMARY KEY CLUSTERED ([SourceID] ASC) WITH (FILLFACTOR = 90)
);

