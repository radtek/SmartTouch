CREATE TABLE [dbo].[LeadAdapterJobLogs] (
    [LeadAdapterJobLogID]        INT            IDENTITY (1, 1) NOT NULL,
    [LeadAdapterAndAccountMapID] INT            NOT NULL,
    [StartDate]                  DATETIME       NULL,
    [EndDate]                    DATETIME       NULL,
    [LeadAdapterJobStatusID]     TINYINT        NOT NULL,
    [Remarks]                    NVARCHAR (500) NULL,
    [FileName]                   NVARCHAR (200) NOT NULL,
    [CreatedBy]                  INT            NOT NULL,
    [CreatedDateTime]            DATETIME       NOT NULL,
    [StorageName]                NVARCHAR (300) NULL,
	ProcessedFileName            VARCHAR(50) NULL,
    CONSTRAINT [PK_LeadAdapterJobLogs] PRIMARY KEY CLUSTERED ([LeadAdapterJobLogID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_LeadAdapterJobLogs_LeadAdapterAndAccountMap] FOREIGN KEY ([LeadAdapterAndAccountMapID]) REFERENCES [dbo].[LeadAdapterAndAccountMap] ([LeadAdapterAndAccountMapID]),
    CONSTRAINT [FK_LeadAdapterJobLogs_LeadAdapterJobStatus] FOREIGN KEY ([LeadAdapterJobStatusID]) REFERENCES [dbo].[LeadAdapterJobStatus] ([LeadAdapterJobStatusID])
);


GO

CREATE NONCLUSTERED INDEX [IX_LeadAdapterJobLogs_LeadAdapterAndAccountMapID]
    ON [dbo].[LeadAdapterJobLogs]([LeadAdapterAndAccountMapID] ASC)
    INCLUDE([LeadAdapterJobLogID], [StartDate], [EndDate], [LeadAdapterJobStatusID], [Remarks], [FileName], [CreatedBy], [CreatedDateTime], [StorageName]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LeadAdapterJobLogs_LeadAdapterAndAccountMapID_LeadAdapterJobStatusID]
    ON [dbo].[LeadAdapterJobLogs]([LeadAdapterAndAccountMapID] ASC, [LeadAdapterJobStatusID] ASC)
    INCLUDE([LeadAdapterJobLogID], [Remarks], [FileName], [CreatedDateTime]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LeadAdapterJobLogs_LeadAdapterJobStatusID]
    ON [dbo].[LeadAdapterJobLogs]([LeadAdapterJobStatusID] ASC)
    INCLUDE([LeadAdapterAndAccountMapID]) WITH (FILLFACTOR = 90);

