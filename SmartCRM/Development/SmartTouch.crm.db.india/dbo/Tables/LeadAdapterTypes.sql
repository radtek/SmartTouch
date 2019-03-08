CREATE TABLE [dbo].[LeadAdapterTypes] (
    [LeadAdapterTypeID]              TINYINT       IDENTITY (1, 1) NOT NULL,
    [Name]                           NVARCHAR (75) NOT NULL,
    [LeadAdapterCommunicationTypeID] TINYINT       NOT NULL,
    CONSTRAINT [PK_LeadAdapterTypes] PRIMARY KEY CLUSTERED ([LeadAdapterTypeID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_LeadAdapterTypes_LeadAdapterCommunicationTypes] FOREIGN KEY ([LeadAdapterCommunicationTypeID]) REFERENCES [dbo].[LeadAdapterCommunicationTypes] ([LeadAdapterCommunicationTypeID])
);

