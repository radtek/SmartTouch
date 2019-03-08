CREATE TABLE [dbo].[NoteTagMap] (
    [NoteTagMapID] INT IDENTITY (1, 1) NOT NULL,
    [NoteID]       INT NOT NULL,
    [TagID]        INT NOT NULL,
    CONSTRAINT [PK_NoteTagMap] PRIMARY KEY CLUSTERED ([NoteTagMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_NoteTags_Notes] FOREIGN KEY ([NoteID]) REFERENCES [dbo].[Notes] ([NoteID]),
    CONSTRAINT [FK_NoteTags_Tags] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tags] ([TagID]) ON DELETE CASCADE
);


GO



CREATE TRIGGER [dbo].[tr_NoteTagMap_Delete] ON [dbo].[NoteTagMap] FOR DELETE AS INSERT INTO NoteTagMap_Audit(NoteTagMapID,NoteID,TagID,AuditAction,AuditStatus) SELECT NoteTagMapID,NoteID,TagID,'D',0 FROM Deleted

UPDATE [dbo].[NoteTagMap_Audit]
SET [AuditStatus] = 0
FROM [dbo].[NoteTagMap_Audit] NTMA INNER JOIN Deleted D ON NTMA.NoteTagMapID = D.NoteTagMapID



GO


CREATE TRIGGER [dbo].[tr_NoteTagMap_Insert] ON [dbo].[NoteTagMap] FOR INSERT AS INSERT INTO NoteTagMap_Audit(NoteTagMapID,NoteID,TagID,AuditAction,AuditStatus) SELECT NoteTagMapID,NoteID,TagID,'I',1 FROM Inserted




GO


CREATE TRIGGER [dbo].[tr_NoteTagMap_Update] ON [dbo].[NoteTagMap] FOR UPDATE AS INSERT INTO NoteTagMap_Audit(NoteTagMapID,NoteID,TagID,AuditAction,AuditStatus) SELECT NoteTagMapID,NoteID,TagID,'U',1 FROM Inserted



