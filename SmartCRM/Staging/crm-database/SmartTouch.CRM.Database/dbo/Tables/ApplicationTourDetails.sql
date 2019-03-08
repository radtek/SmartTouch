CREATE TABLE [dbo].[ApplicationTourDetails] (
    [ApplicationTourDetailsID] INT            IDENTITY (1, 1) NOT NULL,
    [DivisionID]               SMALLINT       NOT NULL,
    [SectionID]                SMALLINT       NOT NULL,
    [Title]                    NVARCHAR (600) NULL,
    [Content]                  NVARCHAR (MAX) NULL,
    [Order]                    SMALLINT       NOT NULL,
    [CreatedBy]                INT            NOT NULL,
    [CreatedOn]                DATETIME       NOT NULL,
    [LastUpdatedBy]            INT            NOT NULL,
    [LastUpdatedOn]            DATETIME       NOT NULL,
    [HTMLID]                   NCHAR (100)    NOT NULL,
    [PopUpPlacement]           NCHAR (10)     NOT NULL,
    [Status]                   BIT            NOT NULL,
    CONSTRAINT [PK_ApplicationTourDetails] PRIMARY KEY CLUSTERED ([ApplicationTourDetailsID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ApplicationTourDetails_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_ApplicationTourDetails_Users1] FOREIGN KEY ([LastUpdatedBy]) REFERENCES [dbo].[Users] ([UserID])
);

