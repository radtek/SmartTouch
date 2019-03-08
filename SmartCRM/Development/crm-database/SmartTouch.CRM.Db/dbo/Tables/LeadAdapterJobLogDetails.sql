CREATE TABLE [dbo].[LeadAdapterJobLogDetails] (
    [LeadAdapterJobLogDetailID] INT              IDENTITY (1, 1) NOT NULL,
    [LeadAdapterJobLogID]       INT              NOT NULL,
    [LeadAdapterRecordStatusID] TINYINT          NOT NULL,
    [Remarks]                   NVARCHAR (MAX)   NULL,
    [CreatedBy]                 INT              NOT NULL,
    [CreatedDateTime]           DATETIME         NOT NULL,
    [RowData]                   NVARCHAR (4000)  NULL,
    [ReferenceID]               UNIQUEIDENTIFIER NULL,
    [SubmittedData]             NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_LeadAdapterJobLogDetails] PRIMARY KEY CLUSTERED ([LeadAdapterJobLogDetailID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_LeadAdapterJobLogDetails_LeadAdapterJobLogs] FOREIGN KEY ([LeadAdapterJobLogID]) REFERENCES [dbo].[LeadAdapterJobLogs] ([LeadAdapterJobLogID]),
    CONSTRAINT [FK_LeadAdapterJobLogDetails_LeadAdapterRecordStatus] FOREIGN KEY ([LeadAdapterRecordStatusID]) REFERENCES [dbo].[LeadAdapterRecordStatus] ([LeadAdapterRecordStatusID])
);


GO
CREATE NONCLUSTERED INDEX [IX_LeadAdapterJobLogDetails_LeadAdapterJobLogID]
    ON [dbo].[LeadAdapterJobLogDetails]([LeadAdapterJobLogID] ASC)
    INCLUDE([ReferenceID], [SubmittedData]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LeadAdapterJobLogDetails_LeadAdapterJobLogID_LeadAdapterRecordStatusID]
    ON [dbo].[LeadAdapterJobLogDetails]([LeadAdapterJobLogID] ASC, [LeadAdapterRecordStatusID] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LeadAdapterJobLogDetails_LeadAdapterJobLogID_LeadAdapterRecordStatusID_ReferenceID]
    ON [dbo].[LeadAdapterJobLogDetails]([LeadAdapterJobLogID] ASC, [LeadAdapterRecordStatusID] ASC)
    INCLUDE([ReferenceID]) WITH (FILLFACTOR = 90);


GO

CREATE NONCLUSTERED INDEX [IX_LeadAdapterJobLogDetails_ReferenceID]
    ON [dbo].[LeadAdapterJobLogDetails]([ReferenceID] ASC)
    INCLUDE([LeadAdapterJobLogDetailID]) WITH (FILLFACTOR = 90);
GO


CREATE NONCLUSTERED INDEX [IX_LeadAdapterJobLogDetails_missing_96] ON [dbo].[LeadAdapterJobLogDetails]
(
	[LeadAdapterRecordStatusID] ASC
)
INCLUDE ( 	[RowData]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
