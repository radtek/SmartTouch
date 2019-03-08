CREATE TABLE [dbo].[NoteTagMap_Audit] (
    [AuditId]      BIGINT        IDENTITY (1, 1) NOT NULL,
    [NoteTagMapID] INT           NOT NULL,
    [NoteID]       INT           NOT NULL,
    [TagID]        INT           NOT NULL,
    [AuditAction]  CHAR (1)      NOT NULL,
    [AuditDate]    DATETIME      CONSTRAINT [DF_NoteTagMap_Audit_AuditDate] DEFAULT (getutcdate()) NOT NULL,
    [AuditUser]    VARCHAR (50)  CONSTRAINT [DF_NoteTagMap_Audit_AuditUser] DEFAULT (suser_sname()) NOT NULL,
    [AuditApp]     VARCHAR (128) CONSTRAINT [DF_NoteTagMap_Audit_AuditApp] DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') NULL,
    [AuditStatus]  BIT           NOT NULL,
    CONSTRAINT [PK_NoteTagMap_Audit] PRIMARY KEY CLUSTERED ([AuditId] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY]
);

