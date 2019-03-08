CREATE TABLE [dbo].[Campaigns] (
    [CampaignID]                INT            IDENTITY (1, 1) NOT NULL,
    [CampaignStatusID]          SMALLINT       NOT NULL,
    [CampaignTemplateID]        INT            NOT NULL,
    [Name]                      NVARCHAR (125) NOT NULL,
    [Subject]                   NVARCHAR (75)  NULL,
    [HTMLContent]               NVARCHAR (MAX) NULL,
    [ScheduleTime]              DATETIME       NULL,
    [From]                      NVARCHAR (256) NULL,
    [CreatedBy]                 INT            NOT NULL,
    [CreatedDate]               DATETIME       NOT NULL,
    [ServiceProviderID]         INT            NULL,
    [AccountID]                 INT            NOT NULL,
    [IsDeleted]                 BIT            NOT NULL,
    [SenderName]                NVARCHAR (150) NOT NULL,
    [ServiceProviderCampaignID] NVARCHAR (150) NULL,
    [Remarks]                   NVARCHAR (4000) NULL,
    [LastViewedState]           CHAR (1)       NULL,
    [LastUpdatedBy]             INT            NULL,
    [LastUpdatedOn]             DATETIME       NULL,
    [IsLinkedToWorkflows]       BIT            NOT NULL,
    [ProcessedDate]             DATETIME       NULL,
    [TagRecipients]             SMALLINT       NULL,
    [SSRecipients]              SMALLINT       NULL,
    [IsRecipientsProcessed]     BIT            DEFAULT ((0)) NULL,
    [CampaignTypeID]            TINYINT        CONSTRAINT [DF_Campaigns_CampaignTypeID] DEFAULT ((131)) NULL,
	[IncludePlainText]          bit            NOT NULL DEFAULT ('0'),
	[HasDisclaimer]             bit            NULL CONSTRAINT [DF_Campaigns_HasDisclaimer]  DEFAULT ((0))  ,
    CONSTRAINT [PK_Campaigns] PRIMARY KEY CLUSTERED ([CampaignID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Campaigns_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_Campaigns_Campaigns] FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID]),
    CONSTRAINT [FK_Campaigns_CampaignTemplates] FOREIGN KEY ([CampaignTemplateID]) REFERENCES [dbo].[CampaignTemplates] ([CampaignTemplateID]),
    CONSTRAINT [FK_Campaigns_ServiceProviders] FOREIGN KEY ([ServiceProviderID]) REFERENCES [dbo].[ServiceProviders] ([ServiceProviderID]),
    CONSTRAINT [FK_Campaigns_Statuses] FOREIGN KEY ([CampaignStatusID]) REFERENCES [dbo].[Statuses] ([StatusID]),
    CONSTRAINT [FK_Campaigns_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [_dta_index_Campaigns_6_84911374__K13_K1]
    ON [dbo].[Campaigns]([IsDeleted] ASC, [CampaignID] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Campaigns_AccountID_CampaignID_CampaignStatusID]
    ON [dbo].[Campaigns]([AccountID] ASC)
    INCLUDE([CampaignID], [CampaignStatusID], [CampaignTemplateID], [Name], [Subject], [HTMLContent], [ScheduleTime], [From], [CreatedBy], [CreatedDate], [ServiceProviderID], [IsDeleted], [SenderName], [ServiceProviderCampaignID], [Remarks], [LastViewedState], [LastUpdatedBy], [LastUpdatedOn], [IsLinkedToWorkflows], [ProcessedDate]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Campaigns_AccountID_CampaignStatusID]
    ON [dbo].[Campaigns]([AccountID] ASC, [CampaignStatusID] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Campaigns_AccountID_CampaignStatusID_Name]
    ON [dbo].[Campaigns]([AccountID] ASC)
    INCLUDE([CampaignID], [CampaignStatusID], [CampaignTemplateID], [Name], [Subject], [HTMLContent], [ScheduleTime], [From], [CreatedBy], [CreatedDate], [ServiceProviderID], [IsDeleted], [SenderName], [ServiceProviderCampaignID], [Remarks], [LastViewedState], [LastUpdatedBy], [LastUpdatedOn], [IsLinkedToWorkflows], [ProcessedDate], [TagRecipients], [SSRecipients]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Campaigns_AccountID_IsDeleted]
    ON [dbo].[Campaigns]([AccountID] ASC, [IsDeleted] ASC)
    INCLUDE([CampaignID], [CampaignStatusID], [CampaignTemplateID], [Name], [Subject], [ScheduleTime], [From], [CreatedBy], [CreatedDate], [ServiceProviderID], [LastUpdatedOn], [ProcessedDate]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Campaigns_AccountID_IsDeleted_CampaignStatusID]
    ON [dbo].[Campaigns]([AccountID] ASC, [IsDeleted] ASC, [CampaignStatusID] ASC)
    INCLUDE([CampaignID], [Name], [CreatedDate], [LastUpdatedOn]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Campaigns_AccountID_IsDeleted_CampaignStatusID_Name]
    ON [dbo].[Campaigns]([AccountID] ASC, [IsDeleted] ASC, [CampaignStatusID] ASC, [Name] ASC)
    INCLUDE([CampaignID], [ScheduleTime], [LastUpdatedOn], [IsLinkedToWorkflows], [ProcessedDate], [CampaignTypeID]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Campaigns_CampaignStatusID_AccountID]
    ON [dbo].[Campaigns]([CampaignStatusID] ASC, [AccountID] ASC)
    INCLUDE([CampaignID], [CampaignTemplateID], [Name], [Subject], [HTMLContent], [ScheduleTime], [From], [CreatedBy], [CreatedDate], [ServiceProviderID], [IsDeleted], [SenderName], [ServiceProviderCampaignID], [Remarks], [LastViewedState], [LastUpdatedBy], [LastUpdatedOn], [IsLinkedToWorkflows], [ProcessedDate], [TagRecipients], [SSRecipients]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Campaigns_CampaignStatusID_AccountID_IsDeleted_IsLinkedToWorkflows]
    ON [dbo].[Campaigns]([CampaignStatusID] ASC, [AccountID] ASC, [IsDeleted] ASC, [IsLinkedToWorkflows] ASC)
    INCLUDE([CampaignID], [Name], [CreatedDate], [LastUpdatedOn]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Campaigns_CampaignStatusID_AccountID_LastUpdatedOn]
    ON [dbo].[Campaigns]([CampaignStatusID] ASC, [AccountID] ASC, [LastUpdatedOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Campaigns_CampaignStatusID_ScheduleTime]
    ON [dbo].[Campaigns]([CampaignStatusID] ASC, [ScheduleTime] ASC)
    INCLUDE([CampaignID], [CampaignTemplateID], [Name], [Subject], [ServiceProviderID], [AccountID], [IsDeleted], [HTMLContent], [From], [CreatedBy], [CreatedDate]) WITH (FILLFACTOR = 90);


GO


CREATE NONCLUSTERED INDEX [IX_Campaigns_Name_AccountID]
    ON [dbo].[Campaigns]([Name] ASC, [AccountID] ASC)
    INCLUDE([CampaignID]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Campaigns_ProcessedDate]
    ON [dbo].[Campaigns]([ProcessedDate] ASC) WITH (FILLFACTOR = 90);


GO

Create NonClustered Index IX_Campaigns_missing_19 On [dbo].[Campaigns] ([IsDeleted], [IsRecipientsProcessed],[CampaignStatusID], [ScheduleTime]) Include ([CampaignID], [CreatedDate]);
GO

Create NonClustered Index IX_Campaigns_missing_21 On [dbo].[Campaigns] ([CampaignStatusID], [IsDeleted], [IsRecipientsProcessed],[ProcessedDate]) Include ([CampaignID]);
GO


CREATE TRIGGER [dbo].[tr_Campaigns_Update] ON [dbo].[Campaigns]
FOR UPDATE AS 
	INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],[AccountID],EntityName)
	SELECT CampaignID, LastUpdatedBy, 4, CASE WHEN IsDeleted = 0 THEN 3 
										  WHEN IsDeleted = 1 THEN 4 END, GETUTCDATE(), AccountID, Name FROM Inserted


GO
CREATE TRIGGER [dbo].[tr_Campaigns_Delete] ON [dbo].[Campaigns] FOR DELETE AS 
 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate])
	 SELECT CampaignID, CreatedBy, 4, 4, GETUTCDATE() FROM Inserted


GO
CREATE TRIGGER [dbo].[tr_Campaigns_Insert] ON [dbo].[Campaigns] FOR 
INSERT AS 
 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID, EntityName)
	 SELECT CampaignID, CreatedBy, 4, 1, GETUTCDATE(), AccountID, Name FROM Inserted

Create NonClustered Index IX_Campaigns_ServiceProviderID_AccountID_IsDeleted_ScheduleTim_CampaignID  On [dbo].[Campaigns] ([ServiceProviderID], [AccountID], [IsDeleted],[ScheduleTime]) Include ([CampaignID], [CampaignStatusID], [Name], [CreatedBy]);
GO