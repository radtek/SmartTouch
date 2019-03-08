CREATE TABLE [dbo].[SearchDefinitionSubscriptionMap] (
    [SearchDefinitionSubscriptionMapID] INT     IDENTITY (1, 1) NOT NULL,
    [SubscriptionID]                    TINYINT NOT NULL,
    [SearchDefinitionID]                INT     NOT NULL,
    CONSTRAINT [PK_SearchDefinitionSubscriptionMap] PRIMARY KEY CLUSTERED ([SearchDefinitionSubscriptionMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_SearchDefinitionSubscriptionMap_SearchDefinitions] FOREIGN KEY ([SearchDefinitionID]) REFERENCES [dbo].[SearchDefinitions] ([SearchDefinitionID]),
    CONSTRAINT [FK_SearchDefinitionSubscriptionMap_Subscriptions] FOREIGN KEY ([SubscriptionID]) REFERENCES [dbo].[Subscriptions] ([SubscriptionID])
);

