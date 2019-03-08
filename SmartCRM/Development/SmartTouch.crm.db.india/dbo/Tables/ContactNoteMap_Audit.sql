CREATE TABLE [dbo].[ContactNoteMap_Audit] (
    [AuditId]          BIGINT        IDENTITY (1, 1) NOT NULL,
    [ContactNoteMapID] INT           NOT NULL,
    [NoteID]           INT           NOT NULL,
    [ContactID]        INT           NOT NULL,
    [AuditAction]      CHAR (1)      NOT NULL,
    [AuditDate]        DATETIME      CONSTRAINT [DF_ContactNoteMap_Audit_AuditDate] DEFAULT (getutcdate()) NOT NULL,
    [AuditUser]        VARCHAR (50)  CONSTRAINT [DF_ContactNoteMap_Audit_AuditUser] DEFAULT (suser_sname()) NOT NULL,
    [AuditApp]         VARCHAR (128) CONSTRAINT [DF_ContactNoteMap_Audit_AuditApp] DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') NULL,
    [AuditStatus]      BIT           NOT NULL,
    CONSTRAINT [PK_ContactNoteMap_Audit] PRIMARY KEY CLUSTERED ([AuditId] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY]
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactNoteMap_Audit_ContactID]
    ON [dbo].[ContactNoteMap_Audit]([ContactID] ASC) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_ContactNoteMap_Audit_ContactNoteMapID]
    ON [dbo].[ContactNoteMap_Audit]([ContactNoteMapID] ASC)
    INCLUDE([AuditId]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_ContactNoteMap_Audit_NoteID]
    ON [dbo].[ContactNoteMap_Audit]([NoteID] ASC) WITH (FILLFACTOR = 90)
    ON [SECONDARY];

