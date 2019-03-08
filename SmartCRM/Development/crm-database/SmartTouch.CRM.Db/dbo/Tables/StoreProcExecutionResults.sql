CREATE TABLE [dbo].[StoreProcExecutionResults] (
    [ResultID]  BIGINT        IDENTITY (1, 1) NOT NULL,
    [ProcName]  VARCHAR (255) NULL,
    [StartTime] DATETIME      DEFAULT (getdate()) NULL,
    [EndTime]   DATETIME      NULL,
    [TotalTime] FLOAT (53)    NULL,
    [Status]    VARCHAR (1)   DEFAULT ('S') NULL,
    [AccountID] INT           NULL,
    [ParamList] VARCHAR (MAX) NULL
);

