CREATE TABLE [dbo].[PushNotifications] (
    [PushNotificationID] INT            IDENTITY (1, 1) NOT NULL,
    [Device]             VARCHAR (50)   NULL,
    [SubscriptionID]     NVARCHAR (MAX) NULL,
    [AccountID]          INT            NOT NULL,
    [UserId]             INT            NOT NULL,
    [Allow]              BIT            NOT NULL,
    [CreatedDate]        DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([PushNotificationID] ASC)
);

