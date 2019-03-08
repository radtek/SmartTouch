CREATE TABLE [dbo].[WorkflowTriggerTypes] (
    [TriggerTypeID]   TINYINT      IDENTITY (1, 1) NOT NULL,
    [TriggerName]     VARCHAR (75) NOT NULL,
    [TriggerCategory] TINYINT      NOT NULL,
    CONSTRAINT [PK_WorkflowTriggerTypes] PRIMARY KEY CLUSTERED ([TriggerTypeID] ASC) WITH (FILLFACTOR = 90)
);

