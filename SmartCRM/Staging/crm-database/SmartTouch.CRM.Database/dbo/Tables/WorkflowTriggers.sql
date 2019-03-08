CREATE TABLE [dbo].[WorkflowTriggers] (
    [WorkflowTriggerID]        INT           IDENTITY (1, 1) NOT NULL,
    [TriggerTypeID]            TINYINT       NOT NULL,
    [WorkflowID]               SMALLINT      NOT NULL,
    [CampaignID]               INT           NULL,
    [FormID]                   INT           NULL,
    [LifecycleDropdownValueID] SMALLINT      NULL,
    [TagID]                    INT           NULL,
    [SearchDefinitionID]       INT           NULL,
    [IsStartTrigger]           BIT           NOT NULL,
    [OpportunityStageID]       SMALLINT      NULL,
    [SelectedLinks]            VARCHAR (256) NULL,
    [LeadAdapterID]            INT           NULL,
    [LeadScore]                INT           NULL,
    [WebPage]                  VARCHAR (500) NULL,
    [Duration]                 SMALLINT      NULL,
    [DurationOperator]         TINYINT       NULL,
    [IsAnyWebPage]             BIT           NOT NULL,
    [ActionType]               SMALLINT      NULL,
    [TourType]                 SMALLINT      NULL,
    CONSTRAINT [PK_WorkflowTriggers] PRIMARY KEY CLUSTERED ([WorkflowTriggerID] ASC) WITH (FILLFACTOR = 90),
    FOREIGN KEY ([ActionType]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_WorkflowTriggers_Campaigns] FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID]),
    CONSTRAINT [FK_WorkflowTriggers_DropdownValues] FOREIGN KEY ([LifecycleDropdownValueID]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_WorkflowTriggers_DropdownValues1] FOREIGN KEY ([OpportunityStageID]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_WorkflowTriggers_DropdownValues2] FOREIGN KEY ([TourType]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_WorkflowTriggers_Forms] FOREIGN KEY ([FormID]) REFERENCES [dbo].[Forms] ([FormID]),
    CONSTRAINT [FK_WorkflowTriggers_LeadAdapterAndAccountMap] FOREIGN KEY ([LeadAdapterID]) REFERENCES [dbo].[LeadAdapterAndAccountMap] ([LeadAdapterAndAccountMapID]),
    CONSTRAINT [FK_WorkflowTriggers_SearchDefinitions] FOREIGN KEY ([SearchDefinitionID]) REFERENCES [dbo].[SearchDefinitions] ([SearchDefinitionID]),
    CONSTRAINT [FK_WorkflowTriggers_Tags] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tags] ([TagID]),
    CONSTRAINT [FK_WorkflowTriggers_Workflows] FOREIGN KEY ([WorkflowID]) REFERENCES [dbo].[Workflows] ([WorkflowID])
);


GO
CREATE NONCLUSTERED INDEX [IX_WorkflowTriggers_FormID]
    ON [dbo].[WorkflowTriggers]([FormID] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_WorkflowTriggers_WorkflowID]
    ON [dbo].[WorkflowTriggers]([WorkflowID] ASC)
    INCLUDE([WorkflowTriggerID], [TriggerTypeID], [CampaignID], [FormID], [LifecycleDropdownValueID], [TagID], [SearchDefinitionID], [IsStartTrigger], [OpportunityStageID], [LeadAdapterID], [LeadScore], [WebPage], [Duration], [DurationOperator], [IsAnyWebPage]) WITH (FILLFACTOR = 90);

