CREATE TABLE [dbo].[Notes] (
    [NoteID]              INT            IDENTITY (1, 1) NOT NULL,
    [NoteDetails]         NVARCHAR (MAX) NOT NULL,
    [CreatedBy]           INT            NOT NULL,
    [CreatedOn]           DATETIME       NOT NULL,
    [AccountID]           INT            NULL,
    [SelectAll]           BIT            CONSTRAINT [df_Notes_default_0] DEFAULT ((0)) NOT NULL,
    [AddToContactSummary] BIT            CONSTRAINT [DF_AddToContactSummary] DEFAULT ((0)) NOT NULL,
    [NoteCategory]        SMALLINT       DEFAULT ((13216)) NOT NULL,
    CONSTRAINT [PK_Notes] PRIMARY KEY NONCLUSTERED ([NoteID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON) ON [PRIMARY],
    CONSTRAINT [FK_Notes_DropdownValues] FOREIGN KEY ([NoteCategory]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID])
) ON [AccountId_Scheme_UserActivityLogs] ([AccountID]);


GO
CREATE NONCLUSTERED INDEX [IX_Notes_missing_185]
    ON [dbo].[Notes]([CreatedBy] ASC, [AddToContactSummary] ASC)
    INCLUDE([NoteID], [NoteDetails], [CreatedOn], [NoteCategory]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_UserActivityLogs] ([AccountID]);


GO



CREATE TRIGGER [dbo].[tr_Notes_Insert] ON [dbo].[Notes] 
FOR INSERT AS INSERT INTO Notes_Audit(NoteID,NoteDetails,CreatedBy,CreatedOn,AuditAction,AuditStatus,AccountID,AddToContactSummary,NoteCategory) 
SELECT NoteID,NoteDetails,CreatedBy,CreatedOn,'I',1, AccountID,AddToContactSummary,NoteCategory FROM Inserted


INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate], AccountID, EntityName)
	 SELECT NoteID, CreatedBy,6, 1, GETUTCDATE(), AccountID, NoteDetails FROM Inserted



GO


CREATE TRIGGER [dbo].[tr_Notes_Update] ON [dbo].[Notes] 
FOR UPDATE AS INSERT INTO Notes_Audit(NoteID,NoteDetails,CreatedBy,CreatedOn,AuditAction,AuditStatus,AccountID,AddToContactSummary,NoteCategory) 
SELECT NoteID, NoteDetails, CreatedBy, CreatedOn, 'U', 1, AccountID,AddToContactSummary,NoteCategory FROM Inserted

 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID,EntityName)
	 SELECT NoteID, CreatedBy, 6, 3, GETUTCDATE(), AccountID, NoteDetails FROM Inserted



GO

CREATE TRIGGER [dbo].[tr_Notes_Delete] ON [dbo].[Notes]
FOR DELETE AS 
	--INSERT INTO Notes_Audit(NoteID,NoteDetails,CreatedBy,CreatedOn,AuditAction,AuditStatus)
	--SELECT NoteID,NoteDetails,CreatedBy,CreatedOn,'D',0 FROM Deleted

	UPDATE [dbo].[Notes_Audit]
		SET [AuditStatus] = 0
		FROM [dbo].[Notes_Audit] NA INNER JOIN Deleted D ON NA.[NoteID] = D.NoteID

--INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate])
--	 SELECT NoteID, CreatedBy, 6, 4, GETUTCDATE() FROM Deleted
