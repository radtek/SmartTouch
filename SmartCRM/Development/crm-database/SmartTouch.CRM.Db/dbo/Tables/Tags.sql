CREATE TABLE [dbo].[Tags] (
    [TagID]       INT            IDENTITY (1, 1) NOT NULL,
    [TagName]     NVARCHAR (75)  NOT NULL,
    [Description] NVARCHAR (100) NULL,
    [AccountID]   INT            NOT NULL,
    [CreatedBy]   INT            NULL,
    [Count]       INT            NULL DEFAULT 0,
    [IsDeleted]   BIT            CONSTRAINT [IsDeleted_Default] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED ([TagID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY]
);


GO
CREATE NONCLUSTERED INDEX [_dta_index_Tags_6_522484940__K4_K1_2_7]
    ON [dbo].[Tags]([AccountID] ASC, [TagID] ASC)
    INCLUDE([TagName], [IsDeleted]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Tags_AccountID_IsDeleted]
    ON [dbo].[Tags]([AccountID] ASC, [IsDeleted] ASC) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_Tags_IsDeleted]
    ON [dbo].[Tags]([IsDeleted] ASC)
    INCLUDE([TagID]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_Tags_TagName_AccountID]
    ON [dbo].[Tags]([TagName] ASC, [AccountID] ASC) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [Tags_AccountID]
    ON [dbo].[Tags]([AccountID] ASC)
    INCLUDE([TagID], [TagName], [Description], [CreatedBy], [IsDeleted]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE TRIGGER [dbo].[tr_Tags_Delete] ON [dbo].[Tags] FOR DELETE AS INSERT INTO Tags_Audit(TagID,TagName,Description,Count,AccountID,IsDeleted, AuditAction,AuditStatus) SELECT TagID,TagName,Description,Count,AccountID, IsDeleted, 'D',0 FROM Deleted

UPDATE [dbo].[Tags_Audit]
SET [AuditStatus] = 0
FROM [dbo].[Tags_Audit] TA INNER JOIN Deleted D ON TA.[TagID] = D.TagID

 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID,EntityName)
	 SELECT TagID, CreatedBy, 9, 4, GETUTCDATE(),AccountID,TagName FROM Inserted


GO
CREATE TRIGGER [dbo].[tr_Tags_Insert] ON [dbo].[Tags] FOR INSERT AS INSERT INTO Tags_Audit(TagID,TagName,Description,Count,[AccountID], IsDeleted, AuditAction,AuditStatus) 
SELECT TagID,TagName,Description,Count,AccountID, IsDeleted, 'I',1 FROM Inserted

 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID,EntityName)
	 SELECT TagID, CreatedBy, 9, 1, GETUTCDATE(), AccountID,TagName FROM Inserted


GO
CREATE TRIGGER [dbo].[tr_Tags_Update] ON [dbo].[Tags] FOR UPDATE AS INSERT INTO Tags_Audit(TagID,TagName,Description,Count, AccountID,IsDeleted,AuditAction,AuditStatus) SELECT TagID,TagName,Description,Count,AccountID, IsDeleted, 'U',1 FROM Inserted

 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID,EntityName)
	 SELECT TagID, CreatedBy, 9, 3, GETUTCDATE(), AccountID,TagName FROM Inserted

