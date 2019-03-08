CREATE TABLE [dbo].[LeadAdapterCommunicationTypes] (
    [LeadAdapterCommunicationTypeID] TINYINT       IDENTITY (1, 1) NOT NULL,
    [Title]                          NVARCHAR (75) NOT NULL,
    CONSTRAINT [PK_LeadAdapterCommunicationTypes] PRIMARY KEY CLUSTERED ([LeadAdapterCommunicationTypeID] ASC) WITH (FILLFACTOR = 90)
);

