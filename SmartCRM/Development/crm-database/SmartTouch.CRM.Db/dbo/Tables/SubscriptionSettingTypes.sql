CREATE TABLE [dbo].[SubscriptionSettingTypes] (
    [SubscriptionSettingTypeID] INT           IDENTITY (1, 1) NOT NULL,
    [Name]                      TINYINT       NOT NULL,
    [Value]                     VARCHAR (100) NOT NULL,
    [Description]               VARCHAR (128) NOT NULL,
    CONSTRAINT [PK_SubscriptionSettingTypes] PRIMARY KEY CLUSTERED ([SubscriptionSettingTypeID] ASC) WITH (FILLFACTOR = 90)
);

