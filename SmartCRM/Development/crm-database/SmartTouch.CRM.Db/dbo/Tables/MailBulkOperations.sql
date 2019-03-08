CREATE TABLE [dbo].[MailBulkOperations](
	[MailBulkOperationID] [int] IDENTITY(1,1) NOT NULL,
	[From] [varchar](500) NULL,
	[To] [text] NULL,
	[CC] [text] NULL,
	[BCC] [text] NULL,
	[Subject] [nvarchar](4000) NULL,
	[Body] [nvarchar](max) NULL,
	CONSTRAINT [PK_MailBulkOperations] PRIMARY KEY CLUSTERED ([MailBulkOperationID] ASC) WITH (FILLFACTOR = 90) ON [PRIMARY]
	)
	

GO

