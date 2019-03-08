CREATE TABLE [dbo].[Notes_Audit] (
    [AuditId]             BIGINT          IDENTITY (1, 1) NOT NULL,
    [NoteID]              INT             NOT NULL,
    [NoteDetails]         NVARCHAR (4000) NOT NULL,
    [CreatedBy]           INT             NOT NULL,
    [CreatedOn]           DATETIME        NOT NULL,
    [AuditAction]         CHAR (1)        NOT NULL,
    [AuditDate]           DATETIME        CONSTRAINT [DF_Notes_Audit_AuditDate] DEFAULT (getutcdate()) NOT NULL,
    [AuditUser]           VARCHAR (50)    CONSTRAINT [DF_Notes_Audit_AuditUser] DEFAULT (suser_sname()) NOT NULL,
    [AuditApp]            VARCHAR (128)   CONSTRAINT [DF_Notes_Audit_AuditApp] DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') NULL,
    [AuditStatus]         BIT             NOT NULL,
    [AccountID]           INT             NULL,
    [AddToContactSummary] BIT             CONSTRAINT [DF__Notes_Aud__AddTo__24B338F0] DEFAULT ('0') NOT NULL,
    [NoteCategory]        SMALLINT        CONSTRAINT [DF__Notes_Aud__NoteC__4B62F1BD] DEFAULT ((13216)) NOT NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_Notes_Audit_missing_1096]
    ON [dbo].[Notes_Audit]([NoteID] ASC, [AccountID] ASC)
    INCLUDE([NoteDetails], [CreatedBy], [AuditAction], [AuditDate], [AddToContactSummary]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_UserActivityLogs] ([AccountID]);


GO
CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex_Notes_Audit]
    ON [dbo].[Notes_Audit]
    ON [AccountId_Scheme_UserActivityLogs] ([AccountID]);

