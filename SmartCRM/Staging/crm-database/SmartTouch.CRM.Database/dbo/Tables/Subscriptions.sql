CREATE TABLE [dbo].[Subscriptions] (
    [SubscriptionID]   TINYINT       IDENTITY (1, 1) NOT NULL,
    [SubscriptionName] NVARCHAR (75) NOT NULL,
    CONSTRAINT [PK_Subscriptions] PRIMARY KEY CLUSTERED ([SubscriptionID] ASC) WITH (FILLFACTOR = 90)
);

