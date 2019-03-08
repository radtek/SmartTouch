CREATE TABLE [dbo].[SubscriptionSettings] (
    [SubscriptionSettingsID] INT            IDENTITY (1, 1) NOT NULL,
    [SubscriptionID]         TINYINT        NOT NULL,
    [Name]                   TINYINT        NOT NULL,
    [Value]                  NVARCHAR (50)  NULL,
    [LastModifiedBy]         INT            NOT NULL,
    [LastModifiedOn]         DATETIME       NOT NULL,
    [Description]            NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_SubscriptionSettings] PRIMARY KEY CLUSTERED ([SubscriptionSettingsID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_SubscriptionSettings_Subscriptions] FOREIGN KEY ([SubscriptionID]) REFERENCES [dbo].[Subscriptions] ([SubscriptionID]),
    CONSTRAINT [FK_SubscriptionSettings_Users] FOREIGN KEY ([LastModifiedBy]) REFERENCES [dbo].[Users] ([UserID])
);

