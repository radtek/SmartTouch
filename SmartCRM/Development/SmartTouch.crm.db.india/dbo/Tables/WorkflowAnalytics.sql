CREATE TABLE [dbo].[WorkflowAnalytics] (
    [WorkflowAnalyticsID] INT      IDENTITY (1, 1) NOT NULL,
    [WorkflowID]          INT      NOT NULL,
    [Started]             INT      DEFAULT ((0)) NULL,
    [InProgress]          INT      DEFAULT ((0)) NULL,
    [Completed]           INT      DEFAULT ((0)) NULL,
    [OptedOut]            INT      DEFAULT ((0)) NULL,
    [LastModifiedDate]    DATETIME DEFAULT (getutcdate()) NULL
);
go

CREATE NONCLUSTERED INDEX [IX_WorkflowAnalytics_WorkflowID_Started_InProgress_Completed_OptedOut] ON [dbo].[WorkflowAnalytics]
(
	[WorkflowID] ASC
)
INCLUDE ( 	[Started],
	[InProgress],
	[Completed],
	[OptedOut]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO