
CREATE TABLE [dbo].[Notes_Audit](
	[AuditId] [bigint] IDENTITY(1,1) NOT NULL,
	[NoteID] [int] NOT NULL,
	[NoteDetails] [nvarchar](4000) NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[AuditAction] [char](1) NOT NULL,
	[AuditDate] [datetime] NOT NULL CONSTRAINT [DF_Notes_Audit_AuditDate]  DEFAULT (getutcdate()),
	[AuditUser] [varchar](50) NOT NULL CONSTRAINT [DF_Notes_Audit_AuditUser]  DEFAULT (suser_sname()),
	[AuditApp] [varchar](128) NULL CONSTRAINT [DF_Notes_Audit_AuditApp]  DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') '),
	[AuditStatus] [bit] NOT NULL,
	[AccountID] [int] NULL,
	[AddToContactSummary] [bit] NOT NULL DEFAULT ('0'),
	[NoteCategory] SMALLINT  NOT NULL  DEFAULT(13216),
	CONSTRAINT [PK_Notes_Audit] PRIMARY KEY CLUSTERED ([AuditId] ASC) WITH (FILLFACTOR = 90)
);
GO


CREATE NONCLUSTERED INDEX [IX_Notes_Audit_AccountID]
    ON [dbo].[Notes_Audit]([AccountID] ASC)
    INCLUDE([NoteID], [NoteDetails], [CreatedBy], [AuditAction], [AuditDate]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO

CREATE NONCLUSTERED INDEX [IX_Notes_Audit_NoteID_AccountID]
    ON [dbo].[Notes_Audit]([NoteID] ASC, [AccountID] ASC)
    INCLUDE([NoteDetails], [CreatedBy], [AuditAction], [AuditDate]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];

GO

Create NonClustered Index IX_Notes_Audit_missing_1096 On [dbo].[Notes_Audit] ([NoteID], [AccountID]) Include ([NoteDetails], [CreatedBy], [AuditAction], [AuditDate], [AddToContactSummary]);
GO

