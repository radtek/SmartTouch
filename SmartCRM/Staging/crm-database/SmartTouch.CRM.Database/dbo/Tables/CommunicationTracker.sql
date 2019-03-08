CREATE TABLE [dbo].[CommunicationTracker] (
    [CommunicationTrackerID] BIGINT           IDENTITY (1, 1) NOT NULL,
    [CommunicationTypeID]    TINYINT          NOT NULL,
    [Address]                BIT              NULL,
    [ContactID]              INT              NOT NULL,
    [TrackerGuid]            UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]            DATETIME         NOT NULL,
    [CommunicationStatusID]  TINYINT          NOT NULL,
    CONSTRAINT [PK_CommunicationInformation] PRIMARY KEY CLUSTERED ([CommunicationTrackerID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CommunicationInformation_CommunicationInformation] FOREIGN KEY ([CommunicationStatusID]) REFERENCES [dbo].[CommunicationStatus] ([CommunicationStatusID]),
    CONSTRAINT [FK_CommunicationTracker_CommunicationTypes] FOREIGN KEY ([CommunicationTypeID]) REFERENCES [dbo].[CommunicationTypes] ([CommunicationTypeID]),
    CONSTRAINT [FK_CommunicationTracker_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID])
);

