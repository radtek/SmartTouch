CREATE TABLE [dbo].[AccountActivities_old] (
    [AccountsActivityID] INT      IDENTITY (1, 1) NOT NULL,
    [AccountID]          INT      NULL,
    [AccountCreatedDate] DATETIME NULL,
    [ActiveUsers]        INT      NULL,
    [LastUpdatedOn]      DATETIME NULL,
    [SubscriptionID]     INT      NULL,
    [LastCampaignSent]   DATETIME NULL,
    CONSTRAINT [PK_AccountsActivity_old] PRIMARY KEY CLUSTERED ([AccountsActivityID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_AccountsActivities_old_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID])
);

