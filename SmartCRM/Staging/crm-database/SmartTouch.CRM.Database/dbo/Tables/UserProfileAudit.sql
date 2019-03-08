CREATE TABLE [dbo].[UserProfileAudit] (
    [UserProfileAuditID] INT            IDENTITY (1, 1) NOT NULL,
    [UserID]             INT            NOT NULL,
    [UserAuditTypeID]    TINYINT        NOT NULL,
    [AuditedOn]          DATETIME       NOT NULL,
    [Password]           NVARCHAR (100) NULL,
    [AuditedBy]          INT            NOT NULL,
    CONSTRAINT [PK_UserProfileAudit] PRIMARY KEY CLUSTERED ([UserProfileAuditID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_UserProfileAudit_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);

