CREATE TABLE [dbo].[ImportContactLogs] (
    [ImportContactLogID] INT              IDENTITY (1, 1) NOT NULL,
    [UserName]           VARCHAR (200)    NULL,
    [ErrorNumber]        INT              NULL,
    [ErrorSeverity]      INT              NULL,
    [ErrorState]         VARCHAR (1000)   NULL,
    [ErrorProcedure]     VARCHAR (500)    NULL,
    [ErrorLine]          INT              NULL,
    [ErrorMessage]       VARCHAR (MAX)    NULL,
    [LogDate]            DATETIME         NULL,
    [Scripts]            NVARCHAR (MAX)   NULL,
    [ReferenceID]        UNIQUEIDENTIFIER NULL,
    [ContactID]          INT              NULL,
    CONSTRAINT [PK_ImportContactLogs] PRIMARY KEY CLUSTERED ([ImportContactLogID] ASC) WITH (FILLFACTOR = 90)
);

