CREATE TABLE [dbo].[DailySummaryEmailAudit] (
    [DailySummaryEmailAuditID] INT      IDENTITY (1, 1) NOT NULL,
    [UserID]                   INT      NOT NULL,
    [AuditedOn]                DATETIME NOT NULL,
    [Status]                   TINYINT  NOT NULL,
    CONSTRAINT [PK_DailySummaryEmailAudit] PRIMARY KEY CLUSTERED ([DailySummaryEmailAuditID] ASC) WITH (FILLFACTOR = 90)
);

