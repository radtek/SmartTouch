CREATE TABLE [dbo].[OpportunityNoteMap] (
    [OpportunityNoteMapID] INT IDENTITY (1, 1) NOT NULL,
    [OpportunityID]        INT NOT NULL,
    [NoteID]               INT NOT NULL,
    CONSTRAINT [PK_OpportunityNoteMap] PRIMARY KEY CLUSTERED ([OpportunityNoteMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_OpportunityNoteMap_Notes] FOREIGN KEY ([NoteID]) REFERENCES [dbo].[Notes] ([NoteID]),
    CONSTRAINT [FK_OpportunityNoteMap_Opportunities] FOREIGN KEY ([OpportunityID]) REFERENCES [dbo].[Opportunities] ([OpportunityID])
);


GO

CREATE TRIGGER [dbo].[tr_OpportunityNoteMap_Delete] ON [dbo].[OpportunityNoteMap] FOR DELETE AS INSERT INTO OpportunityNoteMap_Audit(OpportunityNoteMapID,OpportunityID,NoteID,AuditAction,AuditStatus) SELECT OpportunityNoteMapID,OpportunityID,NoteID,'D',0 FROM Deleted

UPDATE [dbo].[OpportunityNoteMap_Audit]
SET [AuditStatus] = 0
FROM [dbo].[OpportunityNoteMap_Audit] NA INNER JOIN Deleted D ON NA.[NoteID] = D.NoteID

 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate])
	 SELECT OpportunityID, 1, 16, 4, GETUTCDATE() FROM Deleted
GO

CREATE TRIGGER [dbo].[tr_OpportunityNoteMap_Insert] ON [dbo].[OpportunityNoteMap] FOR INSERT AS INSERT INTO OpportunityNoteMap_Audit(OpportunityNoteMapID,OpportunityID,NoteID,AuditAction,AuditStatus) SELECT OpportunityNoteMapID,OpportunityID,NoteID,'I',1 FROM Inserted

 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate])
	 SELECT OpportunityID, 1, 16, 1, GETUTCDATE() FROM Inserted
GO

CREATE TRIGGER [dbo].[tr_OpportunityNoteMap_Update] ON [dbo].[OpportunityNoteMap] FOR UPDATE AS INSERT INTO OpportunityNoteMap_Audit(OpportunityNoteMapID,OpportunityID,NoteID,AuditAction,AuditStatus) SELECT OpportunityNoteMapID,OpportunityID,NoteID,'U',1 FROM Inserted

 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate])
	 SELECT OpportunityID, 1, 16, 3, GETUTCDATE() FROM Inserted