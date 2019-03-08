CREATE TABLE [dbo].[WorkflowCampaignActionLinks] (
    [WorkflowCampaignLinkID] INT IDENTITY (1, 1) NOT NULL,
    [LinkID]                 INT NOT NULL,
    [ParentWorkflowActionID] INT NOT NULL,
    [LinkActionID]           INT NOT NULL,
    CONSTRAINT [PK_WorkflowCampaignActionLinks] PRIMARY KEY CLUSTERED ([WorkflowCampaignLinkID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WorkflowCampaignActionLinks_CampaignLinks] FOREIGN KEY ([LinkID]) REFERENCES [dbo].[CampaignLinks] ([CampaignLinkID]),
    CONSTRAINT [FK_WorkflowCampaignActionLinks_WorkflowActions] FOREIGN KEY ([LinkActionID]) REFERENCES [dbo].[WorkflowActions] ([WorkflowActionID]),
    CONSTRAINT [FK_WorkflowCampaignActionLinks_WorkflowCampaignActions1] FOREIGN KEY ([ParentWorkflowActionID]) REFERENCES [dbo].[WorkflowCampaignActions] ([WorkflowCampaignActionID])
);

GO 

Create NonClustered Index IX_WorkflowCampaignActionLinks_missing_117 On [dbo].[WorkflowCampaignActionLinks] ([LinkID]);
GO