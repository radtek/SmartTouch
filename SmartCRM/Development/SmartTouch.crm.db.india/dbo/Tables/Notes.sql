

CREATE TABLE [dbo].[Notes](
	[NoteID] [int] IDENTITY(1,1) NOT NULL,
	[NoteDetails] [nvarchar](max) NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[AccountID] [int] NULL,
	[SelectAll] [bit] NULL,
	[AddToContactSummary] [bit] NOT NULL CONSTRAINT [DF_AddToContactSummary]  DEFAULT ((0)),
	[NoteCategory] SMALLINT  NOT NULL  DEFAULT(13216),
	CONSTRAINT [PK_Notes] PRIMARY KEY CLUSTERED ([NoteID] ASC) WITH (FILLFACTOR = 90),
	CONSTRAINT [FK_Notes_DropdownValues] FOREIGN KEY ([NoteCategory]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID])
	);
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

Create NonClustered Index IX_Notes_missing_185 On [dbo].[Notes] ([CreatedBy], [AddToContactSummary]) Include ([NoteID], [NoteDetails], [CreatedOn], [NoteCategory]);
GO
