CREATE TABLE [dbo].[AVColumnPreferences] (
    [AVColumnPreferenceID] INT     IDENTITY (1, 1) NOT NULL,
    [EntityID]             INT     NOT NULL,
    [EntityType]           TINYINT NOT NULL,
    [FieldID]              INT     NOT NULL,
    [FieldType]            TINYINT NOT NULL,
    [ShowingType]          TINYINT NOT NULL,
    CONSTRAINT [PK_ContactsViewPreference ] PRIMARY KEY CLUSTERED ([AVColumnPreferenceID] ASC) WITH (FILLFACTOR = 90)
);

