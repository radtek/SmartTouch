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
    CONSTRAINT [PK_LeadAdapterJobLogDetails] PRIMARY KEY CLUSTERED ([LeadAdapterJobLogDetailID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON),
    CONSTRAINT [FK_LeadAdapterJobLogDetails_LeadAdapterJobLogs] FOREIGN KEY ([LeadAdapterJobLogID]) REFERENCES [dbo].[LeadAdapterJobLogs] ([LeadAdapterJobLogID]),
    CONSTRAINT [FK_LeadAdapterJobLogDetails_LeadAdapterRecordStatus] FOREIGN KEY ([LeadAdapterRecordStatusID]) REFERENCES [dbo].[LeadAdapterRecordStatus] ([LeadAdapterRecordStatusID])
);


GO
CREATE NONCLUSTERED INDEX [IX_LeadAdapterJobLogDetails_ReferenceID]
    ON [dbo].[LeadAdapterJobLogDetails]([ReferenceID] ASC)
    INCLUDE([LeadAdapterJobLogDetailID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_LeadAdapterJobLogDetails_LeadAdapterJobLogID_LeadAdapterRecordStatusID]
    ON [dbo].[LeadAdapterJobLogDetails]([LeadAdapterJobLogID] ASC, [LeadAdapterRecordStatusID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_LeadAdapterJobLogDetails_LeadAdapterJobLogID_LeadAdapterRecordStatusID_ReferenceID]
    ON [dbo].[LeadAdapterJobLogDetails]([LeadAdapterJobLogID] ASC, [LeadAdapterRecordStatusID] ASC)
    INCLUDE([ReferenceID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [LeadAdapterJobLogDetails_LeadAdapterRecordStatusID_LeadAdapterJobLogID]
    ON [dbo].[LeadAdapterJobLogDetails]([LeadAdapterRecordStatusID] ASC)
    INCLUDE([LeadAdapterJobLogID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

