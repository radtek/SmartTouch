CREATE TABLE [dbo].[UserAuditTypes] (
    [UserAuditTypeID]   TINYINT        IDENTITY (1, 1) NOT NULL,
    [UserAuditTypeName] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_UserAuditTypes] PRIMARY KEY CLUSTERED ([UserAuditTypeID] ASC) WITH (FILLFACTOR = 90)
);

