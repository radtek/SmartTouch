CREATE TABLE [dbo].[ContactEmails_Audit] (
    [AuditId]        BIGINT        IDENTITY (1, 1) NOT NULL,
    [ContactEmailID] INT           NOT NULL,
    [ContactID]      INT           NOT NULL,
    [Email]          VARCHAR (256) NOT NULL,
    [EmailStatus]    SMALLINT      NOT NULL,
    [IsPrimary]      BIT           NOT NULL,
    [AccountID]      INT           NOT NULL,
    [SnoozeUntil]    DATETIME      NULL,
    [IsDeleted]      BIT           NOT NULL,
    [AuditAction]    CHAR (1)      NOT NULL,
    [AuditDate]      DATETIME      CONSTRAINT [DF_ContactEmails_Audit_AuditDate] DEFAULT (getutcdate()) NOT NULL,
    [AuditUser]      VARCHAR (50)  CONSTRAINT [DF_ContactEmails_Audit_AuditUser] DEFAULT (suser_sname()) NOT NULL,
    [AuditApp]       VARCHAR (128) CONSTRAINT [DF_ContactEmails_Audit_AuditApp] DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') NULL,
    [AuditStatus]    BIT           NOT NULL
);
go



CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-ContactEmails_Audit] ON [dbo].[ContactEmails_Audit] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [AccountId_Scheme_UserActivityLogs]([AccountID])
go