CREATE TABLE [dbo].[ActionsMailOperations](
	[ActionsMailOperationID] [int] IDENTITY(1,1) NOT NULL,
	[ActionID] [int] NOT NULL,
	[IsScheduled] [bit] NOT NULL,
	[IsProcessed] [tinyint] NOT NULL,
	[MailBulkOperationID] [int] NOT NULL,
	GroupID UNIQUEIDENTIFIER NULL ,
	CONSTRAINT [PK_ActionsMailOperations] PRIMARY KEY CLUSTERED ([ActionsMailOperationID] ASC) WITH (FILLFACTOR = 90) ON [PRIMARY],
	CONSTRAINT [FK_ActionsMailOperations_MailBulkOperations] FOREIGN KEY ([MailBulkOperationID]) REFERENCES [dbo].[MailBulkOperations] ([MailBulkOperationID])
	)

GO

