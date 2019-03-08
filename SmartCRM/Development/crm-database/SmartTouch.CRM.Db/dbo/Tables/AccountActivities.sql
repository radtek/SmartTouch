CREATE TABLE [dbo].[AccountActivities] (
    [AccountsActivityID] INT      IDENTITY (1, 1) NOT NULL,
    [AccountID]          INT      NULL,
    [ActiveUsers]        INT      NULL,
    [LastCampaignSent]   DATETIME NULL,
    [LastLogin]          DATETIME NULL,
    CONSTRAINT [PK_AccountsActivity] PRIMARY KEY CLUSTERED ([AccountsActivityID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_AccountsActivities_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID])
);
GO
