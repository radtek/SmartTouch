CREATE TABLE [dbo].[WorkflowTimerActions] (
    [WorkflowTimerActionID] INT          IDENTITY (1, 1) NOT NULL,
    [WorkflowActionID]      INT          NOT NULL,
    [TimerType]             TINYINT      NOT NULL,
    [DelayPeriod]           INT          NULL,
    [DelayUnit]             TINYINT      NULL,
    [RunOn]                 TINYINT      NULL,
    [RunAt]                 TIME (7)     NULL,
    [RunType]               TINYINT      NULL,
    [RunOnDate]             DATETIME     NULL,
    [StartDate]             DATETIME     NULL,
    [EndDate]               DATETIME     NULL,
    [RunOnDay]              VARCHAR (15) NULL,
    [DaysOfWeek]            VARCHAR (13) NULL,
    CONSTRAINT [PK_WorkflowTimerActions] PRIMARY KEY CLUSTERED ([WorkflowTimerActionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WorkflowTimerActions_WorkflowActions] FOREIGN KEY ([WorkflowActionID]) REFERENCES [dbo].[WorkflowActions] ([WorkflowActionID])
);

