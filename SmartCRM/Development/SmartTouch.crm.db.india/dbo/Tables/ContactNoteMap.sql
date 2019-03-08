CREATE TABLE [dbo].[ContactNoteMap] (
    [ContactNoteMapID] INT IDENTITY (1, 1) NOT NULL,
    [NoteID]           INT NOT NULL,
    [ContactID]        INT NOT NULL,
    CONSTRAINT [PK_ContactNoteMap] PRIMARY KEY CLUSTERED ([ContactNoteMapID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY],
    CONSTRAINT [FK_ContactNotes_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_ContactNotes_Notes] FOREIGN KEY ([NoteID]) REFERENCES [dbo].[Notes] ([NoteID])
);


GO

CREATE NONCLUSTERED INDEX [IX_ContactNoteMap_NoteID]
    ON [dbo].[ContactNoteMap]([NoteID] ASC) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO

Create NonClustered Index IX_ContactNoteMap_missing_17 On [dbo].[ContactNoteMap] ([ContactID]) Include ([NoteID]);

GO

Create NonClustered Index IX_ContactNoteMap_missing_69 On [dbo].[ContactNoteMap] ([ContactID]);
GO





CREATE TRIGGER [dbo].[tr_ContactNoteMap_Delete] ON [dbo].[ContactNoteMap] FOR DELETE AS 
--INSERT INTO [dbo].ContactNoteMap_Audit(ContactNoteMapID,NoteID,ContactID,AuditAction,AuditStatus) SELECT ContactNoteMapID,NoteID,ContactID,'D',0 FROM Deleted
	
	UPDATE [dbo].ContactNoteMap_Audit
		SET AuditStatus = 0
		FROM [dbo].ContactNoteMap_Audit CNMA INNER JOIN Deleted D ON CNMA.[ContactNoteMapID] = D.[ContactNoteMapID]










GO




CREATE TRIGGER [dbo].[tr_ContactNoteMap_Insert] ON [dbo].[ContactNoteMap] FOR INSERT AS INSERT INTO ContactNoteMap_Audit(ContactNoteMapID,NoteID,ContactID,AuditAction,AuditStatus) SELECT ContactNoteMapID,NoteID,ContactID,'I',1 FROM Inserted






GO




CREATE TRIGGER [dbo].[tr_ContactNoteMap_Update] ON [dbo].[ContactNoteMap] FOR UPDATE AS INSERT INTO ContactNoteMap_Audit(ContactNoteMapID,NoteID,ContactID,AuditAction,AuditStatus) SELECT ContactNoteMapID,NoteID,ContactID,'U',1 FROM Inserted






