CREATE TABLE [dbo].[DebugLogs] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [AccountId] INT           NULL,
    [Remarks]   VARCHAR (MAX) NULL,
    [CreatedOn] DATETIME      NULL
);
GO

