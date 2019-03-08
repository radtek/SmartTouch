CREATE TABLE [dbo].[LoginAudit] (
    [LoginAuditID]   INT           IDENTITY (1, 1) NOT NULL,
    [UserID]         INT           NOT NULL,
    [SignInActivity] TINYINT       NOT NULL,
    [IPAddress]      NVARCHAR (50) NOT NULL,
    [AuditedOn]      DATETIME      NOT NULL,
    [AccountID]      INT           NULL,
    CONSTRAINT [PK_LoginAudit] PRIMARY KEY CLUSTERED ([LoginAuditID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON),
    FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_LoginAudit_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_LoginAudit_UserID_AuditedOn]
    ON [dbo].[LoginAudit]([UserID] ASC, [AccountID] ASC)
    INCLUDE([AuditedOn]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

