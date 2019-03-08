CREATE TABLE [dbo].[States] (
    [StateID]   VARCHAR (50)  NOT NULL,
    [StateName] NVARCHAR (64) NOT NULL,
    [CountryID] NCHAR (2)     NOT NULL,
    CONSTRAINT [PK_States] PRIMARY KEY CLUSTERED ([StateID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_States_Countries] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[Countries] ([CountryID])
);

