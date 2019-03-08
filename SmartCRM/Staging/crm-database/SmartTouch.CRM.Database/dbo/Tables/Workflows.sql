CREATE TABLE [dbo].[Workflows] (
    [WorkflowID]                    SMALLINT      IDENTITY (1, 1) NOT NULL,
    [WorkflowName]                  VARCHAR (75)  NOT NULL,
    [AccountID]                     INT           NOT NULL,
    [Status]                        SMALLINT      NOT NULL,
    [DeactivatedOn]                 DATETIME      NULL,
    [IsWorkflowAllowedMoreThanOnce] BIT           NULL,
    [AllowParallelWorkflows]        TINYINT       NULL,
    [RemovedWorkflows]              VARCHAR (512) NULL,
    [CreatedBy]                     INT           NOT NULL,
    [CreatedOn]                     DATETIME      NOT NULL,
    [ModifiedBy]                    INT           NULL,
    [ModifiedOn]                    DATETIME      NULL,
    [IsDeleted]                     BIT           NOT NULL,
    [ParentWorkflowID]              INT           DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Workflows] PRIMARY KEY CLUSTERED ([WorkflowID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Workflows_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_Workflows_Statuses] FOREIGN KEY ([Status]) REFERENCES [dbo].[Statuses] ([StatusID]),
    CONSTRAINT [FK_Workflows_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_Workflows_Users1] FOREIGN KEY ([ModifiedBy]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE TRIGGER [dbo].[tr_Workflows_Update] ON dbo.Workflows
FOR UPDATE AS 
	INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],[AccountID],EntityName)
	SELECT WorkflowID, ModifiedBy, 33, CASE WHEN IsDeleted = 0 THEN 3 
										  WHEN IsDeleted = 1 THEN 4 END, GETUTCDATE(), AccountID, WorkflowName FROM Inserted

GO
CREATE TRIGGER [dbo].[tr_Workflows_Insert] ON dbo.Workflows FOR 
INSERT AS 
 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID, EntityName)
	 SELECT WorkflowID, CreatedBy, 33, 1, GETUTCDATE(), AccountID, WorkflowName FROM Inserted

GO
CREATE TRIGGER [dbo].[tr_Workflows_Delete] ON dbo.Workflows FOR DELETE AS 
 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate])
	 SELECT WorkflowID, CreatedBy, 33, 4, GETUTCDATE() FROM Inserted
