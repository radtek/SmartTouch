CREATE TABLE [dbo].[SubscriptionModuleMap] (
    [SubscriptionModuleMapID] INT            IDENTITY (1, 1) NOT NULL,
    [SubscriptionID]          TINYINT        NOT NULL,
    [ModuleID]                TINYINT        NOT NULL,
    [AccountID]               INT            NULL,
    [Limit]                   INT            NULL,
    [ExcludedRoles]           NVARCHAR (100) NULL,
    CONSTRAINT [PK_SubscriptionModuleMap] PRIMARY KEY CLUSTERED ([SubscriptionModuleMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_SubscriptionModuleMap_Modules] FOREIGN KEY ([ModuleID]) REFERENCES [dbo].[Modules] ([ModuleID]),
    CONSTRAINT [FK_SubscriptionModuleMap_Subscriptions] FOREIGN KEY ([SubscriptionID]) REFERENCES [dbo].[Subscriptions] ([SubscriptionID])
);

