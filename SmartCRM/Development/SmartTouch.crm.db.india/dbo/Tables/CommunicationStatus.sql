CREATE TABLE [dbo].[CommunicationStatus] (
    [CommunicationStatusID] TINYINT       IDENTITY (1, 1) NOT NULL,
    [StatusName]            NVARCHAR (75) NOT NULL,
    CONSTRAINT [PK_CommunicationStatus] PRIMARY KEY CLUSTERED ([CommunicationStatusID] ASC) WITH (FILLFACTOR = 90)
);

