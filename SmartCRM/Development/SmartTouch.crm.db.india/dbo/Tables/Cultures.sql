CREATE TABLE [dbo].[Cultures] (
    [CultureID]   TINYINT        NOT NULL,
    [CultureName] NVARCHAR (255) NULL,
    CONSTRAINT [PK_Cultures] PRIMARY KEY CLUSTERED ([CultureID] ASC) WITH (FILLFACTOR = 90)
);

