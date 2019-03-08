CREATE TABLE [dbo].[WorkflowAnalytics] (
    [WorkflowAnalyticsID] INT      IDENTITY (1, 1) NOT NULL,
    [WorkflowID]          INT      NOT NULL,
    [Started]             INT      DEFAULT ((0)) NULL,
    [InProgress]          INT      DEFAULT ((0)) NULL,
    [Completed]           INT      DEFAULT ((0)) NULL,
    [OptedOut]            INT      DEFAULT ((0)) NULL,
    [LastModifiedDate]    DATETIME DEFAULT (getutcdate()) NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_WorkflowAnalytics_WorkflowID_Started_InProgress_Completed_OptedOut]
    ON [dbo].[WorkflowAnalytics]([WorkflowID] ASC)
    INCLUDE([Started], [InProgress], [Completed], [OptedOut]) WITH (FILLFACTOR = 90);

