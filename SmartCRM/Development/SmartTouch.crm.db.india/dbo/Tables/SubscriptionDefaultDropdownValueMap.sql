CREATE TABLE [dbo].[SubscriptionDefaultDropdownValueMap] (
    [SubscriptionDefaultDropdownValueMapID] INT      IDENTITY (1, 1) NOT NULL,
    [SubscriptionID]                        TINYINT  NULL,
    [DropdownValueID]                       SMALLINT NULL,
    CONSTRAINT [PK_SubscriptionDefaultDropdownValueMap] PRIMARY KEY CLUSTERED ([SubscriptionDefaultDropdownValueMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_SubscriptionDefaultDropdownValueMap_DropdownValues] FOREIGN KEY ([DropdownValueID]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_SubscriptionDefaultDropdownValueMap_Subscriptions] FOREIGN KEY ([SubscriptionID]) REFERENCES [dbo].[Subscriptions] ([SubscriptionID])
);

