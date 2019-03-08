CREATE TABLE [dbo].[CommunicationTypes] (
    [CommunicationTypeID]   TINYINT       IDENTITY (1, 1) NOT NULL,
    [CommunicationTypeName] NVARCHAR (75) NULL,
    CONSTRAINT [PK_CommunicationTypes] PRIMARY KEY CLUSTERED ([CommunicationTypeID] ASC) WITH (FILLFACTOR = 90)
);

