CREATE TABLE [dbo].[Actions] (
    [ActionID]           INT              IDENTITY (1, 1) NOT NULL,
    [ActionDetails]      NVARCHAR (1000)  NOT NULL,
    [RemindOn]           DATETIME         NULL,
    [CreatedBy]          INT              NOT NULL,
    [CreatedOn]          DATETIME         NOT NULL,
    [NotificationStatus] TINYINT          NULL,
    [RemindbyText]       BIT              NULL,
    [RemindbyEmail]      BIT              NULL,
    [RemindbyPopup]      BIT              NULL,
    [EmailRequestGuid]   UNIQUEIDENTIFIER NULL,
    [TextRequestGuid]    UNIQUEIDENTIFIER NULL,
    [AccountID]          INT              NULL,
    [LastUpdatedBy]      INT              NULL,
    [LastUpdatedOn]      DATETIME         NULL,
    [ActionType]         SMALLINT         NULL,
    [ActionDate]         DATETIME         NULL,
    [ActionStartTime]    DATETIME         NULL,
    [ActionEndTime]      DATETIME         NULL,
    [SelectAll]          BIT              NULL,
	[MailBulkId]     INT                   null,
    CONSTRAINT [PK_Actions] PRIMARY KEY CLUSTERED ([ActionID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY],
    CONSTRAINT [FK_Actions_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_Actions_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID])
);


GO




CREATE NONCLUSTERED INDEX [IX_Actions_CreatedBy_NotificationStatus_RemindbyPopup_RemindOn]
    ON [dbo].[Actions]([CreatedBy] ASC, [NotificationStatus] ASC, [RemindbyPopup] ASC, [RemindOn] ASC) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_Actions_CreatedBy_RemindOn]
    ON [dbo].[Actions]([CreatedBy] ASC, [RemindOn] ASC)
    INCLUDE([ActionDetails]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO

Create NonClustered Index IX_Actions_missing_207 On [dbo].[Actions] ([AccountID]) Include ([ActionID], [ActionDetails], [CreatedOn], [ActionType], [ActionDate], [ActionEndTime]);
GO


CREATE TRIGGER [dbo].[tr_Actions_Delete] ON [dbo].[Actions] FOR DELETE AS 
INSERT INTO Actions_Audit(ActionID,ActionDetails,RemindOn,CreatedBy,CreatedOn,  NotificationStatus, RemindbyText, RemindbyEmail, RemindbyPopup, EmailRequestGuid, TextRequestGuid, AuditAction,AuditStatus,AccountID, LastUpdatedBy, LastUpdatedOn,ActionType ,ActionDate ,ActionStartTime ,ActionEndTime)
 SELECT ActionID,ActionDetails,RemindOn,CreatedBy,CreatedOn, NotificationStatus, RemindbyText, RemindbyEmail, RemindbyPopup, EmailRequestGuid, TextRequestGuid, 'D',0, AccountID, LastUpdatedBy, LastUpdatedOn ,ActionType ,ActionDate ,ActionStartTime ,ActionEndTime FROM deleted


INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate], AccountID, EntityName)
	 SELECT ActionID, CreatedBy,5, 4, GETUTCDATE(), AccountID, ActionDetails FROM deleted

UPDATE [dbo].[Actions_Audit]
SET [AuditStatus] = 0
WHERE ActionID IN (SELECT ActionID FROM deleted)




GO



CREATE TRIGGER [dbo].[tr_Actions_Insert] ON [dbo].[Actions] FOR 
INSERT AS INSERT INTO Actions_Audit(ActionID,ActionDetails,RemindOn,CreatedBy,CreatedOn, NotificationStatus, RemindbyText, RemindbyEmail, RemindbyPopup, EmailRequestGuid, TextRequestGuid, AuditAction,AuditStatus, AccountID, LastUpdatedBy, LastUpdatedOn,ActionType ,ActionDate ,ActionStartTime ,ActionEndTime)
 SELECT ActionID,ActionDetails,RemindOn,CreatedBy,CreatedOn, NotificationStatus, RemindbyText, RemindbyEmail, RemindbyPopup, EmailRequestGuid, TextRequestGuid, 'I',1, AccountID, LastUpdatedBy, LastUpdatedOn,ActionType ,ActionDate ,ActionStartTime ,ActionEndTime  FROM Inserted


 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID,EntityName)
	 SELECT I.ActionID, I.CreatedBy,5, 1, GETUTCDATE(), I.AccountID, I.ActionDetails FROM Inserted I
	 





GO

CREATE TRIGGER [dbo].[tr_Actions_Update] ON [dbo].[Actions] FOR UPDATE AS 
INSERT INTO Actions_Audit(ActionID,ActionDetails,RemindOn,CreatedBy,CreatedOn,  NotificationStatus, RemindbyText, RemindbyEmail, RemindbyPopup, EmailRequestGuid, TextRequestGuid, AuditAction,AuditStatus, AccountID, LastUpdatedBy, LastUpdatedOn,ActionType ,ActionDate ,ActionStartTime ,ActionEndTime)
 SELECT ActionID,ActionDetails,RemindOn,CreatedBy,CreatedOn, NotificationStatus, RemindbyText, RemindbyEmail, RemindbyPopup, EmailRequestGuid, TextRequestGuid, 'U', 1, AccountID, LastUpdatedBy, LastUpdatedOn,ActionType ,ActionDate ,ActionStartTime ,ActionEndTime FROM Inserted
 
 
 
 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],[AccountID], [EntityName])
	 SELECT ActionID, CreatedBy,5, 3, GETUTCDATE(), AccountID, ActionDetails FROM Inserted



