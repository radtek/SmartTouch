CREATE TYPE [dbo].[LeadAdapterDetails] AS TABLE (
    [SubmittedData]             NVARCHAR (MAX)   NULL,
    [RowData]                   NVARCHAR (MAX)   NULL,
    [ReferenceID]               UNIQUEIDENTIFIER NULL,
    [LeadAdapterRecordStatusID] TINYINT          NULL,
    [Remarks]                   NVARCHAR (500)   NULL,
    [CreatedBy]                 INT              NULL,
    [CreatedDateTime]           DATETIME         NULL,
    [LeadAdapterJobLogID]       INT              NULL,
    [Id]                        INT              NULL,
    [LeadAdapterRecordStatus]   NVARCHAR (500)   NULL);

