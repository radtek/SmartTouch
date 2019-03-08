CREATE TABLE [dbo].[Tours] (
    [TourID]             INT              IDENTITY (1, 1) NOT NULL,
    [TourDetails]        NVARCHAR (1000)  NULL,
    [CommunityID]        SMALLINT         NOT NULL,
    [TourDate]           DATETIME         NOT NULL,
    [TourType]           SMALLINT         NOT NULL,
    [ReminderDate]       DATETIME         NULL,
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
    [SelectAll]          BIT              NULL,
    CONSTRAINT [PK_Tours] PRIMARY KEY CLUSTERED ([TourID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY],
    CONSTRAINT [FK_Tours_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_Tours_DropdownValues] FOREIGN KEY ([TourType]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_Tours_DropdownValues1] FOREIGN KEY ([CommunityID]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_Tours_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_Tours_AccountID]
    ON [dbo].[Tours]([AccountID] ASC)
    INCLUDE([TourID], [CommunityID], [CreatedBy], [CreatedOn]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_Tours_CommunityID]
    ON [dbo].[Tours]([CommunityID] ASC)
    INCLUDE([TourID], [TourDate], [TourType]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_Tours_CreatedBy]
    ON [dbo].[Tours]([CreatedBy] ASC)
    INCLUDE([TourID], [CommunityID], [CreatedOn]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_Tours_CreatedBy_NotificationStatus_RemindbyPopup_ReminderDate]
    ON [dbo].[Tours]([CreatedBy] ASC, [NotificationStatus] ASC, [RemindbyPopup] ASC, [ReminderDate] ASC)
    INCLUDE([TourID], [TourDetails], [CommunityID], [TourDate], [TourType], [CreatedOn], [RemindbyText], [RemindbyEmail], [EmailRequestGuid], [TextRequestGuid], [AccountID], [LastUpdatedBy], [LastUpdatedOn]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO


CREATE NONCLUSTERED INDEX [IX_Tours_TourType_CreatedBy]
    ON [dbo].[Tours]([TourType] ASC, [CreatedBy] ASC)
    INCLUDE([TourID], [CommunityID], [CreatedOn]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO


CREATE TRIGGER [dbo].[tr_Tours_Delete] ON [dbo].[Tours]
FOR DELETE AS 
	INSERT INTO Tours_Audit(TourID,TourDetails,CommunityID,TourDate,TourType,ReminderDate,CreatedBy,CreatedOn,NotificationStatus, RemindbyText, RemindbyEmail, RemindbyPopup, EmailRequestGuid, TextRequestGuid,AccountID,LastUpdatedBy,LastUpdatedOn,AuditAction,AuditStatus) 
	SELECT TourID,TourDetails,CommunityID,TourDate,TourType,ReminderDate,CreatedBy,CreatedOn, NotificationStatus, RemindbyText, RemindbyEmail, RemindbyPopup, EmailRequestGuid, TextRequestGuid, AccountID,LastUpdatedBy,LastUpdatedOn,'D',0 FROM Deleted

	INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate], AccountID, EntityName)
		SELECT TourID, CreatedBy,7, 4, GETUTCDATE(), AccountID, TourDetails FROM Deleted

	UPDATE [dbo].[Tours_Audit]
		SET [AuditStatus] = 0
		FROM [dbo].[Tours_Audit] TA INNER JOIN Deleted D on TA.[TourID] = D.TourID

		
	

GO




CREATE TRIGGER [dbo].[tr_Tours_Insert] ON [dbo].[Tours] FOR INSERT AS 
INSERT INTO Tours_Audit(TourID,TourDetails,CommunityID,TourDate,TourType,ReminderDate,CreatedBy,CreatedOn,NotificationStatus, RemindbyText, RemindbyEmail, RemindbyPopup, EmailRequestGuid, TextRequestGuid,AccountID,LastUpdatedBy,LastUpdatedOn, AuditAction,AuditStatus) 
SELECT TourID,TourDetails,CommunityID,TourDate,TourType,ReminderDate,CreatedBy,CreatedOn, NotificationStatus, RemindbyText, RemindbyEmail, RemindbyPopup, EmailRequestGuid, TextRequestGuid, AccountID,LastUpdatedBy,LastUpdatedOn,'I',1 FROM Inserted



INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate], AccountID, EntityName)
	 SELECT TourID, CreatedBy,7, 1, GETUTCDATE(), AccountID, TourDetails FROM Inserted


GO






CREATE TRIGGER [dbo].[tr_Tours_Update] ON [dbo].[Tours] FOR UPDATE AS 
INSERT INTO Tours_Audit(TourID,TourDetails,CommunityID,TourDate,TourType,ReminderDate,CreatedBy,CreatedOn,NotificationStatus, RemindbyText, RemindbyEmail, RemindbyPopup, EmailRequestGuid, TextRequestGuid,AccountID,LastUpdatedBy,LastUpdatedOn,AuditAction,AuditStatus)
 SELECT TourID,TourDetails,CommunityID,TourDate,TourType,ReminderDate,CreatedBy,CreatedOn,NotificationStatus, RemindbyText, RemindbyEmail, RemindbyPopup, EmailRequestGuid, TextRequestGuid,AccountID,LastUpdatedBy,LastUpdatedOn,'U',1 FROM Inserted


 
INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate], AccountID, EntityName)
	 SELECT TourID, CreatedBy,7, 3, GETUTCDATE(), AccountID, TourDetails FROM Inserted

