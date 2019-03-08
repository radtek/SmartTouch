CREATE TABLE [dbo].[Countries] (
    [CountryID]   NCHAR (2)     NOT NULL,
    [CountryName] NVARCHAR (64) NOT NULL,
    CONSTRAINT [PK_Countries] PRIMARY KEY CLUSTERED ([CountryID] ASC) WITH (FILLFACTOR = 90)
);

