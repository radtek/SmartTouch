CREATE TABLE [dbo].[WebVisitEmailAudit] (
    [AuditID]        INT            IDENTITY (1, 1) NOT NULL,
    [Recipients]     NVARCHAR (500) NULL,
    [EmailStatus]    SMALLINT       NOT NULL,
    [SentDate]       DATETIME       NULL,
    [VisitReference] NVARCHAR (50)  NOT NULL,
    [CreatedOn]      DATETIME       NOT NULL,
    [JobID]          VARCHAR (150)  NULL,
    [Remarks]        NVARCHAR (300) NULL,
CONSTRAINT [PK_WebVisitEmailAudit] PRIMARY KEY CLUSTERED ([AuditID] ASC) WITH (FILLFACTOR = 90)
);


GO
CREATE NONCLUSTERED INDEX [IX_WebVisitEmailAudit_EmailStatus]
    ON [dbo].[WebVisitEmailAudit]([EmailStatus] ASC)
    INCLUDE([CreatedOn]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_WebVisitEmailAudit_VisitReference]
    ON [dbo].[WebVisitEmailAudit]([VisitReference] ASC) WITH (FILLFACTOR = 90);

