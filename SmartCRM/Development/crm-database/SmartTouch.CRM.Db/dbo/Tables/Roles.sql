CREATE TABLE [dbo].[Roles] (
    [RoleID]         SMALLINT      IDENTITY (1, 1) NOT NULL,
    [RoleName]       NVARCHAR (75) NOT NULL,
    [AccountID]      INT           NULL,
    [SubscriptionID] TINYINT       NULL,
    [IsDeleted]      BIT           NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([RoleID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Roles_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_Roles_Subscriptions] FOREIGN KEY ([SubscriptionID]) REFERENCES [dbo].[Subscriptions] ([SubscriptionID])
);

