CREATE TABLE [dbo].[LeadAdapterRecordStatus] (
    [LeadAdapterRecordStatusID] TINYINT       IDENTITY (1, 1) NOT NULL,
    [Title]                     NVARCHAR (75) NOT NULL,
    CONSTRAINT [PK_LeadAdapterRecordStatus] PRIMARY KEY CLUSTERED ([LeadAdapterRecordStatusID] ASC) WITH (FILLFACTOR = 90)
);

