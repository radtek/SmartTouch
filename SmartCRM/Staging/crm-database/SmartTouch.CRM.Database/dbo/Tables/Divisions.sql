CREATE TABLE [dbo].[Divisions] (
    [DivisionID]   SMALLINT      IDENTITY (1, 1) NOT NULL,
    [DivisionName] NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Divisions] PRIMARY KEY CLUSTERED ([DivisionID] ASC) WITH (FILLFACTOR = 90)
);

