CREATE TABLE [dbo].[Tags_Audit] (
    [AuditId]     BIGINT         IDENTITY (1, 1) NOT NULL,
    [TagID]       INT            NOT NULL,
    [TagName]     NVARCHAR (150) NOT NULL,
    [Description] NVARCHAR (200) NULL,
    [AccountID]   INT            NULL,
    [Count]       INT            NULL,
    [CreatedBy]   INT            NULL,
    [AuditAction] CHAR (1)       NOT NULL,
    [AuditDate]   DATETIME       CONSTRAINT [DF_Tags_Audit_AuditDate] DEFAULT (getutcdate()) NOT NULL,
    [AuditUser]   VARCHAR (50)   CONSTRAINT [DF_Tags_Audit_AuditUser] DEFAULT (suser_sname()) NOT NULL,
    [AuditApp]    VARCHAR (128)  CONSTRAINT [DF_Tags_Audit_AuditApp] DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') NULL,
    [AuditStatus] BIT            NOT NULL,
    [IsDeleted]   BIT            NULL,
    CONSTRAINT [PK_Tags_Audit] PRIMARY KEY CLUSTERED ([AuditId] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
);


GO
CREATE NONCLUSTERED INDEX [IX_Tags_Audit_TagID_AuditAction]
    ON [dbo].[Tags_Audit]([TagID] ASC, [AuditAction] ASC)
    INCLUDE([AuditDate]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];

