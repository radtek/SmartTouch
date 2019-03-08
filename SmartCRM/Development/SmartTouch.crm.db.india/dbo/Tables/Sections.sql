CREATE TABLE [dbo].[Sections] (
    [SectionID]   SMALLINT      IDENTITY (1, 1) NOT NULL,
    [SectionName] NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Sections] PRIMARY KEY CLUSTERED ([SectionID] ASC) WITH (FILLFACTOR = 90)
);

