CREATE TABLE [dbo].[SeedList] (
    [SeedID]      INT           IDENTITY (1, 1) NOT NULL,
    [Email]       VARCHAR (300) NOT NULL,
    [CreatedBy]   INT           NOT NULL,
    [CreatedDate] DATETIME      NOT NULL,
    CONSTRAINT [PK_SeedList] PRIMARY KEY CLUSTERED ([SeedID] ASC) WITH (FILLFACTOR = 90)
);

