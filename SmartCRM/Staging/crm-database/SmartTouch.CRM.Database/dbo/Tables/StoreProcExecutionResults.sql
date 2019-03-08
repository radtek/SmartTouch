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


GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20170713-115824]
    ON [dbo].[StoreProcExecutionResults]([EndTime] ASC, [TotalTime] ASC, [Status] ASC)
    INCLUDE([ResultID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_StoreProcExecutionResults_ResultID]
    ON [dbo].[StoreProcExecutionResults]([ResultID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

