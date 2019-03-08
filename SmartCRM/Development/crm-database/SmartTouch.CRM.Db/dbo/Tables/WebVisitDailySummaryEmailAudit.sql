CREATE TABLE [dbo].[WebVisitDailySummaryEmailAudit] (
    [WebVisitDailySummaryEmailAuditID] INT            IDENTITY (1, 1) NOT NULL,
    [WebAnalyticsProviderID]           SMALLINT       NOT NULL,
    [UserID]                           INT            NOT NULL,
    [StatusID]                         SMALLINT       NOT NULL,
    [SentOn]                           DATETIME       NULL,
    [JobID]                            VARCHAR (150)  NULL,
    [Recipients]                       NVARCHAR (MAX) NULL,
    [Remarks]                          NVARCHAR (300) NULL,
    CONSTRAINT [PK_WebVisitDailySummaryEmailAudit] PRIMARY KEY CLUSTERED ([WebVisitDailySummaryEmailAuditID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WebVisitDailySummaryEmailAudit_Statuses] FOREIGN KEY ([StatusID]) REFERENCES [dbo].[Statuses] ([StatusID]),
    CONSTRAINT [FK_WebVisitDailySummaryEmailAudit_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_WebVisitDailySummaryEmailAudit_WebAnalyticsProviders] FOREIGN KEY ([WebAnalyticsProviderID]) REFERENCES [dbo].[WebAnalyticsProviders] ([WebAnalyticsProviderID])
);

