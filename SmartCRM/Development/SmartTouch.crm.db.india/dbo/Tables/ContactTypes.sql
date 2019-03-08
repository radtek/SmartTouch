CREATE TABLE [dbo].[ContactTypes] (
    [ContactTypeID] TINYINT      NOT NULL,
    [Type]          VARCHAR (75) NOT NULL,
    CONSTRAINT [PK_ContactTypes] PRIMARY KEY CLUSTERED ([ContactTypeID] ASC) WITH (FILLFACTOR = 90)
);

