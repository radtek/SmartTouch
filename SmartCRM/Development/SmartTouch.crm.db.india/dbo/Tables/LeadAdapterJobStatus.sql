CREATE TABLE [dbo].[LeadAdapterJobStatus] (
    [LeadAdapterJobStatusID] TINYINT       IDENTITY (1, 1) NOT NULL,
    [Title]                  NVARCHAR (75) NOT NULL,
    CONSTRAINT [PK_LeadAdapterJobStatus] PRIMARY KEY CLUSTERED ([LeadAdapterJobStatusID] ASC) WITH (FILLFACTOR = 90)
);

