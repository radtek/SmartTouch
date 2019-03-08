CREATE TABLE [dbo].[LogsTable] (
    [ID]       INT            IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR (MAX) NULL,
    [step]     INT            NULL,
    [JobID]    INT            NULL,
    [Dates]    DATETIME       NULL,
    [Counters] INT            NULL
);

