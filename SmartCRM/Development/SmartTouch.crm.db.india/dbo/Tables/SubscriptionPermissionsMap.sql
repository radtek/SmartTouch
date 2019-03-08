CREATE TABLE [dbo].[SubscriptionPermissionsMap] (
    [SubscriptionPermissionsMapID] INT     IDENTITY (1, 1) NOT NULL,
    [SubscriptionID]               TINYINT NOT NULL,
    [ModuleOperationsMapID]        INT     NOT NULL,
    CONSTRAINT [PK_SubscriptionPermissionsMap] PRIMARY KEY CLUSTERED ([SubscriptionPermissionsMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_SubscriptionPermissionsMap_ModuleOperationsMap] FOREIGN KEY ([ModuleOperationsMapID]) REFERENCES [dbo].[ModuleOperationsMap] ([ModuleOperationsMapID]),
    CONSTRAINT [FK_SubscriptionPermissionsMap_Subscriptions] FOREIGN KEY ([SubscriptionID]) REFERENCES [dbo].[Subscriptions] ([SubscriptionID])
);

