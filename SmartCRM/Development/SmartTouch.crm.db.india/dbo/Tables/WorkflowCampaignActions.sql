CREATE TABLE [dbo].[WorkflowCampaignActions] (
    [WorkflowCampaignActionID] INT IDENTITY (1, 1) NOT NULL,
    [CampaignID]               INT NOT NULL,
    [WorkflowActionID]         INT NOT NULL,
    CONSTRAINT [PK_WorkflowCampaignActions] PRIMARY KEY CLUSTERED ([WorkflowCampaignActionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WorkflowCampaignActions_Campaigns] FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID]),
    CONSTRAINT [FK_WorkflowCampaignActions_WorkflowActions] FOREIGN KEY ([WorkflowActionID]) REFERENCES [dbo].[WorkflowActions] ([WorkflowActionID])
);

